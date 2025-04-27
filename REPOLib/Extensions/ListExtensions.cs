using System;
using System.Collections.Generic;
using System.Linq;

namespace REPOLib.Extensions;

internal static class ListExtensions
{
    public enum StringSortMode
    {
        Shortest,
        Longest
    }

    public static IEnumerable<T> SortByStringLength<T>(this IEnumerable<T> list, Func<T, string> getString, StringSortMode sortMode)
    {
        if (list == null)
        {
            throw new ArgumentNullException(nameof(list));
        }

        if (getString == null)
        {
            throw new ArgumentNullException(nameof(getString));
        }

        return sortMode switch
        {
            StringSortMode.Shortest => list.OrderBy(item => getString(item)?.Length ?? 0),
            StringSortMode.Longest => list.OrderByDescending(item => getString(item)?.Length ?? 0),
            _ => throw new ArgumentOutOfRangeException(nameof(sortMode), sortMode, null)
        };
    }
}
