using System;
using System.Collections.Generic;
using System.Linq;

namespace REPOLib.Extensions;

public static class StringExtension
{
    public static bool EqualsAny(this string value, IReadOnlyCollection<string> inputs, StringComparison comparisonType = StringComparison.Ordinal)
    {
        if (value == null || inputs == null)
        {
            return false;
        }

        return inputs.Contains(value, StringComparer.FromComparison(comparisonType));
    }
}
