using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace REPOLib.Extensions;

internal static class StatsManagerExtensions
{
    public static bool HasItem(this StatsManager statsManager, Item item)
        => item != null && statsManager.itemDictionary.ContainsKey(item.itemAssetName);

    internal static bool AddItem(this StatsManager statsManager, Item item)
    {
        statsManager.itemDictionary.TryAdd(item.itemAssetName, item);
        foreach (Dictionary<string, int> dictionary in statsManager.AllDictionariesWithPrefix("item"))
            dictionary[item.itemAssetName] = 0;

        return true;
    }

    public static List<Item> GetItems(this StatsManager statsManager)
        => statsManager.itemDictionary.Values.ToList();

    public static bool TryGetItemByName(this StatsManager statsManager, string? name, [NotNullWhen(true)] out Item? item)
        => (item = statsManager.GetItemByName(name)) != null;

    public static Item? GetItemByName(this StatsManager statsManager, string? name)
        => statsManager.GetItems()
                       .FirstOrDefault(x => x.NameEquals(name));

    public static bool TryGetItemThatContainsName(this StatsManager statsManager, string name, [NotNullWhen(true)] out Item? item)
        => (item = statsManager.GetItemThatContainsName(name)) != null;

    public static Item? GetItemThatContainsName(this StatsManager statsManager, string name)
        => statsManager.GetItems()
                       .SortByStringLength(x => x.itemName, ListExtensions.StringSortMode.Shortest)
                       .FirstOrDefault(x => x.NameContains(name));
}