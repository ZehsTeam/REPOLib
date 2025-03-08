using REPOLib.Extensions;
using System;
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
    #region Original
    public static void RegisterValuable(GameObject prefab)
    {
        RegisterValuable(prefab?.name, prefab, new List<string>());
    }
    
    public static void RegisterValuable(string prefabId, GameObject prefab)
    {
        RegisterValuable(prefabId, prefab, new List<string>());
    }
    #endregion

    public static void RegisterValuable(GameObject prefab, List<LevelValuables> presets)
    {
        RegisterValuable(prefab?.name, prefab, presets);
    }

    public static void RegisterValuable(GameObject prefab, List<string> presetNames)
    {
        RegisterValuable(prefab?.name, prefab, presetNames);
    }

    public static void RegisterValuable(string prefabId, GameObject prefab, List<LevelValuables> presets)
    {
        RegisterValuable(prefabId, prefab, (from preset in presets select preset.name).ToList());
    }

    public static void RegisterValuable(string prefabId, GameObject prefab, List<string> presetNames)
    {
        if (presetNames == null || presetNames.Count == 0)
        {
            Logger.LogInfo($"No levels specified for \"{prefabId}\", adding to generic list.", extended: true);
            presetNames = ["Valuables - Generic"];
        }

        if (prefab == null)
        {
            throw new ArgumentException("Failed to register valuable. Prefab is null.");
        }

        if (string.IsNullOrWhiteSpace(prefabId))
        {
            throw new ArgumentException("Failed to register valuable. PrefabId is invalid.");
        }

        if (!_canRegisterValuables)
        {
            Logger.LogError($"Failed to register valuable \"{prefabId}\". You can only register valuables in awake!");
            return;
        }

        if (_valuablesToRegister.ContainsKey(prefab))
        {
            Logger.LogWarning($"Failed to register valuable \"{prefabId}\". This valuable is already registered!");
            return;
        }

        NetworkPrefabs.RegisterNetworkPrefab(prefabId, prefab);

        _valuablesToRegister.Add(prefab, presetNames);
    }
    #endregion
}
