﻿using BepInEx;
using HarmonyLib;
using REPOLib.Modules;
using REPOLib.Patches;

namespace REPOLib;

/// <summary>
/// The Plugin class of REPOLib.
/// </summary>
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    private readonly Harmony _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

    /// <summary>
    /// The REPOLib plugin instance.
    /// </summary>
    public static Plugin Instance { get; private set; } = null!;

    #pragma warning disable IDE0051 // Remove unused private members
    private void Awake()
    #pragma warning restore IDE0051 // Remove unused private members
    {
        Instance = this;

        REPOLib.Logger.Initialize(BepInEx.Logging.Logger.CreateLogSource(MyPluginInfo.PLUGIN_GUID));
        REPOLib.Logger.LogInfo($"{MyPluginInfo.PLUGIN_NAME} has awoken!");

        _harmony.PatchAll(typeof(RunManagerPatch));
        _harmony.PatchAll(typeof(EnemyDirectorPatch));
        _harmony.PatchAll(typeof(StatsManagerPatch));
        _harmony.PatchAll(typeof(SemiFuncPatch));
        _harmony.PatchAll(typeof(AudioManagerPatch));
        _harmony.PatchAll(typeof(SteamManagerPatch));
        _harmony.PatchAll(typeof(EnemyGnomeDirectorPatch));
        _harmony.PatchAll(typeof(EnemyBangDirectorPatch));
        _harmony.PatchAll(typeof(PlayerControllerPatch));
        _harmony.PatchAll(typeof(DebugCommandHandlerPatch));

        ConfigManager.Initialize(Config);

        Upgrades.Initialize();
        Modules.Commands.Initialize();

        BundleLoader.LoadAllBundles(Paths.PluginPath, ".repobundle");
    }
}
