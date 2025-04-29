using BepInEx.Configuration;
using REPOLib.Patches;

namespace REPOLib;

internal static class ConfigManager
{
    public static ConfigFile? ConfigFile { get; private set; }

    public static ConfigEntry<bool>? ExtendedLogging { get; private set; }
    public static ConfigEntry<bool>? DeveloperMode { get; private set; }
    public static ConfigEntry<bool>? VanillaDeveloperMode { get; private set; }

    public static void Initialize(ConfigFile configFile)
    {
        ConfigFile = configFile;
        
        ExtendedLogging =      ConfigFile.Bind("General", "ExtendedLogging",      defaultValue: false, "Enable extended logging.");
        DeveloperMode =        ConfigFile.Bind("General", "DeveloperMode",        defaultValue: false, "Enable developer mode cheats for testing.");
        VanillaDeveloperMode = ConfigFile.Bind("General", "VanillaDeveloperMode", defaultValue: false, "Enable vanilla developer mode cheats for testing.");

        VanillaDeveloperMode.SettingChanged += (_, _) => SteamManagerPatch.UpdateDeveloperMode(SteamManager.instance);
    }
}
