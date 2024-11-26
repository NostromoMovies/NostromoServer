using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nostromo.Server.Services;
using Nostromo.Server.Utilities;
using Nostromo.Server.Utilities.FileSystemWatcher;
using Nostromo.Server.Scheduling;
using Nostromo.Server.Settings;
using Nostromo.Server.Database;
using Nostromo.Server.Database.Repositories;

namespace Nostromo.Server.Server;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.Net.Http.Headers;

public class WebStartup
{
    private readonly IWebHostEnvironment _env;

    public WebStartup(IWebHostEnvironment env)
    {
        _env = env;
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
        services.AddHttpContextAccessor();

        // Register DatabaseService
        services.AddScoped<IDatabaseService, DatabaseService>();

        // Configure WatcherSettings from configuration
        services.Configure<WatcherSettings>(_configuration.GetSection("WatcherSettings"));

        // Register FileSystemWatcher
        services.AddSingleton<RecoveringFileSystemWatcher>(sp =>
        {
            var watchPath = _configuration.GetValue<string>("WatchSettings:Path")
                ?? throw new InvalidOperationException("Watch path not configured");
            return new RecoveringFileSystemWatcher(watchPath);
        });

        // Add Quartz services
        services.AddQuartzServices();

        // Register core services
        services.AddSingleton<ISettingsProvider>(_settingsProvider);

        services.AddHttpClient<ITmdbService, TmdbService>(client => {
            // This configures the HttpClient that will be injected
            client.BaseAddress = new Uri("https://api.themoviedb.org/3/");
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        });

        services.AddSingleton<FileWatcherService>();
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