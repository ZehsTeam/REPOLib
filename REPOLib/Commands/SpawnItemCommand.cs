using Photon.Pun;
using REPOLib.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace REPOLib.Commands
{
    public static class SpawnItemCommand
    {
        private static readonly Dictionary<string, GameObject> itemPrefabs = [];

        [REPOLibCommandExecution(
            "Spawn Item",
            "Spawn an instance of an item with the specified (case-insensitive) name. You can optionally leave out \"Item \" from the prefab name.",
            requiresDeveloperMode: true
            )]
        [REPOLibCommandAlias("spawnitem")]
        [REPOLibCommandAlias("si")]
        public static void Execute(string args)
        {
            Logger.LogInfo($"Running spawn command with args \"{args}\"", extended: true);

            if (args == null ||  args.Length == 0 )
            {
                Logger.LogWarning("No args provided to spawn command.");
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
            Item itemPrefab;
            Logger.LogInfo($"Trying to spawn \"{name}\" at {spawnPos}...", extended: true);
            string itemName = StatsManager.instance.itemDictionary.Keys.FirstOrDefault(k => k.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (itemName == null)
            {
                itemName = StatsManager.instance.itemDictionary.Keys.FirstOrDefault(k => k.Equals("Item " + name, StringComparison.OrdinalIgnoreCase));
                if (itemName == null)
                {
                    Logger.LogWarning($"Could not find an item with name \"{name}\" to spawn");
                    return;
                }
            }
            if (!StatsManager.instance.itemDictionary.TryGetValue(itemName, out itemPrefab))
            {
                Logger.LogWarning($"Item prefab for \"{name}\" not found.");
                return;
            }
            try
            {
                if (GameManager.instance?.gameMode == 0)
                {
                    Logger.LogInfo($"Locally spawning {itemPrefab.name} at {spawnPos}.");
                    UnityEngine.Object.Instantiate(itemPrefab, spawnPos, Quaternion.identity);
                }
                else
                {
                    string valuablePath = ResourcesHelper.GetItemPrefabPath(itemPrefab);
                    if (valuablePath == string.Empty)
                    {
                        Logger.LogError($"Failed to get the path of valuable \"{itemPrefab.name ?? "null"}\"");
                        return;
                    }
                    Logger.LogInfo($"Network spawning {valuablePath} at {spawnPos}.");
                    PhotonNetwork.InstantiateRoomObject(valuablePath, spawnPos, Quaternion.identity);
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"Failed to spawn \"{name}\":\n{e}");
            }
        }
    }
}
