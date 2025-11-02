using HarmonyLib;
using REPOLib.Modules;

namespace REPOLib.Patches;

[HarmonyPatch(typeof(RunManager))]
internal static class RunManagerPatch
{
    private static bool _patchedAwake = false;

    [HarmonyPatch(nameof(RunManager.Awake))]
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    private static void AwakePatch(RunManager __instance)
    {
        if (_patchedAwake) return;
        _patchedAwake = true;

        Valuables.RegisterInitialValuables();
        BundleLoader.FinishLoadOperations(__instance);
    }
}
