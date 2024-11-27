// Startup.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nostromo.Server.Database;
using Nostromo.Server.Scheduling.Jobs;
using Nostromo.Server.Services;
using Nostromo.Server.Settings;
using Nostromo.Server.Utilities;
using Nostromo.Server.Utilities.FileSystemWatcher;
using Quartz;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using Nostromo.Server.API.Authentication;
using System.Security.Claims;
using Microsoft.OpenApi.Models;

namespace Nostromo.Server.Server
{
    public class Startup
    {
        private IServiceProvider _serviceProvider;
        private IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly ISettingsProvider _settingsProvider;
        private IWebHost _webHost;
        private readonly IServiceCollection _services;

        public Startup(ILogger<Startup> logger, ISettingsProvider settingsProvider)
        {
            _logger = logger;
            _settingsProvider = settingsProvider;
            _services = new ServiceCollection();

            // Build configuration
            _configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();
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

            services.AddNostromoDatabase(_configuration);
            services.AddHttpClient();

            // add authentication
            services.AddAuthentication("ApiKey")
                .AddScheme<AuthenticationSchemeOptions, CustomAuthHandler>("ApiKey", null);

            // add authorization
            services.AddAuthorization(options =>
            {
                options.AddPolicy("admin", policy =>
                    policy.RequireClaim(ClaimTypes.Role, "admin"));
            });

            services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.ApiKey,
                    In = ParameterLocation.Header,
                    Name = "apikey",
                    Description = "API Key can be provided in header or query parameter"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "ApiKey"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            // Register core services
            services.AddScoped<IDatabaseService, DatabaseService>();
            services.AddSingleton<ISettingsProvider>(_settingsProvider);
            services.AddSingleton<FileWatcherService>();
            services.AddSingleton<NostromoServer>();

            // Configure WatcherSettings
            services.Configure<WatcherSettings>(_configuration.GetSection("WatcherSettings"));

            // Register FileSystemWatcher
            services.AddSingleton<RecoveringFileSystemWatcher>(sp =>
            {
                var watchPath = _configuration.GetValue<string>("WatchSettings:Path")
                    ?? throw new InvalidOperationException("Watch path not configured");
                return new RecoveringFileSystemWatcher(watchPath);
            });

            // Add Quartz services
            services.AddQuartz();

            // TMDB Configuration
            var tmdbApiKey = _configuration.GetValue<string>("TMDB:ApiKey", "cbd64d95c4c66beed284bd12701769ec");
            var tmdbBaseUrl = _configuration.GetValue<string>("TMDB:BaseUrl", "https://api.themoviedb.org/3");

            if (tmdbApiKey == "cbd64d95c4c66beed284bd12701769ec" || tmdbBaseUrl == "https://api.themoviedb.org/3")
            {
                _logger.LogWarning("TMDB configuration is using default values. Ensure appsettings.json is correctly configured.");
            }

            services.AddTransient(sp => new DownloadMovieMetadataJob(
                sp.GetRequiredService<ILogger<DownloadMovieMetadataJob>>(),
                sp.GetRequiredService<HttpClient>(),
                sp.GetRequiredService<IDatabaseService>(),
                tmdbApiKey,
                tmdbBaseUrl
            ));

            services.AddHttpClient<ITmdbService, TmdbService>(client => {
                client.BaseAddress = new Uri("https://api.themoviedb.org/3/");
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
            });
        }

        public async Task Start()
        {
            try
            {
                ConfigureServices(_services);
                _serviceProvider = _services.BuildServiceProvider();

                // Initialize database
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<NostromoDbContext>();
                    await dbContext.Database.MigrateAsync();
                }

                if (!await StartWebHost())
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

        private async Task<bool> StartWebHost()
        {
            try
            {
                _webHost ??= InitWebHost();
                await _webHost.StartAsync();
                _logger.LogInformation("Web host started successfully on port {Port}",
                    _settingsProvider.GetSettings().ServerPort);
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to start web host: {Error}", e.Message);
                await StopHost();
                return false;
            }
        }

        private IWebHost InitWebHost()
        {
            if (_webHost != null) return _webHost;

            var settings = _settingsProvider.GetSettings();
            var serverProjectPath = Path.GetFullPath(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../..", "Nostromo.Server"));

            var builder = new WebHostBuilder()
                .UseKestrel(options =>
                {
                    options.ListenAnyIP(settings.ServerPort);
                })
                .UseWebRoot(Path.Combine(serverProjectPath, "webui"))
                .UseStartup<WebStartup>()
                .ConfigureServices(services =>
                {
                    // Copy core services to the web host
                    foreach (var descriptor in _services)
                    {
                        services.Add(descriptor);
                    }
                });

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