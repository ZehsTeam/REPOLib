using Photon.Pun;
using REPOLib.Modules;
using System;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace REPOLib.Commands;

public static class SpawnItemCommand
{
    [CommandExecution(
        "Spawn Item",
        "Spawn an instance of an item with the specified (case-insensitive) name. You can optionally leave out \"Item \" from the prefab name.",
        requiresDeveloperMode: true
        )]
    [CommandAlias("spawnitem")]
    [CommandAlias("si")]
    public static void Execute(string args)
    {
        Logger.LogInfo($"Running spawn command with args \"{args}\"", extended: true);

        if (args == null ||  args.Length == 0 )
        {
            Logger.LogWarning("No args provided to spawn command.");
            return;
        }

        if (!SemiFunc.IsMasterClientOrSingleplayer())
        {
            Logger.LogError("Only the host can spawn items!");
            return;
        }

        if (StatsManager.instance == null)
        {
            Logger.LogError("Failed spawn item command, StatsManager is not initialized.");
            return;
        }

        if (PlayerAvatar.instance == null)
        {
            Logger.LogWarning("Can't spawn anything, player avatar is not initialized.");
            return;
        }

        SpawnItemByName(args);
    }

    private static void SpawnItemByName(string name)
    {
        Vector3 spawnPos = PlayerAvatar.instance.transform.position + PlayerAvatar.instance.transform.forward * 1f;

        Logger.LogInfo($"Trying to spawn item \"{name}\" at {spawnPos}...", extended: true);

        if (!TryGetItemByName(name, out Item item))
        {
            Logger.LogWarning($"Could not find an item with the name \"{name}\" to spawn.");
            return;
        }

        try
        {
            if (SemiFunc.IsMultiplayer())
            {
                string itemPath = ResourcesHelper.GetItemPrefabPath(item);

                if (itemPath == string.Empty)
                {
                    Logger.LogError($"Failed to get the path of item \"{item.itemName}\"");
                    return;
                }

                Logger.LogInfo($"Network spawning \"{itemPath}\" at {spawnPos}.");
                PhotonNetwork.InstantiateRoomObject(itemPath, spawnPos, Quaternion.identity);
            }
            else
            {
                Logger.LogInfo($"Locally spawning \"{item.itemName}\" at {spawnPos}.");
                Object.Instantiate(item.prefab, spawnPos, Quaternion.identity);
            }
        }
        catch (Exception e)
        {
            Logger.LogError($"Failed to spawn \"{item.itemName}\":\n{e}");
        }
    }

    private static bool TryGetItemByName(string name, out Item item)
    {
        if (StatsManager.instance == null)
        {
            item = null;
            return false;
        }

        Item[] items = StatsManager.instance.itemDictionary.Values.ToArray();

        item = items.FirstOrDefault(x => 
            x.itemAssetName.Contains(name, StringComparison.OrdinalIgnoreCase) ||
            x.itemName.Contains(name, StringComparison.OrdinalIgnoreCase));

        return item != null;
    }
}
