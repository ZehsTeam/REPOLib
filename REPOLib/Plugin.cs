using BepInEx;
using HarmonyLib;
using REPOLib.Patches;

namespace REPOLib;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    private readonly Harmony _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

    public static Plugin Instance { get; private set; }
    
    private void Awake()
    {
        Instance = this;

        REPOLib.Logger.Initialize(BepInEx.Logging.Logger.CreateLogSource(MyPluginInfo.PLUGIN_GUID));
        REPOLib.Logger.LogInfo($"{MyPluginInfo.PLUGIN_NAME} has awoken!");

        _harmony.PatchAll(typeof(RunManagerPatch));
        _harmony.PatchAll(typeof(EnemyDirectorPatch));

        ConfigManager.Initialize(Config);
    }
}
