using REPOLib.Extensions;
using REPOLib.Objects.Sdk;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace REPOLib.Modules;

/// <summary>
/// The Items module of REPOLib.
/// </summary>
public static class Items
{
    /// <summary>
    /// Gets all items.
    /// </summary>
    /// <returns>The list of all items.</returns>
    public static IReadOnlyList<Item> AllItems => StatsManager.instance?.GetItems() ?? [];

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
        //Utilities.FixAudioMixerGroups(item.prefab);   This shouldn't be needed here since it's already called in RegisterItem.

        if (StatsManager.instance.AddItem(item))
        {
            if (!_itemsRegistered.Contains(item))
            {
                _itemsRegistered.Add(item);
            }

            Logger.LogInfo($"Added item \"{item.itemName}\" to StatsManager.", extended: true);
        }
        else
        {
            Logger.LogWarning($"Failed to add item \"{item.itemName}\" to StatsManager.", extended: true);
        }
    }

    /// <inheritdoc cref="RegisterItem(ItemAttributes)"/>
    /// <param name="itemContent">The <see cref="ItemContent"/> to register.</param>
    public static PrefabRef? RegisterItem(ItemContent? itemContent)
    {
        if (itemContent == null)
        {
            Logger.LogError($"Failed to register item. ItemContent is null.");
            return null;
        }

        return RegisterItem(itemContent.Prefab);
    }

    /// <summary>
    /// Registers an <see cref="Item"/>.
    /// </summary>
    /// <param name="itemAttributes">The item prefab to register.</param>
    /// <returns>The registered item <see cref="PrefabRef"/> or null.</returns>
    public static PrefabRef? RegisterItem(ItemAttributes? itemAttributes)
    {
        if (itemAttributes == null)
        {
            Logger.LogError($"Failed to register item. ItemAttributes is null.");
            return null;
        }

        Item item = itemAttributes.item;

        if (item == null)
        {
            Logger.LogError($"Failed to register item. Item is null.");
            return null;
        }

        GameObject prefab = itemAttributes.gameObject;
        string prefabId = $"Items/{prefab.name}";

        PrefabRef? existingPrefabRef = NetworkPrefabs.GetNetworkPrefabRef(prefabId);

        if (existingPrefabRef != null)
        {
            if (prefab == existingPrefabRef.Prefab)
            {
                Logger.LogWarning($"Failed to register item \"{item.itemName}\". Item is already registered!");
            }
            else
            {
                Logger.LogError($"Failed to register item \"{item.itemName}\". Item prefab already exists with the same name.");
            }

            return null;
        }

        PrefabRef? prefabRef = NetworkPrefabs.RegisterNetworkPrefab(prefabId, prefab);

        if (prefabRef == null)
        {
            Logger.LogError($"Failed to register item \"{item.itemName}\". PrefabRef is null.");
            return null;
        }

        item.prefab = prefabRef;

        Utilities.FixAudioMixerGroups(prefab);

        _itemsToRegister.Add(item);

        if (_initialItemsRegistered)
        {
            RegisterItemWithGame(item);
        }

        return prefabRef;
    }

    /// <summary>
    /// Spawns an <see cref="Item"/>.
    /// </summary>
    /// <param name="item">The <see cref="Item"/> to spawn.</param>
    /// <param name="position">The position where the item will be spawned.</param>
    /// <param name="rotation">The rotation of the item.</param>
    /// <returns>The <see cref="Item"/> object that was spawned or null.</returns>
    public static GameObject? SpawnItem(Item? item, Vector3 position, Quaternion rotation)
    {
        if (item == null)
        {
            Logger.LogError("Failed to spawn item. Item is null.");
            return null;
        }

        if (!item.prefab.IsValid())
        {
            Logger.LogError($"Failed to spawn item \"{item.itemName}\". PrefabRef is not valid.");
            return null;
        }

        if (!SemiFunc.IsMasterClientOrSingleplayer())
        {
            Logger.LogError($"Failed to spawn item \"{item.itemName}\". You are not the host.");
            return null;
        }

        GameObject? gameObject = NetworkPrefabs.SpawnNetworkPrefab(item.prefab, position, rotation);

        if (gameObject == null)
        {
            Logger.LogError($"Failed to spawn item \"{item.itemName}\". Spawned GameObject is null.");
            return null;
        }

        Logger.LogInfo($"Spawned item \"{item.itemName}\" at position {position}, rotation: {rotation.eulerAngles}", extended: true);

        return gameObject;
    }

    #region Deprecated
    #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    [Obsolete("This is no longer supported. Use AllItems or RegisteredItems instead.", error: true)]
    public static IReadOnlyList<Item> GetItems()
    {
        return AllItems;
    }

    [Obsolete("This is no longer supported. Use AllItems or RegisteredItems instead.", error: true)]
    public static bool TryGetItemByName(string name, [NotNullWhen(true)] out Item? item)
    {
        item = null;
        return false;
    }

    [Obsolete("This is no longer supported. Use AllItems or RegisteredItems instead.", error: true)]
    public static Item? GetItemByName(string name)
    {
        return null;
    }

    [Obsolete("This is no longer supported. Use AllItems or RegisteredItems instead.", error: true)]
    public static bool TryGetItemThatContainsName(string name, [NotNullWhen(true)] out Item? item)
    {
        item = null;
        return false;
    }

    [Obsolete("This is no longer supported. Use AllItems or RegisteredItems instead.", error: true)]
    public static Item? GetItemThatContainsName(string name)
    {
        return null;
    }
    #pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    #endregion
}
