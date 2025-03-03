using REPOLib.Extensions;
using REPOLib.Objects;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace REPOLib.Modules;

public static class Valuables
{
    public static IReadOnlyList<GameObject> RegisteredValuables => _valuablesRegistered;

    private static readonly HashSet<LevelValuables> _valuablePresets = new HashSet<LevelValuables>(new LevelValuableComparer());
    private static readonly List<GameObject> _valuablesToRegister = [];
    private static readonly List<GameObject> _valuablesRegistered = [];

    private static bool _canRegisterValuables = true;

    [Obsolete("", true)]
    private static bool _registeredValuables = false;

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
                _valuablePresets.Add(valuablePreset);
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

        foreach (var valuable in _valuablesToRegister)
        {
            if (_valuablesRegistered.Contains(valuable))
            {
                continue;
            }

            foreach (var preset in _valuablePresets)
            {
                if (preset.AddValuable(valuable))
                {
                    _valuablesRegistered.Add(valuable);
                    Logger.LogInfo($"Added valuable \"{valuable.name}\" to valuable preset \"{preset.name}\"", extended: true);
                }
                else
                {
                    Logger.LogWarning($"Failed to add valuable \"{valuable.name}\" to valuable preset \"{preset.name}\"", extended: true);
                }
            }
        }

        _valuablesToRegister.Clear();
        _canRegisterValuables = false;
    }

    public static void RegisterValuable(GameObject prefab)
    {
        RegisterValuable(prefab?.name, prefab);
    }

    public static void RegisterValuable(string prefabId, GameObject prefab)
    {
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

        if (_valuablesToRegister.Contains(prefab))
        {
            Logger.LogWarning($"Failed to register valuable \"{prefabId}\". This valuable is already registered!");
            return;
        }

        NetworkPrefabs.RegisterNetworkPrefab(prefabId, prefab);

        _valuablesToRegister.Add(prefab);
    }
}
