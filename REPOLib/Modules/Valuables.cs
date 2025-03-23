using REPOLib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace REPOLib.Modules;

public static class Valuables
{
    public static IReadOnlyList<GameObject> RegisteredValuables => _valuablesRegistered;
    public static IReadOnlyList<LevelValuables> ValuablePresets => _valuablePresets.Values.ToList();

    private static IEnumerable<GameObject> PendingAndRegisteredValuables => _valuablesToRegister.Keys.Concat(_valuablesRegistered);

    public static string GenericValuablePresetName => "Valuables - Generic";
    
    private static readonly Dictionary<string, LevelValuables> _valuablePresets = [];
    private static readonly Dictionary<GameObject, List<string>> _valuablesToRegister = [];
    private static readonly List<GameObject> _valuablesRegistered = [];

    private static bool _initialValuablesRegistered;

    private static void CacheValuablePresets()
    {
        if (RunManager.instance == null)
        {
            Logger.LogError($"Failed to cache LevelValuables. RunManager instance is null.");
            return;
        }

        foreach (var level in RunManager.instance.levels)
        {
            foreach (var valuablePreset in level.ValuablePresets)
            {
                _valuablePresets.TryAdd(valuablePreset.name, valuablePreset);
            }
        }
    }

    internal static void RegisterInitialValuables()
    {
        if (_initialValuablesRegistered)
        {
            return;
        }

        CacheValuablePresets();

        if (_valuablePresets.Count == 0)
        {
            Logger.LogError($"Failed to register valuables. LevelValuables list is empty!");
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

        if (!presetNames.Any(x => _valuablePresets.Keys.Any(y => x == y)))
        {
            Logger.LogError($"Valuable \"{valuable.name}\" does not have any valid valuable preset names set. Adding generic valuable preset name.");
            presetNames.Add(GenericValuablePresetName);
        }

        foreach (var presetName in presetNames)
        {
            if (presetName == null || !_valuablePresets.ContainsKey(presetName))
            {
                Logger.LogError($"Failed to add valuable \"{valuable.name}\" to valuable preset \"{presetName}\". The valuable preset does not exist.");
                continue;
            }

            if (_valuablePresets[presetName].AddValuable(valuable))
            {
                _valuablesRegistered.Add(valuable);
                Logger.LogInfo($"Added valuable \"{valuable.name}\" to valuable preset \"{presetName}\"", extended: true);
            }
            else
            {
                Logger.LogWarning($"Failed to add valuable \"{valuable.name}\" to valuable preset \"{presetName}\"", extended: true);
            }
        }
    }

    #region Public
    public static void RegisterValuable(GameObject prefab)
    {
        RegisterValuable(prefab, new List<string>());
    }

    public static void RegisterValuable(GameObject prefab, List<LevelValuables> presets)
    {
        RegisterValuable(prefab, (from preset in presets select preset.name).ToList());
    }

    public static void RegisterValuable(GameObject prefab, List<string> presetNames)
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

        RegisterValuable(valuableObject, presetNames);
    }

    public static void RegisterValuable(ValuableObject valuableObject)
    {
        RegisterValuable(valuableObject, new List<string>());
    }

    public static void RegisterValuable(ValuableObject valuableObject, List<LevelValuables> presets)
    {
        RegisterValuable(valuableObject, (from preset in presets select preset.name).ToList());
    }

    public static void RegisterValuable(ValuableObject valuableObject, List<string> presetNames)
    {
        if (valuableObject == null)
        {
            Logger.LogError($"Failed to register valuable. ValuableObject is null.");
            return;
        }

        GameObject prefab = valuableObject.gameObject;

        if (presetNames == null || presetNames.Count == 0)
        {
            //Logger.LogInfo($"No valuable presets specified for valuable \"{prefab.name}\". Adding valuable to generic preset.", extended: true);
            presetNames = [GenericValuablePresetName];
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
    }

    public static ValuableObject SpawnValuable(ValuableObject valuableObject, Vector3 position, Quaternion rotation)
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

        GameObject gameObject = NetworkPrefabs.SpawnNetworkPrefab(valuableObject.gameObject, position, rotation);

        Logger.LogInfo($"Spawned valuable \"{gameObject.name}\" at position {position}, rotation: {rotation.eulerAngles}", extended: true);

        return gameObject.GetComponent<ValuableObject>();
    }
    #endregion

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
