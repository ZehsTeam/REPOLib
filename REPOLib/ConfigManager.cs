using BepInEx.Configuration;

namespace REPOLib;

internal static class ConfigManager
{
    public static ConfigFile ConfigFile { get; private set; }

    public static ConfigEntry<bool> ExtendedLogging { get; private set; }

    public static void Initialize(ConfigFile configFile)
    {
        ConfigFile = configFile;
        BindConfigs();
    }

    private static void BindConfigs()
    {
        ExtendedLogging = ConfigFile.Bind("General", "ExtendedLogging", defaultValue: false, "Enable extended logging.");
    }
}
