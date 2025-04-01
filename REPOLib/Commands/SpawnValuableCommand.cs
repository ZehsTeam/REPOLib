using REPOLib.Modules;
using UnityEngine;

namespace REPOLib.Commands;

internal static class SpawnValuableCommand
{
    [CommandExecution(
        "Spawn Valuable",
        "Spawn an instance of a valuable with the specified (case-insensitive) name. You can optionally leave out \"Valuable \" from the prefab name.",
        requiresDeveloperMode: true
    )]
    [CommandAlias("spawnvaluable")]
    [CommandAlias("spawnval")]
    [CommandAlias("sv")]
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
            Logger.LogError("Only the host can spawn valuables!");
            return;
        }

        if (PlayerAvatar.instance == null)
        {
            Logger.LogWarning("Can't spawn anything, player avatar is not initialized.");
            return;
        }

        if (ValuableDirector.instance == null)
        {
            Logger.LogError("ValuableDirector not initialized.");
            return;
        }

        string name = args;

        Vector3 position = PlayerAvatar.instance.transform.position + new Vector3(0f, 1f, 0f) + PlayerAvatar.instance.transform.forward * 1f;

        Logger.LogInfo($"Trying to spawn valuable \"{name}\" at {position}...", extended: true);

        if (!Valuables.TryGetValuableThatContainsName(name, out ValuableObject? valuableObject))
        {
            Logger.LogWarning($"Spawn command failed. Unknown valuable with name \"{name}\"");
            return;
        }

        Valuables.SpawnValuable(valuableObject, position, Quaternion.identity);
    }
}
