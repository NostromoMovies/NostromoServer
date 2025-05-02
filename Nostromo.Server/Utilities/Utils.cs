using Nostromo.Server.Server;
using Nostromo.Server.Settings;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Nostromo.Server.Utilities;

public static class Utils
{
    public static NostromoServer NostromoServer { get; set; }
    public static IServiceProvider ServiceContainer { get; set; }
    public static ISettingsProvider SettingsProvider { get; set; }

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

    public static bool IsLinux =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    public static string? GetMediaTypeFromFileName(string filePath)
    {
        string fileName = Path.GetFileName(filePath).ToLowerInvariant();

        var tvPatterns = new[]
        {
        @"s\d{1,2}e\d{1,2}",         // S01E02
        @"\d{1,2}x\d{2}",            // 1x02
        @"season\s?\d{1,2}",         // Season 1
        @"s\d{1,2}\s?e\d{1,2}",      // S1 E2
        @"\bs\d{1,2}\se\d{1,2}\b"    // s4 e6
    };

        foreach (var pattern in tvPatterns)
        {
            if (Regex.IsMatch(fileName, pattern, RegexOptions.IgnoreCase))
                return "TV";
        }

        return "Movie";
    }

    public static string DefaultInstance { get; set; } = Assembly.GetEntryAssembly().GetName().Name;
}