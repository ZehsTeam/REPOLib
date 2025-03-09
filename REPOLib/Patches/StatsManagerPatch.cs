using HarmonyLib;
using REPOLib.Modules;

namespace REPOLib.Patches;

[HarmonyPatch(typeof(StatsManager))]
internal static class StatsManagerPatch
{
    [HarmonyPatch(nameof(StatsManager.Start))]
    [HarmonyPostfix]
    private static void StartPatch()
    {
        Logger.LogInfo("\n\n\n\nStatsManager.Start();\n\n\n");

        Items.RegisterItems();
    }
}
