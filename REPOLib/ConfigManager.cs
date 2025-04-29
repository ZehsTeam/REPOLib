using System;
using BepInEx.Configuration;
using REPOLib.Patches;

namespace REPOLib;

internal static class ConfigManager
{
    private static ConfigFile? _configFile;
    private static ConfigEntry<bool>? _extendedLogging;
    private static ConfigEntry<bool>? _developerMode;
    private static ConfigEntry<bool>? _vanillaDeveloperMode;

    public static ConfigFile ConfigFile
    {
        get => _configFile ??
               throw new NullReferenceException(
                   $"{nameof(ConfigFile)} cannot be null. Did you forget to call {nameof(Initialize)}?");
        private set => _configFile = value;
    }

    public static ConfigEntry<bool> ExtendedLogging
    {
        get => _extendedLogging ??
               throw new NullReferenceException(
                   $"{nameof(ExtendedLogging)} cannot be null. Did you forget to call {nameof(Initialize)}?");
        private set => _extendedLogging = value;
    }

    public static ConfigEntry<bool> DeveloperMode
    {
        get => _developerMode ??
               throw new NullReferenceException(
                   $"{nameof(DeveloperMode)} cannot be null. Did you forget to call {nameof(Initialize)}?");
        private set => _developerMode = value;
    }

    public static ConfigEntry<bool> VanillaDeveloperMode
    {
        get => _vanillaDeveloperMode ??
               throw new NullReferenceException(
                   $"{nameof(VanillaDeveloperMode)} cannot be null. Did you forget to call {nameof(Initialize)}?");
        private set => _vanillaDeveloperMode = value;
    }

    public static void Initialize(ConfigFile configFile)
    {
        ConfigFile = configFile;

        ExtendedLogging = ConfigFile.Bind("General", "ExtendedLogging", false, "Enable extended logging.");
        DeveloperMode = ConfigFile.Bind("General", "DeveloperMode", false, "Enable developer mode cheats for testing.");
        VanillaDeveloperMode = ConfigFile.Bind("General", "VanillaDeveloperMode", false,
            "Enable vanilla developer mode cheats for testing.");
        VanillaDeveloperMode.SettingChanged += (_, _) => SteamManagerPatch.UpdateDeveloperMode(SteamManager.instance);
    }
}