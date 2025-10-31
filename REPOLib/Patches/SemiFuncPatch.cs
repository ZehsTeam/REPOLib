using HarmonyLib;
using REPOLib.Modules;

namespace REPOLib.Patches;

[HarmonyPatch(typeof(SemiFunc))]
internal static class SemiFuncPatch
{
    [HarmonyPatch(nameof(SemiFunc.DebugTester))]
    [HarmonyPrefix]
    private static bool DebugTesterPatch(ref bool __result)
    {
        if (ConfigManager.DeveloperMode.Value)
        {
            __result = true;
            return false;
        }

        return true;
    }

    [HarmonyPatch(nameof(SemiFunc.EnemySpawn))]
    [HarmonyPrefix]
    private static bool EnemySpawnPatch(ref bool __result)
    {
        if (Enemies.SpawnNextEnemiesNotDespawned > 0)
        {
            Enemies.SpawnNextEnemiesNotDespawned--;

            __result = true;
            return false;
        }

        return true;
    }
}
