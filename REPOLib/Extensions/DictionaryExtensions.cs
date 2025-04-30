using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using JetBrains.Annotations;

namespace REPOLib.Extensions;

internal static class DictionaryExtensions
{
    private static bool TryGetKey<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TValue value,
        [NotNullWhen(true)] out TKey? key)
    {
        foreach (KeyValuePair<TKey, TValue> kvp in dictionary.Where(kvp => Equals(kvp.Value, value)))
        {
            key = kvp.Key!;
            return true;
        }

        key = default;
        return false;
    }

    public static TKey? GetKeyOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TValue value) =>
        dictionary.TryGetKey(value, out TKey? key) ? key : default;

    public static TKey GetKeyOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TValue value, TKey defaultKey)
        => dictionary.TryGetKey(value, out TKey? key) ? key : defaultKey;

    public static bool ContainsKey<T>(this Dictionary<string, T> dictionary, string key, bool ignoreKeyCase)
        => !ignoreKeyCase
            ? dictionary.ContainsKey(key)
            : dictionary.Any(kvp => string.Equals(kvp.Key, key, StringComparison.OrdinalIgnoreCase));

    public static bool TryGetValue<T>(this Dictionary<string, T> dictionary, string key, [NotNullWhen(true)] out T? value, bool ignoreKeyCase)
    {
        if (!ignoreKeyCase)
            return dictionary.TryGetValue(key, out value);

        foreach (KeyValuePair<string, T> kvp in dictionary.Where(kvp => string.Equals(kvp.Key, key, StringComparison.OrdinalIgnoreCase)))
        {
            value = kvp.Value!;
            return true;
        }

        value = default;
        return false;
    }

    public static T? GetValueOrDefault<T>(this Dictionary<string, T> dictionary, string key, bool ignoreKeyCase)
        => dictionary.TryGetValue(key, out T? value, ignoreKeyCase)
            ? value
            : default;

    public static T GetValueOrDefault<T>(this Dictionary<string, T> dictionary, string key, T defaultValue, bool ignoreKeyCase)
        => dictionary.TryGetValue(key, out T? value, ignoreKeyCase)
            ? value
            : defaultValue;
}