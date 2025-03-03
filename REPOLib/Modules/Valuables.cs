using REPOLib.Extensions;
using REPOLib.Objects;
using System.Collections.Generic;
using UnityEngine;

namespace REPOLib.Modules;

public static class Valuables
{
    private static readonly HashSet<LevelValuables> _valuablePresets = new HashSet<LevelValuables>(new LevelValuableComparer());
    private static readonly List<GameObject> _valuablesToRegister = [];

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
        if (_registeredValuables)
        {
            return;
        }

        CacheValuablePresets();

        if (_valuablesToRegister.Count == 0)
        {
            Logger.LogError($"Failed to register valuables. LevelValuables list is empty!");
            return;
        }

        foreach (var valuable in _valuablesToRegister)
        {
            foreach (var preset in _valuablePresets)
            {
                preset.AddValuable(valuable);
            }
        }

        _valuablesToRegister.Clear();
        _registeredValuables = true;
    }

    public static void RegisterValuable(GameObject prefab)
    {
        RegisterValuable(prefab.name, prefab);
    }

    public static void RegisterValuable(string prefabId, GameObject prefab)
    {
        if (_registeredValuables)
        {
            Logger.LogError($"Failed to register valuable \"{prefabId}\". You can only register valuables in awake!");
            return;
        }

        if (_valuablesToRegister.Contains(prefab))
        {
            return;
        }

        NetworkPrefabs.RegisterNetworkPrefab(prefabId, prefab);

        _valuablesToRegister.Add(prefab);
    }
}
