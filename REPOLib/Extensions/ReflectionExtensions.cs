using System;
using System.Collections.Generic;
using System.Reflection;

namespace REPOLib.Extensions;

internal static class ReflectionExtensions
{
    public static IEnumerable<MethodInfo> SafeGetMethods(this Type type)
    {
        try
        {
            return type.GetMethods();
        }
        catch /*(Exception ex)*/
        {
            // Logger.LogWarning($"Error retrieving methods for type {type.FullName}: {ex.Message}");
            return Array.Empty<MethodInfo>();
        }
    }

    public static T? SafeGetCustomAttribute<T>(this MethodInfo method) where T : Attribute
    {
        try
        {
            return method.GetCustomAttribute<T>();
        }
        catch /*(Exception ex)*/
        {
            // Logger.LogWarning($"Error retrieving methods for type {type.FullName}: {ex.Message}");
            return null;
        }   
    }

    public static bool HasCustomAttribute<T>(this MethodInfo method) where T : Attribute
    {
        try
        {
            return method.GetCustomAttribute<T>() != null;
        }
        catch /*(Exception ex)*/
        {
            // Logger.LogWarning($"Error retrieving methods for type {type.FullName}: {ex.Message}");
            return false;
        }
    }
}
