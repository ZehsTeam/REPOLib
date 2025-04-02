using System;
using System.Collections.Generic;
using System.Reflection;

namespace REPOLib.Extensions;


/// <summary>
/// We add these extensions because we don't care about the reflection errors, and we can safely ignore them.
/// </summary>
internal static class ReflectionExtensions
{
    /// <summary>
    /// A safe way to get all methods from a type.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
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

    /// <summary>
    /// A safe way to get a custom attribute from a method.
    /// </summary>
    /// <param name="method"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
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
    
    /// <summary>
    /// A safe way to check if a method has a custom attribute.
    /// </summary>
    /// <param name="method"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
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
