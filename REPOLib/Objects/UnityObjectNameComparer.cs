using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace REPOLib.Objects;

internal class UnityObjectNameComparer<T>(StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
    : IEqualityComparer<T>
    where T : Object
{
    private StringComparison ComparisonType { get; } = comparisonType;

    public bool Equals(T x, T y)
    {
        if (x == y) return true;

        if (x == null || y == null)
            return false;

        return x.name.Equals(y.name, ComparisonType);
    }

    public int GetHashCode(T obj) => obj != null ? obj.name.GetHashCode() : 0;
}
