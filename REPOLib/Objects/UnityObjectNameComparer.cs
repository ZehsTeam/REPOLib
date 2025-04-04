using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace REPOLib.Objects;

/// <summary>
/// Compares the names of UnityEngine Objects.
/// </summary>
/// <typeparam name="T">The <see cref="Object"/> type.</typeparam>
public class UnityObjectNameComparer<T> : IEqualityComparer<T> where T : Object
{
    /// <summary>
    /// The <see cref="StringComparison"/> type.
    /// </summary>
    public StringComparison ComparisonType { get; private set; }

    /// <param name="comparisonType">The <see cref="StringComparison"/> type.</param>
    /// <inheritdoc cref="UnityObjectNameComparer{T}"/>
    public UnityObjectNameComparer(StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
    {
        ComparisonType = comparisonType;
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public int GetHashCode(T obj)
    {
        return obj != null ? obj.name.GetHashCode() : 0;
    }
}
