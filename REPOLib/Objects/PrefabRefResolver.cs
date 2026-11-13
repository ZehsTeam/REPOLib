using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace REPOLib.Objects;

/// <summary>
/// The modded project stores PrefabRefs to base-game assets as their Resources path (loaded with Resources.Load)
/// and PrefabRefs to mod assets as their full asset path, which is their name in the mod's bundle.
/// The serialized bundle reference can't survive the build, so it's assigned here when the bundle loads.
/// </summary>
internal static class PrefabRefResolver
{
    private const string GameResourcesFolder = "Assets/REPO/Game/Resources/";
    private const int MaxDepth = 8;
    private const BindingFlags InstanceFields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    private static readonly Dictionary<Type, FieldInfo[]> _serializedFields = [];
    private static readonly Dictionary<Type, (FieldInfo ResourcePath, FieldInfo Bundle)?> _prefabRefFields = [];

    internal static IEnumerator ResolveBundle(AssetBundle bundle)
    {
        if (bundle.isStreamedSceneAssetBundle) yield break;

        var request = bundle.LoadAllAssetsAsync();
        yield return request;

        int resolved = 0;
        foreach (var asset in request.allAssets)
        {
            switch (asset)
            {
                case GameObject gameObject:
                    foreach (var component in gameObject.GetComponentsInChildren<MonoBehaviour>(true))
                    {
                        if (component != null) resolved += Walk(component, bundle, 0);
                    }
                    break;
                case ScriptableObject scriptableObject:
                    resolved += Walk(scriptableObject, bundle, 0);
                    break;
            }
        }

        if (resolved > 0)
        {
            Logger.LogInfo($"Resolved {resolved} PrefabRef(s) in bundle \"{bundle.name}\"", extended: true);
        }
    }

    /// <summary>
    /// Points a PrefabRef at the base game's Resources or at <paramref name="bundle"/>, depending on where its asset is.
    /// </summary>
    /// <returns>Whether the PrefabRef was changed.</returns>
    internal static bool Resolve(object prefabRef, AssetBundle? bundle)
    {
        var fields = GetPrefabRefFields(prefabRef.GetType());
        if (fields == null) return false;

        var (resourcePathField, bundleField) = fields.Value;
        string? resourcePath = resourcePathField.GetValue(prefabRef) as string;
        if (string.IsNullOrEmpty(resourcePath)) return false;

        if (resourcePath!.StartsWith(GameResourcesFolder, StringComparison.OrdinalIgnoreCase))
        {
            string relativePath = resourcePath.Substring(GameResourcesFolder.Length);
            int extension = relativePath.LastIndexOf('.');
            resourcePathField.SetValue(prefabRef, extension < 0 ? relativePath : relativePath.Substring(0, extension));
            bundleField.SetValue(prefabRef, null);
            return true;
        }

        if (bundle == null || !resourcePath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase)) return false;
        if (bundleField.GetValue(prefabRef) as AssetBundle == bundle) return false;

        if (!bundle.Contains(resourcePath.ToLowerInvariant()))
        {
            Logger.LogWarning($"PrefabRef to \"{resourcePath}\" can't be loaded because it isn't in bundle \"{bundle.name}\". Make sure it's included in the bundle.");
            return false;
        }

        bundleField.SetValue(prefabRef, bundle);
        return true;
    }

    private static int Walk(object obj, AssetBundle bundle, int depth)
    {
        if (depth > MaxDepth) return 0;

        var type = obj.GetType();
        if (GetPrefabRefFields(type) != null)
        {
            return Resolve(obj, bundle) ? 1 : 0;
        }

        int resolved = 0;
        foreach (var field in GetSerializedFields(type))
        {
            object? value;
            try
            {
                value = field.GetValue(obj);
            }
            catch
            {
                continue;
            }

            if (value == null || value is Object) continue;

            if (value is IList list)
            {
                foreach (var element in list)
                {
                    if (element != null && element is not Object) resolved += Walk(element, bundle, depth + 1);
                }
            }
            else
            {
                resolved += Walk(value, bundle, depth + 1);
            }
        }

        return resolved;
    }

    // Fields that can contain a PrefabRef: serializable classes and lists/arrays of them
    private static FieldInfo[] GetSerializedFields(Type type)
    {
        if (_serializedFields.TryGetValue(type, out var cached)) return cached;

        var fields = new List<FieldInfo>();
        for (var current = type; current != null && current != typeof(object) && current != typeof(MonoBehaviour) && current != typeof(ScriptableObject); current = current.BaseType)
        {
            foreach (var field in current.GetFields(InstanceFields | BindingFlags.DeclaredOnly))
            {
                if (field.IsNotSerialized) continue;
                if (!field.IsPublic && field.GetCustomAttribute<SerializeField>() == null) continue;
                if (CanContainPrefabRef(field.FieldType)) fields.Add(field);
            }
        }

        cached = fields.ToArray();
        _serializedFields[type] = cached;
        return cached;
    }

    private static bool CanContainPrefabRef(Type type)
    {
        if (type.IsArray) return CanContainPrefabRef(type.GetElementType()!);
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)) return CanContainPrefabRef(type.GetGenericArguments()[0]);

        return type.IsClass
            && type != typeof(string)
            && !typeof(Object).IsAssignableFrom(type)
            && type.IsSerializable;
    }

    private static (FieldInfo ResourcePath, FieldInfo Bundle)? GetPrefabRefFields(Type type)
    {
        if (_prefabRefFields.TryGetValue(type, out var cached)) return cached;

        var prefabRefType = type;
        while (prefabRefType != null && !(prefabRefType.IsGenericType && prefabRefType.GetGenericTypeDefinition() == typeof(PrefabRef<>)))
        {
            prefabRefType = prefabRefType.BaseType;
        }

        var resourcePathField = prefabRefType?.GetField("resourcePath", InstanceFields);
        var bundleField = prefabRefType?.GetField("bundle", InstanceFields);

        cached = resourcePathField != null && bundleField != null ? (resourcePathField, bundleField) : null;
        _prefabRefFields[type] = cached;
        return cached;
    }
}
