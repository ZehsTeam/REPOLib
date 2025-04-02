using System.Collections.Generic;
using System.Linq;

namespace REPOLib.Extensions;

/// <summary>
/// REPOLib's <see cref="StatsManager"/> extension methods.
/// </summary>
public static class StatsManagerExtensions
{
    /// <summary>
    /// Check if <see cref="StatsManager"/> has the specified <see cref="Item"/>.
    /// </summary>
    /// <param name="statsManager">The <see cref="ItemManager"/> from where to check.</param>
    /// <param name="item">The <see cref="Item"/> to check.</param>
    /// <returns>Whether or not the item was found.</returns>
    public static bool HasItem(this StatsManager statsManager, Item item)
    {
        if (item == null)
        {
            return false;
        }

        return statsManager.itemDictionary.ContainsKey(item.itemAssetName);
    }

    internal static bool AddItem(this StatsManager statsManager, Item item)
    {
        if (!statsManager.itemDictionary.ContainsKey(item.itemAssetName))
        {
            statsManager.itemDictionary.Add(item.itemAssetName, item);
        }

        foreach (Dictionary<string, int> dictionary in statsManager.AllDictionariesWithPrefix("item"))
        {
            dictionary[item.itemAssetName] = 0;
        }

        return true;
    }

    /// <summary>
    /// Gets all items.
    /// </summary>
    /// <param name="statsManager">The <see cref="StatsManager"/> from where to get the items.</param>
    /// <returns>The list of all items.</returns>
    public static List<Item> GetItems(this StatsManager statsManager)
    {
        return statsManager.itemDictionary.Values.ToList();
    }

    /// <summary>
    /// Tries to get an <see cref="Item"/> by name.
    /// </summary>
    /// <param name="statsManager">The <see cref="StatsManager"/> from where to check.</param>
    /// <param name="name">The <see cref="string"/> to match.</param>
    /// <param name="item">The found <see cref="Item"/>.</param>
    /// <returns>Whether or not the <see cref="Item"/> was found.</returns>
    public static bool TryGetItemByName(this StatsManager statsManager, string name, out Item item)
    {
        item = statsManager.GetItemByName(name);
        return item != null;
    }

    /// <summary>
    /// Gets an <see cref="Item"/> by name.
    /// </summary>
    /// <param name="statsManager">The <see cref="StatsManager"/> from where to check.</param>
    /// <param name="name">The <see cref="string"/> to match.</param>
    /// <returns>The <see cref="Item"/> or null.</returns>
    public static Item GetItemByName(this StatsManager statsManager, string name)
    {
        return statsManager.GetItems()
            .FirstOrDefault(x => x.NameEquals(name));
    }

    /// <summary>
    /// Tries to get an <see cref="Item"/> that contains the name.
    /// </summary>
    /// <param name="statsManager">The <see cref="StatsManager"/> from where to check.</param>
    /// <param name="name">The <see cref="string"/> to compare.</param>
    /// <param name="item">The found <see cref="Item"/>.</param>
    /// <returns>Whether or not the <see cref="Item"/> was found.</returns>
    public static bool TryGetItemThatContainsName(this StatsManager statsManager, string name, out Item item)
    {
        item = statsManager.GetItemThatContainsName(name);
        return item != null;
    }

    /// <summary>
    /// Gets an <see cref="Item"/> that contains the name.
    /// </summary>
    /// <param name="statsManager">The <see cref="StatsManager"/> from where to check.</param>
    /// <param name="name">The <see cref="string"/> to compare.</param>
    /// <returns>The <see cref="Item"/> or null.</returns>
    public static Item GetItemThatContainsName(this StatsManager statsManager, string name)
    {
        return statsManager.GetItems()
            .FirstOrDefault(x => x.NameContains(name));
    }
}
