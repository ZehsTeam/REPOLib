using System;

namespace REPOLib.Extensions;

internal static class ItemExtensions
{
    public static bool NameEquals(this Item item, string? name)
    {
        if (item == null)
            return false;

        if (item.name.EqualsAny([ name, $"Item {name}" ], StringComparison.OrdinalIgnoreCase))
            return true;

        return item.itemAssetName.EqualsAny([ name, $"Item {name}" ], StringComparison.OrdinalIgnoreCase)
               || item.itemName.Equals(name, StringComparison.OrdinalIgnoreCase);
    }

    public static bool NameContains(this Item item, string name)
    {
        if (item == null)
            return false;

        if (item.name.Contains(name, StringComparison.OrdinalIgnoreCase))
            return true;

        return item.itemAssetName.Contains(name, StringComparison.OrdinalIgnoreCase)
               || item.itemName.Contains(name, StringComparison.OrdinalIgnoreCase);
    }
}