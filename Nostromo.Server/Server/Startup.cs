using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nostromo.Server.Services;
using Nostromo.Server.Utilities;
using Nostromo.Server.Utilities.FileSystemWatcher;
using Nostromo.Server.Scheduling;
using Nostromo.Server.Settings;
using Microsoft.AspNetCore.Hosting;

namespace Nostromo.Server.Server;

public class Startup
{
    private IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
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

        // Configure WatcherSettings from configuration
        services.Configure<WatcherSettings>(_configuration.GetSection("WatcherSettings"));

        // Register FileSystemWatcher
        //TODO: this needs to be multiple paths taken from configured Drop Folders
        services.AddSingleton<RecoveringFileSystemWatcher>(sp =>
        {
            var watchPath = _configuration.GetValue<string>("WatchSettings:Path")
                ?? throw new InvalidOperationException("Watch path not configured");
            return new RecoveringFileSystemWatcher(watchPath);
        });

        // Add Quartz services
        services.AddQuartzServices();

        // Register core services
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

            // Set the static service provider (though this isn't ideal, keeping it for compatibility)
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
        }
        catch (Exception e)
        {
            var logger = _serviceProvider?.GetService<ILogger<Startup>>();
            logger?.LogError(e, "Failed to start application");
            throw;
        }
    }
}