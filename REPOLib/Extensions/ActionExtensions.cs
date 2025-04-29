using System;
using System.Collections;

namespace REPOLib.Extensions;

internal static class ActionExtensions
{
    public static Func<T, IEnumerator> ToEnumerator<T>(this Action<T> action) => arg =>
    {
        action(arg);
        return null!;
    };
}