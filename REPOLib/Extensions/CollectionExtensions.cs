using REPOLib.Objects.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;

namespace REPOLib.Extensions;

internal static class CollectionExtensions
{
    public static IEnumerable<T> OrderByTypePriority<T>(this IEnumerable<T> source, Type[] types) where T : Content
    {
        Dictionary<Type, int> typesMap = [];

        for (int i = 0; i < types.Length; i++)
        {
            typesMap[types[i]] = i;
        }

        return source.OrderBy(x =>
        {
            if (typesMap.TryGetValue(x.GetType(), out int priority))
                return priority;

            return int.MaxValue;
        });
    }

    public static IEnumerable<T> OrderByTypeFirst<T, TFirst>(this IEnumerable<T> source)
    {
        return source.OrderBy(x => x is TFirst ? 0 : 1);
    }
}
