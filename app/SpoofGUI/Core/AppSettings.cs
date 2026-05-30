using SpoofGUI.Database;

namespace SpoofGUI.Core;

public sealed class AppSettings
{
    private const string AllowInsecureKey = "xray_allow_insecure";
    private const string LogLevelKey = "xray_log_level";
    private const string V2RayModeKey = "default_import_mode";
    private const string CheckUpdatesOnLaunchKey = "check_updates_on_launch";

    private static readonly string[] ValidLogLevels = ["none", "error", "warning", "info", "debug"];
    private static readonly string[] ValidModes = ["Proxy", "Tunnel", "SystemProxy"];

    private readonly SettingsRepository _settings;
    public AppSettings(SettingsRepository settings) => _settings = settings;

    public bool XrayAllowInsecure
    {
        get => ReadBool(AllowInsecureKey, false);
        set => _settings.Set(AllowInsecureKey, value ? "1" : "0");
    }

    public string XrayLogLevel
    {
        get
        {
            var value = _settings.Get(LogLevelKey);
            return value is not null && ValidLogLevels.Contains(value) ? value : "warning";
        }
        set => _settings.Set(LogLevelKey, ValidLogLevels.Contains(value) ? value : "warning");
    }

    public string V2RayMode
    {
        get
        {
            var value = _settings.Get(V2RayModeKey);
            return value is not null && ValidModes.Contains(value) ? value : "Proxy";
        }
        set => _settings.Set(V2RayModeKey, ValidModes.Contains(value) ? value : "Proxy");
    }

    public bool CheckUpdatesOnLaunch
    {
        get => ReadBool(CheckUpdatesOnLaunchKey, false);
        set => _settings.Set(CheckUpdatesOnLaunchKey, value ? "1" : "0");
    }

    private bool ReadBool(string key, bool fallback)
    {
        var value = _settings.Get(key);
        if (value is null) return fallback;
        return value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase);
    }
}
