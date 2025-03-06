using REPOLib.Extensions;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace REPOLib.Modules;

public static class Valuables
{
    public static IReadOnlyList<GameObject> RegisteredValuables => _valuablesRegistered;

    private static readonly Dictionary<string, LevelValuables> _valuablePresets = [];
    private static readonly Dictionary<GameObject, List<string>> _valuablesToRegister = [];
    private static readonly List<GameObject> _valuablesRegistered = [];

    private static bool _canRegisterValuables = true;

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

    internal static void RegisterValuables()
    {
        if (!_canRegisterValuables)
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
            if (_valuablesRegistered.Contains(valuable))
            {
                continue;
            }

            foreach (var presetName in _valuablesToRegister[valuable])
            {
                if (presetName == null || !_valuablePresets.ContainsKey(presetName))
                {
                    Logger.LogError($"Failed adding valuable \"{valuable.name}\" to preset \"{presetName}\". The preset does not exist.");
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

        _valuablesToRegister.Clear();
        _canRegisterValuables = false;
    }

    #region Public

    [Obsolete("", true)]
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
        RegisterValuable(prefabId, prefab, (from preset in presets select preset.name).ToList());
    }

    public static void RegisterValuable(string prefabId, GameObject prefab, List<string> presetNames = null)
    {
        if (presetNames == null)
        {
            presetNames = new List<string>() { "Valuables - Generic" };
        }
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
            presetNames = ["Valuables - Generic"];
        }

        if (!_canRegisterValuables)
        {
            Logger.LogError($"Failed to register valuable \"{prefab.name}\". You can only register valuables from your plugins awake!");
            return;
        }

        if (ResourcesHelper.HasValuablePrefab(valuableObject))
        {
            Logger.LogError($"Failed to register valuable \"{prefab.name}\". Valuable prefab already exists in Resources with the same name.");
            return;
        }

        if (_valuablesToRegister.ContainsKey(prefab))
        {
            Logger.LogWarning($"Failed to register valuable \"{prefab.name}\". Valuable is already registered!");
            return;
        }

        string prefabId = ResourcesHelper.GetValuablePrefabPath(valuableObject);
        NetworkPrefabs.RegisterNetworkPrefab(prefabId, prefab);

        _valuablesToRegister.Add(prefab, presetNames);
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
