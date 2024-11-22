using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nostromo.Server.Services;
using Nostromo.Server.Utilities;
using Nostromo.Server.Utilities.FileSystemWatcher;
using Nostromo.Server.Scheduling;
using Nostromo.Server.Settings;
using Microsoft.AspNetCore.Hosting;
using Nostromo.Server.Database;
using Nostromo.Server.Database.Repositories;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.FileProviders;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace Nostromo.Server.Server
{
    public class WebStartup
    {
        private readonly string _indexPath;

        public WebStartup(ISettingsProvider settingsProvider)
        {
            // Retrieve IndexPath from settings provider and resolve to an absolute path
            _indexPath = Path.GetFullPath(settingsProvider.GetSettings().IndexPath);
        }

        public void ConfigureServices(IServiceCollection services)
        {
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

            // CORS for development
            services.AddCors(options =>
            {
                options.AddPolicy("Development", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Verify the directory exists
            if (!Directory.Exists(_indexPath))
            {
                throw new DirectoryNotFoundException($"Static file directory not found: {_indexPath}");
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseCors("Development");
            }

            // Swagger setup
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Nostromo API V1");
                c.RoutePrefix = "swagger";
            });

            app.UseRouting();

            // Serve static files using the IndexPath
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(_indexPath),
                RequestPath = ""
            });

            // SPA fallback
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }

    public class Startup
    {
        private IServiceProvider _serviceProvider;
        private IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly ISettingsProvider _settingsProvider;
        private IWebHost _webHost;

        public Startup(ILogger<Startup> logger, ISettingsProvider settingsProvider)
        {
            _logger = logger;
            _settingsProvider = settingsProvider;

            // Build configuration
            _configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Logging
            services.AddLogging(builder =>
            {
                builder.AddConfiguration(_configuration.GetSection("Logging"));
                builder.AddConsole();
                builder.AddDebug();
            });

            services.AddNostromoDatabase(_configuration);
            services.AddHttpClient();

            // DatabaseService
            services.AddScoped<IDatabaseService, DatabaseService>();

            // WatcherSettings
            services.Configure<WatcherSettings>(_configuration.GetSection("WatcherSettings"));

            // FileSystemWatcher
            services.AddSingleton<RecoveringFileSystemWatcher>(sp =>
            {
                var watchPath = _configuration.GetValue<string>("WatchSettings:Path")
                    ?? throw new InvalidOperationException("Watch path not configured");
                return new RecoveringFileSystemWatcher(watchPath);
            });

            // Quartz services
            services.AddQuartzServices();

            // Core services
            services.AddSingleton<FileWatcherService>();
            services.AddSingleton<NostromoServer>();
        }

        public async Task Start()
        {
            try
            {
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

                Utils.ServiceContainer = _serviceProvider;

                var nostromoServer = _serviceProvider.GetRequiredService<NostromoServer>();
                Utils.NostromoServer = nostromoServer;

                var fileWatcherService = _serviceProvider.GetRequiredService<FileWatcherService>();
                await fileWatcherService.StartAsync(CancellationToken.None);

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

            var builder = new WebHostBuilder()
                .UseKestrel(options =>
                {
                    options.ListenAnyIP(settingsProvider.GetSettings().ServerPort);
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton(_configuration);
                    services.AddSingleton(settingsProvider);
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