using HarmonyLib;
using REPOLib.Extensions;
using REPOLib.Modules;

namespace REPOLib.Patches;

[HarmonyPatch(typeof(EnemyDirector))]
internal static class EnemyDirectorPatch
{
    private static bool _alreadyRegistered = false;

    [HarmonyPatch(nameof(EnemyDirector.Awake))]
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    private static void AwakePatch()
    {
        // We have to refill the enemies to the lists on each game load
        if (_alreadyRegistered)
        {
            foreach (var enemy in Enemies.RegisteredEnemies)
            {
                EnemyDirector.instance.AddEnemy(enemy);
            }
            return;
        }
        
        Enemies.RegisterEnemies();
        _alreadyRegistered = true;
    }
}