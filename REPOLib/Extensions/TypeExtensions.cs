using System;
using System.Collections.Generic;
using System.Reflection;

namespace REPOLib.Extensions;

internal static class TypeExtensions
{
    public static IEnumerable<MethodInfo?> SafeGetMethods(this Type type)
    {
        try
        {
            return type.GetMethods();
        }
        catch (Exception ex)
        {
            // Log($"Error retrieving methods for type {type.FullName}: {ex.Message}");
            return null;
        }
    }
}
