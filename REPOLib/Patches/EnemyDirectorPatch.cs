using HarmonyLib;
using REPOLib.Modules;

namespace REPOLib.Patches;

[HarmonyPatch(typeof(EnemyDirector))]
internal static class EnemyDirectorPatch
{
    private static bool _patchedAwake = false;

    [HarmonyPatch(nameof(EnemyDirector.Awake))]
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    private static void AwakePatch()
    {
        if (_patchedAwake) return;
        _patchedAwake = true;
        
        Enemies.RegisterEnemies();
    }
}