using System;
using System.Collections.Generic;
using System.Linq;

namespace REPOLib.Extensions;

/// <summary>
/// Extension methods for <see cref="string"/>.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Check if any of the <see cref="string"/>s in the specified collection match the value.
    /// </summary>
    /// <param name="value">The <see cref="string"/> to compare against.</param>
    /// <param name="inputs">The <see cref="string"/> collection to check against the value.</param>
    /// <param name="comparisonType">The <see cref="StringComparison"/> setting.</param>
    /// <returns>Whether or not there was a match.</returns>
    public static bool EqualsAny(this string value, IReadOnlyCollection<string> inputs, StringComparison comparisonType = StringComparison.Ordinal)
    {
        if (value == null || inputs == null)
        {
            return false;
        }

        return inputs.Contains(value, StringComparer.FromComparison(comparisonType));
    }
}
