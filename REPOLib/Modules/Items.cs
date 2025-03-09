using System;
using System.Collections.Generic;

namespace REPOLib.Modules;

public static class Items
{
    public static IReadOnlyList<Item> RegisteredItems => _itemsRegistered;

    private static readonly List<Item> _itemsToRegister = [];
    private static readonly List<Item> _itemsRegistered = [];

    private static bool _canRegisterItems = true;

    internal static void RegisterItems()
    {
        if (!_canRegisterItems)
        {
            return;
        }

        if (StatsManager.instance == null)
        {
            Logger.LogError("Failed to register items. StatsManager instance is null.");
            return;
        }

        foreach (var item in _itemsToRegister)
        {
            if (StatsManager.instance.itemDictionary.ContainsKey(item.itemAssetName))
            {
                continue;
            }

            StatsManager.instance.itemDictionary.Add(item.itemAssetName, item);

            foreach (Dictionary<string, int> dictionary in StatsManager.instance.AllDictionariesWithPrefix("item"))
            {
                dictionary.Add(item.itemAssetName, 0);
            }
        }
        
        _canRegisterItems = false;
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

        if (!_canRegisterItems)
        {
            Logger.LogError($"Failed to register item \"{item.itemName}\". You can only register items from your plugins awake!");
            return;
        }

        if (_itemsToRegister.Contains(item))
        {
            Logger.LogError($"Failed to register item \"{item.itemName}\". Item is already registered!");
            return;
        }

        NetworkPrefabs.RegisterNetworkPrefab(item.prefab);

        _itemsToRegister.Add(item);
    }
}
