using REPOLib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace REPOLib.Modules;

public static class Items
{
    public static IReadOnlyList<Item> AllItems => GetItems();
    public static IReadOnlyList<Item> RegisteredItems => _itemsRegistered;

    private static readonly List<Item> _itemsToRegister = [];
    private static readonly List<Item> _itemsRegistered = [];

    private static bool _initialItemsRegistered;

    // This will run multiple times because of how the vanilla game registers items.
    internal static void RegisterItems()
    {
        if (StatsManager.instance == null)
        {
            Logger.LogError("Failed to register items. StatsManager instance is null.");
            return;
        }

        Logger.LogInfo($"Adding items.");

        foreach (var item in _itemsToRegister)
        {
            RegisterItemWithGame(item);
        }
        
        _initialItemsRegistered = true;
    }

    private static void RegisterItemWithGame(Item item)
    {
        Utilities.FixAudioMixerGroups(item.prefab);
        
        if (StatsManager.instance.AddItem(item))
        {
            if (!_itemsRegistered.Contains(item))
            {
                _itemsRegistered.Add(item);
            }

            Logger.LogInfo($"Added item \"{item.itemName}\"", extended: true);
        }
        else
        {
            Logger.LogWarning($"Failed to add item \"{item.itemName}\"", extended: true);
        }
    }

    public static void RegisterItem(Item item)
    {
        if (item == null)
        {
            throw new ArgumentException("Failed to register item. Item is null.");
        }

        if (item.prefab == null)
        {
            Logger.LogError($"Failed to register item \"{item.itemName}\". Item prefab is null.");
            return;
        }

        if (item.itemAssetName != item.prefab.name)
        {
            Logger.LogError($"Failed to register item \"{item.itemName}\". Item itemAssetName does not match the prefab name.");
            return;
        }

        if (ResourcesHelper.HasItemPrefab(item))
        {
            Logger.LogError($"Failed to register item \"{item.itemName}\". Item prefab already exists in Resources with the same name.");
            return;
        }

        if (_itemsToRegister.Any(x => x.itemAssetName == item.itemAssetName))
        {
            Logger.LogError($"Failed to register item \"{item.itemName}\". Item prefab already exists with the same name.");
            return;
        }

        if (_itemsToRegister.Contains(item))
        {
            Logger.LogError($"Failed to register item \"{item.itemName}\". Item is already registered!");
            return;
        }

        string prefabId = ResourcesHelper.GetItemPrefabPath(item);
        NetworkPrefabs.RegisterNetworkPrefab(prefabId, item.prefab);

        _itemsToRegister.Add(item);
        
        if (_initialItemsRegistered)
        { 
            RegisterItemWithGame(item);   
        }
    }

    public static GameObject SpawnItem(Item item, Vector3 position, Quaternion rotation)
    {
        if (item == null)
        {
            Logger.LogError("Failed to spawn item. Item is null.");
            return null;
        }

        if (item.prefab == null)
        {
            Logger.LogError("Failed to spawn item. Prefab is null.");
            return null;
        }

        if (!SemiFunc.IsMasterClientOrSingleplayer())
        {
            Logger.LogError($"Failed to spawn item \"{item.itemName}\". You are not the host.");
            return null;
        }

        string prefabId = ResourcesHelper.GetItemPrefabPath(item);
        GameObject gameObject = NetworkPrefabs.SpawnNetworkPrefab(prefabId, position, rotation);

        if (gameObject == null)
        {
            Logger.LogError($"Failed to spawn item \"{item.itemName}\"");
            return null;
        }

        Logger.LogInfo($"Spawned item \"{item.itemName}\" at position {position}, rotation: {rotation.eulerAngles}", extended: true);

        return gameObject;
    }

    public static IReadOnlyList<Item> GetItems()
    {
        if (StatsManager.instance == null)
        {
            return [];
        }

        return StatsManager.instance.GetItems();
    }

    public static bool TryGetItemByName(string name, out Item item)
    {
        item = GetItemByName(name);
        return item != null;
    }

    public static Item GetItemByName(string name)
    {
        return StatsManager.instance?.GetItemByName(name);
    }

    public static bool TryGetItemThatContainsName(string name, out Item item)
    {
        item = GetItemThatContainsName(name);
        return item != null;
    }

    public static Item GetItemThatContainsName(string name)
    {
        return StatsManager.instance?.GetItemThatContainsName(name);
    }
}
