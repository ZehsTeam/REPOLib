using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace REPOLib.Extensions;

internal static class StringExtensions
{
    public static bool EqualsAny(this string value, IReadOnlyCollection<string> inputs, StringComparison comparisonType = StringComparison.Ordinal)
    {
        if (value == null || inputs == null)
        {
            return false;
        }

        return inputs.Contains(value, StringComparer.FromComparison(comparisonType));
    }

    public static SteamId ToSteamId(this string value)
    {
        return ulong.TryParse(value, out ulong result) ? result : default;
    }

    public static bool IsValidSteamId(this string value)
    {
        return value.ToSteamId().IsValid;
    }
}
