using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Nostromo.Server.Server;
using Nostromo.Server.Utilities;
using Nostromo.Server.Settings;
using System;

namespace Nostromo.CliService
{
    internal class Program
    {
        private static ILogger _logger;

        public static void Main(string[] args)
        {
            // Initialize Logger
            var logFactory = LoggerFactory.Create(o => o.AddNLog());
            _logger = logFactory.CreateLogger("Main");

            try
            {
                // Initialize Settings Provider
                var settingsProvider = new SettingsProvider(logFactory.CreateLogger<SettingsProvider>());
                Utils.SettingsProvider = settingsProvider;

                // Start Server
                var startup = new Startup(logFactory.CreateLogger<Startup>(), settingsProvider);
                startup.Start().ConfigureAwait(false).GetAwaiter().GetResult();

                // CLI Command Loop
                Console.WriteLine("Nostromo Server started. Type 'exit' to shut down.");
                while (true)
                {
                    var command = Console.ReadLine()?.Trim();
                    if (string.Equals(command, "exit", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("Shutting down...");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Failed to start server");
                Console.WriteLine("Critical error occurred. Check logs for details.");
            }
            finally
            {
                // Ensure proper shutdown
                _logger?.LogInformation("Server is shutting down.");
            }
        }
    }
}
