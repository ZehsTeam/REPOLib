using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace REPOLib.Objects;

public class UnityObjectNameComparer<T> : IEqualityComparer<T> where T : Object
{
    public StringComparison ComparisonType { get; private set; }

    public UnityObjectNameComparer(StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
    {
        ComparisonType = comparisonType;
    }

    public bool Equals(T x, T y)
    {
        if (x == y)
        {
            return true; 
        }

        if (x == null || y == null)
        {
            return false;
        }

        return x.name.Equals(y.name, ComparisonType);
    }

    public int GetHashCode(T obj)
    {
        return obj != null ? obj.name.GetHashCode() : 0;
    }
}
