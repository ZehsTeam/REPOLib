using REPOLib.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;

namespace REPOLib.Modules;

/// <summary>
/// The Items module of REPOLib.
/// </summary>
public static class Items
{
    /// <inheritdoc cref="GetItems"/>
    public static IReadOnlyList<Item> AllItems => GetItems();

    /// <summary>
    /// Gets all items registered with REPOLib.
    /// </summary>
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

    /// <summary>
    /// Registers an <see cref="Item"/>.
    /// </summary>
    /// <param name="item">The <see cref="Item"/> to register.</param>
    /// <exception cref="ArgumentException"></exception>
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

    /// <summary>
    /// Spawns an <see cref="Item"/>.
    /// </summary>
    /// <param name="item">The <see cref="Item"/> to spawn.</param>
    /// <param name="position">The position where the item will be spawned.</param>
    /// <param name="rotation">The rotation of the item.</param>
    /// <returns>The <see cref="Item"/> object that was spawned.</returns>
    public static GameObject? SpawnItem(Item item, Vector3 position, Quaternion rotation)
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
        GameObject? gameObject = NetworkPrefabs.SpawnNetworkPrefab(prefabId, position, rotation);

        if (gameObject == null)
        {
            Logger.LogError($"Failed to spawn item \"{item.itemName}\"");
            return null;
        }

        Logger.LogInfo($"Spawned item \"{item.itemName}\" at position {position}, rotation: {rotation.eulerAngles}", extended: true);

        return gameObject;
    }

    /// <summary>
    /// Gets all items.
    /// </summary>
    /// <returns>The list of all items.</returns>
    public static IReadOnlyList<Item> GetItems()
    {
        if (StatsManager.instance == null)
        {
            return [];
        }

        return StatsManager.instance.GetItems();
    }
    
    /// <summary>
    /// Tries to get an <see cref="Item"/> by name.
    /// </summary>
    /// <param name="name">The <see cref="string"/> to match.</param>
    /// <param name="item">The found <see cref="Item"/>.</param>
    /// <returns>Whether or not the <see cref="Item"/> was found.</returns>
    public static bool TryGetItemByName(string name, [NotNullWhen(true)] out Item? item)
    {
        item = GetItemByName(name);
        return item != null;
    }

    /// <summary>
    /// Gets an <see cref="Item"/> by name.
    /// </summary>
    /// <param name="name">The <see cref="string"/> to match.</param>
    /// <returns>The <see cref="Item"/> or null.</returns>
    public static Item? GetItemByName(string name)
    {
        return StatsManager.instance?.GetItemByName(name);
    }

    /// <summary>
    /// Tries to get an <see cref="Item"/> that contains the name.
    /// </summary>
    /// <param name="name">The <see cref="string"/> to compare.</param>
    /// <param name="item">The found <see cref="Item"/>.</param>
    /// <returns>Whether or not the <see cref="Item"/> was found.</returns>
    public static bool TryGetItemThatContainsName(string name, [NotNullWhen(true)] out Item? item)
    {
        item = GetItemThatContainsName(name);
        return item != null;
    }

    /// <summary>
    /// Gets an <see cref="Item"/> that contains the name.
    /// </summary>
    /// <param name="name">The <see cref="string"/> to compare.</param>
    /// <returns>The <see cref="Item"/> or null.</returns>
    public static Item? GetItemThatContainsName(string name)
    {
        return StatsManager.instance?.GetItemThatContainsName(name);
    }
}
