using Nostromo.Server.Server;
using Nostromo.Server.Settings;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Nostromo.Server.Utilities;

public static class Utils
{
    public static NostromoServer NostromoServer { get; set; }
    public static IServiceProvider ServiceContainer { get; set; }
    public static ISettingsProvider SettingsProvider { get; set; }

    //settings provider

    private static string _applicationPath = null;

    public static string ApplicationPath
    {
        get
        {
            if (_applicationPath != null)
                return _applicationPath;

            if (IsLinux)
                return _applicationPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nostromo",
                    DefaultInstance);

            return _applicationPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                DefaultInstance);
        }
    }



public static bool IsLinux
{
    get
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    }
}

public static string DefaultInstance { get; set; } = Assembly.GetEntryAssembly().GetName().Name;
}
