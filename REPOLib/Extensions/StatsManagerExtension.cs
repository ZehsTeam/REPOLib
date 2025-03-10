using System.Collections.Generic;

namespace REPOLib.Extensions;

internal static class StatsManagerExtension
{
    public static bool HasItem(this StatsManager statsManager, Item item)
    {
        if (item == null)
        {
            return false;
        }

        return statsManager.itemDictionary.ContainsKey(item.itemAssetName);
    }

    public static bool AddItem(this StatsManager statsManager, Item item)
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
}
