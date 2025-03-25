using System;
using System.Collections.Generic;
using System.Reflection;

namespace REPOLib.Extensions;

public static class TypeExtensions
{
    public static IEnumerable<MethodInfo?> SafeGetMethods(this Type type)
    {
        try
        {
            // Return methods safely
            return type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
        }
        catch (Exception ex)
        {
            // Log and return null if there's an error
            // Console.WriteLine($"Error retrieving methods for type {type.FullName}: {ex.Message}");
            return null;
        }
    }
}
