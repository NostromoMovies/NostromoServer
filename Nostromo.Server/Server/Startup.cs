using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nostromo.Server.Services;
using Nostromo.Server.Utilities;
using Nostromo.Server.Scheduling;
using Nostromo.Server.Settings;
using Nostromo.Server.Database;
using Nostromo.Server.Database.Repositories;
using Nostromo.Server.Scheduling.Jobs;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using System.Net.Http.Headers;
using System.Security.Claims;
using Nostromo.Server.API.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Quartz.AspNetCore;

namespace Nostromo.Server.Server
{
    // WebStartup class is now correctly using _configuration.
    public class WebStartup
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;

        // Inject IConfiguration here
        public WebStartup(IWebHostEnvironment env, IConfiguration configuration)
        {
            _env = env;
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<WatcherSettings>(_configuration.GetSection("WatcherSettings"));

            // API Controllers
            services.AddControllers();

            // Swagger documentation
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Nostromo API",
                    Version = "v1",
                    Description = "API endpoints for Nostromo Server"
                });
            });

            // CORS if needed for development
            services.AddCors(options =>
            {
                options.AddPolicy("Development", builder =>
                {
                    builder.WithOrigins("http://localhost:5173")
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseCors("Development");
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Nostromo API V1");
                c.RoutePrefix = "swagger";
            });

            string _serverProjectPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../..", "Nostromo.Server"));
            var webuiPath = Path.Combine(_serverProjectPath, "webui");

            // Move the static files middleware BEFORE routing
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(webuiPath),
                RequestPath = "/webui",
                ServeUnknownFileTypes = true,
                OnPrepareResponse = ctx =>
                {
                    Console.WriteLine($"Attempting to serve static file: {ctx.File.PhysicalPath}");
                }
            });

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                // Only use fallback for non-file routes
                endpoints.MapFallbackToFile("/webui/{**path}", "/index.html", new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(webuiPath)
                });
            });
        }
    }

    // Main Startup class
    public class Startup
    {
        private IServiceProvider _serviceProvider;
        private IConfiguration _configuration; // Declare _configuration here
        private readonly ILogger _logger;
        private readonly ISettingsProvider _settingsProvider;
        private IWebHost _webHost;

        public Startup(ILogger<Startup> logger, ISettingsProvider settingsProvider, IConfiguration configuration)
        {
            _logger = logger;
            _settingsProvider = settingsProvider;
            _configuration = configuration; // Assign the configuration
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Add logging
            services.AddLogging(builder =>
            {
                builder.AddConfiguration(_configuration.GetSection("Logging"));
                builder.AddConsole();
                builder.AddDebug();
            });

            services.AddSingleton(_configuration);

            // Register the database context and services
            services.AddNostromoDatabase(_configuration);
            services.AddHttpClient();
            services.AddHttpContextAccessor();

            // Register DatabaseService
            services.AddScoped<IDatabaseService, DatabaseService>();

            // Register the repository (FileSystemRepository)
            services.AddScoped<IFileSystemRepository, FileSystemRepository>();

            // Register the FileWatcherService
            services.AddSingleton<FileWatcherService>();

            // Configure WatcherSettings from configuration
            services.Configure<WatcherSettings>(_configuration.GetSection("WatcherSettings"));

            // Add Quartz services for scheduled jobs
            services.AddQuartz(q =>
            {
                q.UseMicrosoftDependencyInjectionJobFactory();
            });

            services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);


            // Register Quartz IScheduler
            services.AddSingleton(provider =>
            {
                var schedulerFactory = provider.GetRequiredService<ISchedulerFactory>();
                return schedulerFactory.GetScheduler().Result;
            });

            // Register TMDB API settings and add necessary services
            var tmdbApiKey = _configuration.GetValue<string>("TMDB:ApiKey", "cbd64d95c4c66beed284bd12701769ec");
            var tmdbBaseUrl = _configuration.GetValue<string>("TMDB:BaseUrl", "https://api.themoviedb.org/3");

            if (tmdbApiKey == "cbd64d95c4c66beed284bd12701769ec" || tmdbBaseUrl == "https://api.themoviedb.org/3")
            {
                _logger.LogWarning("TMDB configuration is using default values. Ensure appsettings.json is correctly configured.");
            }

            // Register controllers (this will automatically pick up FolderController)
            services.AddControllers();

            // Register settings provider and core services
            services.AddSingleton<ISettingsProvider>(_settingsProvider);

            // Register the NostromoServer singleton
            services.AddSingleton<NostromoServer>();
        }

        public async Task Start()
        {
            try
            {
                // Create service collection
                var services = new ServiceCollection();
                ConfigureServices(services);

                // Build service provider
                _serviceProvider = services.BuildServiceProvider();

                // Initialize database
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<NostromoDbContext>();
                    await dbContext.Database.MigrateAsync();
                }

                // Start web host
                if (!await StartWebHost(_settingsProvider))
                {
                    throw new Exception("Failed to start web host");
                }

                // Set the static service provider
                Utils.ServiceContainer = _serviceProvider;

                // Get and start the server
                var nostromoServer = _serviceProvider.GetRequiredService<NostromoServer>();
                Utils.NostromoServer = nostromoServer;

                // Start the file watcher service
                var fileWatcherService = _serviceProvider.GetRequiredService<FileWatcherService>();
                await fileWatcherService.StartWatchingAsync(CancellationToken.None);

                if (!nostromoServer.StartUpServer())
                {
                    throw new Exception("Failed to start Nostromo server");
                }

                _logger.LogInformation("Nostromo server started successfully");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to start application: {Error}", e.Message);
                throw;
            }
        }

        private async Task<bool> StartWebHost(ISettingsProvider settingsProvider)
        {
            try
            {
                _webHost ??= InitWebHost(settingsProvider);
                await _webHost.StartAsync();
                _logger.LogInformation("Web host started successfully on port {Port}", settingsProvider.GetSettings().ServerPort);
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to start web host: {Error}", e.Message);
                await StopHost();
                return false;
            }
        }

        private IWebHost InitWebHost(ISettingsProvider settingsProvider)
        {
            if (_webHost != null) return _webHost;

            var settings = settingsProvider.GetSettings();
            var serverProjectPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../..", "Nostromo.Server"));

            var builder = new WebHostBuilder()
                .UseKestrel(options =>
                {
                    options.ListenAnyIP(settings.ServerPort);
                })
                .UseWebRoot(Path.Combine(serverProjectPath, "webui"))  // Add this line
                .ConfigureServices(services =>
                {
                    // Share the configuration
                    services.AddSingleton(_configuration);
                    // Share the settings provider
                    services.AddSingleton(settingsProvider);
                    // Share other core services
                    ConfigureServices(services);
                })
                .UseStartup<WebStartup>();

            var result = builder.Build();

            Utils.SettingsProvider = result.Services.GetRequiredService<ISettingsProvider>();
            Utils.ServiceContainer = result.Services;

            return result;
        }

        private async Task StopHost()
        {
            if (_webHost is IAsyncDisposable disp)
            {
                await disp.DisposeAsync();
            }
            else
            {
                _webHost?.Dispose();
            }
            _webHost = null;
        }
    }
}
