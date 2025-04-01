using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace REPOLib.Extensions;

public static class DictionaryExtensions
{
    public static bool TryGetKey<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TValue value, [NotNullWhen(true)] out TKey? key)
    {
        foreach (var kvp in dictionary)
        {
            if (Equals(kvp.Value, value))
            {
                key = kvp.Key!;
                return true;
            }
        }

        key = default;
        return false;
    }

    public static TKey? GetKeyOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TValue value)
    {
        if (dictionary.TryGetKey(value, out TKey? key))
        {
            return key;
        }

        return default;
    }

    public static TKey? GetKeyOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TValue value, TKey defaultKey)
    {
        if (dictionary.TryGetKey(value, out TKey? key))
        {
            return key;
        }

        return defaultKey;
    }

    public static bool ContainsKey<T>(this Dictionary<string, T> dictionary, string key, bool ignoreKeyCase)
    {
        if (!ignoreKeyCase)
        {
            return dictionary.ContainsKey(key);
        }

        foreach (var kvp in dictionary)
        {
            if (string.Equals(kvp.Key, key, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    public static bool TryGetValue<T>(this Dictionary<string, T> dictionary, string key, [NotNullWhen(true)] out T? value, bool ignoreKeyCase)
    {
        if (!ignoreKeyCase)
        {
            return dictionary.TryGetValue(key, out value);
        }

        foreach (var kvp in dictionary)
        {
            if (string.Equals(kvp.Key, key, StringComparison.OrdinalIgnoreCase))
            {
                value = kvp.Value!;
                return true;
            }
        }

        value = default;
        return false;
    }

    public static T? GetValueOrDefault<T>(this Dictionary<string, T> dictionary, string key, bool ignoreKeyCase)
    {
        if (dictionary.TryGetValue(key, out T? value, ignoreKeyCase))
        {
            return value;
        }

        return default;
    }

    public static T? GetValueOrDefault<T>(this Dictionary<string, T> dictionary, string key, T defaultValue, bool ignoreKeyCase)
    {
        if (dictionary.TryGetValue(key, out T? value, ignoreKeyCase))
        {
            return value;
        }

        return defaultValue;
    }
}
