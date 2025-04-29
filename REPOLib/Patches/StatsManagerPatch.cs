using HarmonyLib;
using REPOLib.Modules;

namespace REPOLib.Patches;

[HarmonyPatch(typeof(StatsManager))]
internal static class StatsManagerPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(StatsManager.RunStartStats))]
    private static void RunStartStatsPatch(StatsManager __instance)
    {
        Items.RegisterItems();
        Upgrades.RegisterUpgrades(__instance);
    }
}
