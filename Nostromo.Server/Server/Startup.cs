using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Nostromo.Server.Services;
using Nostromo.Server.Utilities;

namespace Nostromo.Server.Server;

public class Startup
{
    public static void ConfigureServices(IServiceCollection services)
    {
        // Register existing services
        services.AddSingleton<NostromoServer>();
        services.AddSingleton<FileWatcherService>();

        // Register file watcher services
        //services.AddSingleton<MultiFolderWatcher>();
        //services.Configure<WatcherSettings>(
            //configuration.GetSection("Watcher"));
        services.AddHostedService<FileWatcherService>();
    }

    public async Task Start()
    {
        try
        {
            var nostromoServer = Utils.ServiceContainer.GetRequiredService<NostromoServer>();
            Utils.NostromoServer = nostromoServer;

            // Any additional startup logic
        }
        catch (Exception e)
        {
            //log exception
            throw;
        }
    }
}