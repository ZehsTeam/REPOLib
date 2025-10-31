using REPOLib.Modules;
using UnityEngine;
using ChatCommand = DebugCommandHandler.ChatCommand;

namespace REPOLib.Commands;

internal static class SpawnItemCommand
{
    public static void Register()
    {
        Modules.Commands.RegisterCommand(new ChatCommand(
            "spawnitem",
            "Spawn an instance of an item with the specified (case-insensitive) name. You can optionally leave out \"Item \" from the prefab name.",
            Execute,
            suggest: null,
            debugOnly: true));
    }

    public static void Execute(bool isDebugConsole, string[] args)
    {
        if (!SemiFunc.IsMasterClientOrSingleplayer())
        {
            Logger.LogError("Only the host can spawn items!");
            return;
        }

        if (PlayerAvatar.instance == null)
        {
            Logger.LogWarning("Spawn item command failed. PlayerAvatar instance is null.");
            return;
        }

        if (StatsManager.instance == null)
        {
            Logger.LogError("Spawn item command failed. StatsManager instance is null.");
            return;
        }

        string name = string.Join(" ", args);

        Vector3 position = PlayerAvatar.instance.transform.position + new Vector3(0f, 1f, 0f) + PlayerAvatar.instance.transform.forward * 1f;

        Logger.LogInfo($"Trying to spawn item \"{name}\" at {position}...", extended: true);

        if (!Items.TryGetItemThatContainsName(name, out Item? item))
        {
            Logger.LogWarning($"Spawn item command failed. Unknown item with name \"{name}\"");
            return;
        }

        Items.SpawnItem(item, position, Quaternion.identity);
    }
}
