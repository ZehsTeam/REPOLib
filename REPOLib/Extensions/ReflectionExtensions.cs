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
        catch
        {
            return [];
        }
    }

    public static T? SafeGetCustomAttribute<T>(this MethodInfo method) where T : Attribute
    {
        try
        {
            return method.GetCustomAttribute<T>();
        }
        catch
        {
            return null;
        }
    }

    public static bool HasCustomAttribute<T>(this MethodInfo method) where T : Attribute
    {
        try
        {
            return method.GetCustomAttribute<T>() != null;
        }
        catch
        {
            return false;
        }
    }
}