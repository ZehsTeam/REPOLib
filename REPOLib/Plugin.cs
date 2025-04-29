using BepInEx;
using HarmonyLib;
using REPOLib.Patches;

namespace REPOLib;

/// <summary>
///     The Plugin class of REPOLib.
/// </summary>
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    private readonly Harmony _harmony = new(MyPluginInfo.PLUGIN_GUID);

    /// <summary>
    ///     The REPOLib plugin instance.
    /// </summary>
    public static Plugin Instance { get; private set; } = null!;

    private void Awake()
    {
        Instance = this;

        REPOLib.Logger.Initialize(BepInEx.Logging.Logger.CreateLogSource(MyPluginInfo.PLUGIN_GUID));
        REPOLib.Logger.LogInfo($"{MyPluginInfo.PLUGIN_NAME} has awoken!");

        this._harmony.PatchAll(typeof(RunManagerPatch));
        this._harmony.PatchAll(typeof(EnemyDirectorPatch));
        this._harmony.PatchAll(typeof(StatsManagerPatch));
        this._harmony.PatchAll(typeof(SemiFuncPatch));
        this._harmony.PatchAll(typeof(AudioManagerPatch));
        this._harmony.PatchAll(typeof(SteamManagerPatch));
        this._harmony.PatchAll(typeof(EnemyGnomeDirectorPatch));
        this._harmony.PatchAll(typeof(EnemyBangDirectorPatch));
        this._harmony.PatchAll(typeof(PlayerControllerPatch));

        ConfigManager.Initialize(this.Config);
        BundleLoader.LoadAllBundles(Paths.PluginPath, ".repobundle");
    }
}