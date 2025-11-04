using System.Collections.Generic;
using System.Linq;

namespace REPOLib.Extensions;

internal static class StatsManagerExtensions
{
    public static bool HasItem(this StatsManager statsManager, Item item)
    {
        if (item == null)
        {
            return false;
        }

        return statsManager.itemDictionary.ContainsKey(item.name);
    }

    internal static bool AddItem(this StatsManager statsManager, Item item)
    {
        if (!statsManager.itemDictionary.ContainsKey(item.name))
        {
            statsManager.itemDictionary.Add(item.name, item);
        }

        foreach (Dictionary<string, int> dictionary in statsManager.AllDictionariesWithPrefix("item"))
        {
            dictionary[item.name] = 0;
        }

        return true;
    }

    public static List<Item> GetItems(this StatsManager statsManager)
    {
        return statsManager.itemDictionary.Values.ToList();
    }
}
