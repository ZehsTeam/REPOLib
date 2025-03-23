using REPOLib.Extensions;
using REPOLib.Modules;
using System;
using System.Collections;
using UnityEngine;

namespace REPOLib.Commands;

internal static class SpawnEnemyCommand
{
    [CommandExecution(
        "Spawn Enemy",
        "Spawn an instance of an enemy with the specified (case-insensitive) name. You can optionally leave out \"Enemy - \" from the prefab name.",
        requiresDeveloperMode: true
    )]
    [CommandAlias("spawnenemy")]
    [CommandAlias("se")]
    public static void Execute(string args)
    {
        Logger.LogInfo($"Running spawn command with args \"{args}\"", extended: true);

        if (string.IsNullOrWhiteSpace(args))
        {
            Logger.LogWarning("No args provided to spawn command.");
            return;
        }

        if (!SemiFunc.IsMasterClientOrSingleplayer())
        {
            Logger.LogError("Only the host can spawn enemies!");
            return;
        }

        if (EnemyDirector.instance == null)
        {
            Logger.LogError("Failed spawn enemy command, EnemyDirector is not initialized.");
            return;
        }

        if (LevelGenerator.Instance == null)
        {
            Logger.LogError("Failed spawn enemy command, LevelGenerator is not initialized.");
            return;
        }

        if (RunManager.instance == null)
        {
            Logger.LogError("Failed spawn enemy command, RunManager is not initialized.");
            return;
        }

        if (PlayerAvatar.instance == null)
        {
            Logger.LogWarning("Can't spawn anything, player avatar is not initialized.");
            return;
        }

        if (!EnemyDirector.instance.TryGetEnemyByName(args, out EnemySetup enemySetup))
        {
            Logger.LogWarning($"Spawn command failed. Unknown enemy with name \"{args}\"");
            return;
        }

        Vector3 position = PlayerAvatar.instance.transform.position;

        EnemyDirector.instance.StartCoroutine(SpawnEnemyAfterTime(enemySetup, position, TimeSpan.FromSeconds(3f)));
    }

    private static IEnumerator SpawnEnemyAfterTime(EnemySetup enemySetup, Vector3 position, TimeSpan timeSpan)
    {
        yield return new WaitForSeconds((float)timeSpan.TotalSeconds);
        Enemies.SpawnEnemy(enemySetup, position, spawnDespawned: false);
    }
}
