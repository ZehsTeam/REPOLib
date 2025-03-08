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
        
        //we have to refill the enemys to the lists on each game load
        if (_alreadyRegistered)
        {
            foreach (var enemy in Enemies.RegisteredEnemys)
            {
                EnemyDirector.instance.AddEnemy(enemy);
            }
            return;
        }
        _alreadyRegistered = true;
        
        Enemies.RegisterEnemies();
    }
}