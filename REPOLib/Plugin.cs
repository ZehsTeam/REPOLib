using BepInEx;
using HarmonyLib;
using REPOLib.Commands;
using REPOLib.Patches;

namespace REPOLib;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    private readonly Harmony _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

    public static Plugin Instance { get; private set; }
    
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

        ConfigManager.Initialize(Config);

        if (ConfigManager.DeveloperMode.Value)
        {
            _harmony.PatchAll(typeof(SteamManagerPatch));
        }

        BundleLoader.LoadAllBundles(Paths.PluginPath, ".repobundle");
    }
}
