using HarmonyLib;
using REPOLib.Extensions;
using REPOLib.Modules;

namespace REPOLib.Patches;

[HarmonyPatch(typeof(EnemyDirector))]
internal static class EnemyDirectorPatch
{
    private static bool _alreadyRegistered;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(EnemyDirector.Awake))]
    private static void AwakePatch()
    {
        // We have to refill the enemies to the lists on each game load
        if (!_alreadyRegistered)
        {
            Enemies.RegisterInitialEnemies();
            _alreadyRegistered = true;
            return;
        }

        foreach (EnemySetup? enemy in Enemies.RegisteredEnemies)
            EnemyDirector.instance.AddEnemy(enemy);
    }
}