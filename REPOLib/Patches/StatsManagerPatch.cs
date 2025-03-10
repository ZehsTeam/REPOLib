using HarmonyLib;
using REPOLib.Modules;

namespace REPOLib.Patches;

[HarmonyPatch(typeof(StatsManager))]
internal static class StatsManagerPatch
{
    [HarmonyPatch(nameof(StatsManager.RunStartStats))]
    [HarmonyPostfix]
    private static void RunStartStatsPatch()
    {
        Items.RegisterItems();
    }
}
