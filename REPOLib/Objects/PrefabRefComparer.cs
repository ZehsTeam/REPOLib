using System;
using System.Collections.Generic;

namespace REPOLib.Objects;

internal class PrefabRefComparer : IEqualityComparer<PrefabRef>
{
    public StringComparison ComparisonType { get; }

    public PrefabRefComparer(StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
    {
        ComparisonType = comparisonType;
    }

    public bool Equals(PrefabRef x, PrefabRef y)
    {
        if (ReferenceEquals(x, y))
            return true;

        if (x is null || y is null)
            return false;

        return string.Equals(x.PrefabName, y.PrefabName, ComparisonType)
            && string.Equals(x.ResourcePath, y.ResourcePath, ComparisonType);
    }

    public int GetHashCode(PrefabRef obj)
    {
        if (obj is null)
            return 0;

        unchecked
        {
            int hash = 17;
            hash = hash * 23 + (obj.PrefabName?.GetHashCode(StringComparison.OrdinalIgnoreCase) ?? 0);
            hash = hash * 23 + (obj.ResourcePath?.GetHashCode(StringComparison.OrdinalIgnoreCase) ?? 0);
            return hash;
        }
    }
}

