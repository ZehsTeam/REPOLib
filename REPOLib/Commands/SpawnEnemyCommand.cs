using REPOLib.Extensions;
using REPOLib.Modules;
using System;
using System.Collections;
using UnityEngine;
using ChatCommand = DebugCommandHandler.ChatCommand;

namespace REPOLib.Commands;

internal static class SpawnEnemyCommand
{
    public static void Register()
    {
        Modules.Commands.RegisterCommand(new ChatCommand(
            "spawnenemy",
            "Spawn an instance of an enemy with the specified (case-insensitive) name. You can optionally leave out \"Enemy - \" from the prefab name.",
            Execute,
            suggest: null,
            debugOnly: true));
    }

    public static void Execute(bool isDebugConsole, string[] args)
    {
        Logger.LogInfo($"Running spawn command with args \"{args}\"", extended: true);

        if (!SemiFunc.IsMasterClientOrSingleplayer())
        {
            Logger.LogError("Only the host can spawn enemies!");
            return;
        }

        if (PlayerAvatar.instance == null)
        {
            Logger.LogWarning("Spawn enemy command failed. PlayerAvatar instance is null.");
            return;
        }

        if (EnemyDirector.instance == null)
        {
            Logger.LogError("Spawn enemy command failed. EnemyDirector instance is null.");
            return;
        }

        if (LevelGenerator.Instance == null)
        {
            Logger.LogError("Spawn enemy command failed. LevelGenerator instance is null.");
            return;
        }

        if (RunManager.instance == null)
        {
            Logger.LogError("Spawn enemy command failed. RunManager instance is null.");
            return;
        }

        string name = string.Join(" ", args);

        if (!EnemyDirector.instance.TryGetEnemyThatContainsName(name, out EnemySetup enemySetup))
        {
            Logger.LogWarning($"Spawn enemy command failed. Unknown enemy with name \"{name}\"");
            return;
        }

        Vector3 position = PlayerAvatar.instance.transform.position;
        float delaySeconds = 3f;

        Logger.LogInfo($"Trying to spawn enemy \"{name}\" at {position} in {delaySeconds} seconds...", extended: true);

        EnemyDirector.instance.StartCoroutine(SpawnEnemyAfterTime(enemySetup, position, TimeSpan.FromSeconds(delaySeconds)));
    }

    private static IEnumerator SpawnEnemyAfterTime(EnemySetup enemySetup, Vector3 position, TimeSpan timeSpan)
    {
        yield return new WaitForSeconds((float)timeSpan.TotalSeconds);
        Enemies.SpawnEnemy(enemySetup, position, Quaternion.identity, spawnDespawned: false);
    }
}
