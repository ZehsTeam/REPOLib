using HarmonyLib;
using REPOLib.Modules;

namespace REPOLib.Patches;

[HarmonyPatch(typeof(EnemyDirector))]
internal static class EnemyDirectorPatch
{
    [HarmonyPatch(nameof(EnemyDirector.Awake))]
    [HarmonyPostfix]
    private static void AwakePatch()
    {
        Enemies.RegisterEnemies();
    }
}