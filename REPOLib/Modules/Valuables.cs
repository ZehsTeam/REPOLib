using REPOLib.Extensions;
using REPOLib.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace REPOLib.Modules;

public static class Valuables
{
    public static IReadOnlyList<GameObject> AllValuables => GetValuables();
    public static IReadOnlyList<GameObject> RegisteredValuables => _valuablesRegistered;
    private static IEnumerable<GameObject> PendingAndRegisteredValuables => _valuablesToRegister.Keys.Concat(_valuablesRegistered);
    
    private static readonly Dictionary<GameObject, List<string>> _valuablesToRegister = [];
    private static readonly List<GameObject> _valuablesRegistered = [];

    private static bool _initialValuablesRegistered;
    
    internal static void RegisterInitialValuables()
    {
        if (_initialValuablesRegistered)
        {
            return;
        }

        Logger.LogInfo($"Adding valuables to valuable presets.");

        foreach (var valuable in _valuablesToRegister.Keys)
        {
            RegisterValuableWithGame(valuable);
        }

        _valuablesToRegister.Clear();
        _initialValuablesRegistered = true;
    }

    private static void RegisterValuableWithGame(GameObject valuable)
    {
        if (_valuablesRegistered.Contains(valuable))
        {
            return;
        }

        List<string> presetNames = _valuablesToRegister[valuable];

        if (!presetNames.Any(x => ValuablePresets.AllValuablePresets.Keys.Any(y => x == y)))
        {
            Logger.LogWarning($"Valuable \"{valuable.name}\" does not have any valid valuable preset names set. Adding generic valuable preset name.");
            presetNames.Add(ValuablePresets.GenericValuablePresetName);
        }

        foreach (var presetName in presetNames)
        {
            if (presetName == null || !ValuablePresets.AllValuablePresets.ContainsKey(presetName))
            {
                Logger.LogWarning($"Failed to add valuable \"{valuable.name}\" to valuable preset \"{presetName}\". The valuable preset does not exist.");
                continue;
            }

            if (ValuablePresets.AllValuablePresets[presetName].AddValuable(valuable))
            {
                _valuablesRegistered.Add(valuable);
                Logger.LogDebug($"Added valuable \"{valuable.name}\" to valuable preset \"{presetName}\"", extended: true);
            }
            else
            {
                Logger.LogWarning($"Failed to add valuable \"{valuable.name}\" to valuable preset \"{presetName}\"", extended: true);
            }
        }
    }

    public static void RegisterValuable(GameObject prefab)
    {
        RegisterValuable(prefab, [], ContentRegistry.GetAssemblySource(Assembly.GetCallingAssembly()));
    }

    public static void RegisterValuable(GameObject prefab, List<LevelValuables> presets)
    {
        RegisterValuable(
            prefab, 
            (from preset in presets select preset.name).ToList(), 
            ContentRegistry.GetAssemblySource(Assembly.GetCallingAssembly())
        );
    }

    public static void RegisterValuable(GameObject prefab, List<string> presetNames)
    {
        RegisterValuable(prefab, presetNames, ContentRegistry.GetAssemblySource(Assembly.GetCallingAssembly()));
    }

    private static void RegisterValuable(GameObject prefab, List<string> presetNames, IContentSource source)
    {
        if (prefab == null)
        {
            Logger.LogError($"Failed to register valuable. Prefab is null.");
            return;
        }

        if (!prefab.TryGetComponent(out ValuableObject valuableObject))
        {
            Logger.LogError($"Failed to register valuable. Prefab does not have a ValuableObject component.");
            return;
        }

        RegisterValuable(valuableObject, presetNames, source);
    }

    public static void RegisterValuable(ValuableObject valuableObject)
    {
        RegisterValuable(valuableObject, [], ContentRegistry.GetAssemblySource(Assembly.GetCallingAssembly()));
    }

    public static void RegisterValuable(ValuableObject valuableObject, List<LevelValuables> presets)
    {
        RegisterValuable(
            valuableObject, 
            (from preset in presets select preset.name).ToList(), 
            ContentRegistry.GetAssemblySource(Assembly.GetCallingAssembly())
        );
    }

    public static void RegisterValuable(ValuableObject valuableObject, List<string> presetNames)
    {
        RegisterValuable(valuableObject, presetNames, ContentRegistry.GetAssemblySource(Assembly.GetCallingAssembly()));
    }

    private static void RegisterValuable(ValuableObject valuableObject, List<string> presetNames, IContentSource source)
    {
        if (valuableObject == null)
        {
            Logger.LogError($"Failed to register valuable. ValuableObject is null.");
            return;
        }

        GameObject prefab = valuableObject.gameObject;

        if (presetNames == null || presetNames.Count == 0)
        {
            Logger.LogWarning($"Valuable \"{valuableObject.name}\" does not have any valid valuable preset names set. Adding generic valuable preset name.", extended: true);
            presetNames = [ValuablePresets.GenericValuablePresetName];
        }

        if (ResourcesHelper.HasValuablePrefab(valuableObject))
        {
            Logger.LogError($"Failed to register valuable \"{prefab.name}\". Valuable prefab already exists in Resources with the same name.");
            return;
        }

        if (PendingAndRegisteredValuables.Any(x => x.name.Equals(prefab.name, StringComparison.OrdinalIgnoreCase)))
        {
            Logger.LogError($"Failed to register valuable \"{prefab.name}\". Valuable prefab already exists with the same name.");
            return;
        }

        if (PendingAndRegisteredValuables.Contains(prefab))
        {
            Logger.LogWarning($"Failed to register valuable \"{prefab.name}\". Valuable is already registered!");
            return;
        }

        string prefabId = ResourcesHelper.GetValuablePrefabPath(valuableObject);
        NetworkPrefabs.RegisterNetworkPrefab(prefabId, prefab);

        Utilities.FixAudioMixerGroups(prefab);

        _valuablesToRegister.Add(prefab, presetNames);

        if (_initialValuablesRegistered)
        {
            RegisterValuableWithGame(valuableObject.gameObject);
        }
        
        ContentRegistry.Add(valuableObject, source);
    }

    public static GameObject? SpawnValuable(ValuableObject valuableObject, Vector3 position, Quaternion rotation)
    {
        if (valuableObject == null)
        {
            Logger.LogError("Failed to spawn valuable. ValuableObject is null.");
            return null;
        }

        if (!SemiFunc.IsMasterClientOrSingleplayer())
        {
            Logger.LogError($"Failed to spawn valuable \"{valuableObject.gameObject.name}\". You are not the host.");
            return null;
        }

        string prefabId = ResourcesHelper.GetValuablePrefabPath(valuableObject);
        GameObject? gameObject = NetworkPrefabs.SpawnNetworkPrefab(prefabId, position, rotation);

        if (gameObject == null)
        {
            Logger.LogError($"Failed to spawn valuable \"{valuableObject.gameObject.name}\"");
            return null;
        }

        Logger.LogInfo($"Spawned valuable \"{gameObject.name}\" at position {position}, rotation: {rotation.eulerAngles}", extended: true);

        return gameObject;
    }

    public static IReadOnlyList<GameObject> GetValuables()
    {
        if (RunManager.instance == null)
        {
            return [];
        }

        return ValuablePresets.AllValuablePresets.Values
            .Select(levelValuables => levelValuables.GetCombinedList())
            .SelectMany(list => list)
            .Distinct()
            .ToList();
    }

    public static bool TryGetValuableByName(string name, [NotNullWhen(true)] out ValuableObject? valuableObject)
    {
        valuableObject = GetValuableByName(name);
        return valuableObject != null;
    }

    public static ValuableObject? GetValuableByName(string name)
    {
        foreach (var gameObject in GetValuables())
        {
            if (!gameObject.TryGetComponent(out ValuableObject valuableObject))
            {
                continue;
            }

            if (gameObject.name.EqualsAny([name, $"Valuable {name}"], StringComparison.OrdinalIgnoreCase))
            {
                return valuableObject;
            }
        }

        return default;
    }

    public static bool TryGetValuableThatContainsName(string name, [NotNullWhen(true)] out ValuableObject? valuableObject)
    {
        valuableObject = GetValuableThatContainsName(name);
        return valuableObject != null;
    }

    public static ValuableObject? GetValuableThatContainsName(string name)
    {
        foreach (var gameObject in GetValuables())
        {
            if (!gameObject.TryGetComponent(out ValuableObject valuableObject))
            {
                continue;
            }

            if (gameObject.name.Contains(name, StringComparison.OrdinalIgnoreCase))
            {
                return valuableObject;
            }
        }

        return default;
    }

    #region Deprecated
    [Obsolete("prefabId is no longer supported", true)]
    public static void RegisterValuable(string prefabId, GameObject prefab)
    {
        RegisterValuable(prefab);
    }

    [Obsolete("prefabId is no longer supported", true)]
    public static void RegisterValuable(string prefabId, GameObject prefab, List<LevelValuables> presets)
    {
        RegisterValuable(prefab, presets);
    }

    [Obsolete("prefabId is no longer supported", true)]
    public static void RegisterValuable(string prefabId, GameObject prefab, List<string> presetNames)
    {
        RegisterValuable(prefab, presetNames);
    }
    #endregion
}
