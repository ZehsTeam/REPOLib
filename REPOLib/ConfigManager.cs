using BepInEx.Configuration;
using REPOLib.Patches;

namespace REPOLib;

internal static class ConfigManager
{
    public static ConfigFile ConfigFile { get; private set; } = null!;

    public static ConfigEntry<bool> ExtendedLogging { get; private set; } = null!;
    public static ConfigEntry<bool> DeveloperMode { get; private set; } = null!;
    public static ConfigEntry<bool> VanillaDeveloperMode { get; private set; } = null!;

    public static void Initialize(ConfigFile configFile)
    {
        ConfigFile = configFile;
        BindConfigs();
    }

    private static void BindConfigs()
    {
        ExtendedLogging =      ConfigFile.Bind("General", "ExtendedLogging",      defaultValue: false, "Enable extended logging.");
        DeveloperMode =        ConfigFile.Bind("General", "DeveloperMode",        defaultValue: false, "Enable developer mode cheats for testing.");
        VanillaDeveloperMode = ConfigFile.Bind("General", "VanillaDeveloperMode", defaultValue: false, "Enable vanilla developer mode cheats for testing.");

        VanillaDeveloperMode.SettingChanged += (object sender, System.EventArgs e) => SteamManagerPatch.UpdateDeveloperMode();
    }
}
