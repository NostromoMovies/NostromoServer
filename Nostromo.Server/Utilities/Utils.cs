using Nostromo.Server.Server;
using Nostromo.Server.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
            var p = (int)Environment.OSVersion.Platform;
            return p == 4 || p == 6 || p == 128;
        }
    }

    public static string DefaultInstance { get; set; } = Assembly.GetEntryAssembly().GetName().Name;
}
