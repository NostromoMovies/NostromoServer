// See https://aka.ms/new-console-template for more information

using Nostromo.Server.Server;
public static class Program
{
    private static ILogger _logger = null;
    public static async Task Main()
    {
        try
        {
            //var startup = new Startup();
            //await startup.Start();
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Server failed to start"); 
        }
    }
}