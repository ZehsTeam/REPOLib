using REPOLib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace REPOLib.Modules;

public static class Items
{
    public static IReadOnlyList<Item> RegisteredItems => _itemsRegistered;

    private static readonly List<Item> _itemsToRegister = [];
    private static readonly List<Item> _itemsRegistered = [];

    private static bool _canRegisterItems = true;

    // This will run multiple times because of how the vanilla game registers items.
    internal static void RegisterItems()
    {
        //if (!_canRegisterItems)
        //{
        //    return;
        //}

        if (StatsManager.instance == null)
        {
            Logger.LogError("Failed to register items. StatsManager instance is null.");
            return;
        }

        Logger.LogInfo($"Adding items.");

        foreach (var item in _itemsToRegister)
        {
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

        //_itemsToRegister.Clear();
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
    }
}
