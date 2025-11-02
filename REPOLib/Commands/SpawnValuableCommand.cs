using REPOLib.Modules;
using UnityEngine;
using ChatCommand = DebugCommandHandler.ChatCommand;

namespace REPOLib.Commands;

internal static class SpawnValuableCommand
{
    public static void Register()
    {
        Modules.Commands.RegisterCommand(new ChatCommand(
            "spawnvaluable",
            "Spawn an instance of a valuable with the specified (case-insensitive) name. You can optionally leave out \"Valuable \" from the prefab name.",
            Execute,
            suggest: null,
            debugOnly: true));
    }

    public static void Execute(bool isDebugConsole, string[] args)
    {
        if (!SemiFunc.IsMasterClientOrSingleplayer())
        {
            Logger.LogError("Only the host can spawn valuables!");
            return;
        }

        if (PlayerAvatar.instance == null)
        {
            Logger.LogWarning("Spawn valuable command failed. PlayerAvatar instance is null.");
            return;
        }

        if (ValuableDirector.instance == null)
        {
            Logger.LogError("Spawn valuable command failed. ValuableDirector instance is null.");
            return;
        }

        string name = string.Join(" ", args);

        Vector3 position = PlayerAvatar.instance.transform.position + new Vector3(0f, 1f, 0f) + PlayerAvatar.instance.transform.forward * 1f;

        Logger.LogInfo($"Trying to spawn valuable \"{name}\" at {position}...", extended: true);

        if (!Valuables.TryGetValuableThatContainsName(name, out PrefabRef? prefabRef))
        {
            Logger.LogWarning($"Spawn valuable command failed. Unknown valuable with name \"{name}\"");
            return;
        }

        NetworkPrefabs.SpawnNetworkPrefab(prefabRef, position, Quaternion.identity);
    }
}
