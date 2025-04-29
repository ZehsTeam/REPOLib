using HarmonyLib;
using REPOLib.Commands;
using REPOLib.Modules;

namespace REPOLib.Patches;

[HarmonyPatch(typeof(RunManager))]
internal static class RunManagerPatch
{
    private static bool _patchedAwake;

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(nameof(RunManager.Awake))]
    private static void AwakePatch(RunManager __instance)
    {
        if (_patchedAwake) return;
        _patchedAwake = true;

        NetworkPrefabs.Initialize();
        NetworkingEvents.Initialize();
        Levels.RegisterInitialLevels();
        Valuables.RegisterInitialValuables();

        BundleLoader.OnAllBundlesLoaded += CommandManager.Initialize;
        BundleLoader.FinishLoadOperations(__instance);
    }
}