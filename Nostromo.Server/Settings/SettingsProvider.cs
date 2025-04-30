using FluentNHibernate.Utils;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Nostromo.Server.Utilities;
using Formatting = Newtonsoft.Json.Formatting;
using JsonConverter = Newtonsoft.Json.JsonConverter;

namespace Nostromo.Server.Settings;
public class SettingsProvider : ISettingsProvider
{
    private readonly ILogger<SettingsProvider> _logger;
    private const string SettingsFilename = "server-settings.json";
    private static readonly object SettingsLock = new();

    private static IServerSettings Instance { get; set; }

    public SettingsProvider(ILogger<SettingsProvider> logger)
    {
        _logger = logger;
    }

    public IServerSettings GetSettings(bool copy = false)
    {
        if (Instance == null) LoadSettings();
        if (copy) return Instance.DeepClone();
        return Instance;
    }

    public void LoadSettings()
    {
        var appPath = Utils.ApplicationPath;
        if (!Directory.Exists(appPath))
            Directory.CreateDirectory(appPath);

        var settingsPath = Path.Combine(appPath, SettingsFilename);
        if (!File.Exists(settingsPath))
            Instance = new ServerSettings();
    }

    public void SaveSettings(IServerSettings settings)
    {
        if (Instance == null)
        {
            _logger.LogWarning("Tried to save settings, but the settings were null");
            return;
        }

        var settingsPath = Path.Combine(Utils.ApplicationPath, SettingsFilename);

        lock (SettingsLock)
        {
            var onDisk = File.Exists(settingsPath) ? File.ReadAllText(settingsPath) : string.Empty;
            var inCode = Serialize(Instance, true);
            if (!onDisk.Equals(inCode, StringComparison.Ordinal))
            {
                File.WriteAllText(settingsPath, inCode);
                //TODO: raise settings event?
            }
        }
    }

    public static string Serialize(object obj, bool indent = false)
    {
        var serializerSettings = new JsonSerializerSettings
        {
            Formatting = indent ? Formatting.Indented : Formatting.None,
            DefaultValueHandling = DefaultValueHandling.Include,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            Converters = new List<JsonConverter> { new StringEnumConverter() }
        };
        return JsonConvert.SerializeObject(obj, serializerSettings);
    }

    public void SaveSettings()
    {
        throw new NotImplementedException();
    }

    public void DebugSettingsToLog()
    {
        throw new NotImplementedException();
    }
}
