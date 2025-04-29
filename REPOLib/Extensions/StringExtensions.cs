using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace REPOLib.Extensions;

[PublicAPI]
internal static class StringExtensions
{
    public static bool EqualsAny(this string? value, IReadOnlyCollection<string?>? inputs, StringComparison comparisonType = StringComparison.Ordinal)
    {
        if (value == null || inputs == null)
            return false;

        return inputs.Contains(value, StringComparer.FromComparison(comparisonType));
    }

    public static SteamId ToSteamId(this string value)
        => ulong.TryParse(value, out var result) ? result : 0;
    
    public static bool IsValidSteamId(this string value)
        => value.ToSteamId().IsValid;
}
