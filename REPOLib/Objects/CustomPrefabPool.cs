using BepInEx.Logging;
using Photon.Pun;
using REPOLib.Extensions;
using REPOLib.Modules;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using Object = UnityEngine.Object;

namespace REPOLib.Objects;

internal class CustomPrefabPool : IPunPrefabPool
{
    private readonly Dictionary<string, GameObject> _prefabs = [];
    private DefaultPool? _defaultPool;

    public DefaultPool? DefaultPool
    {
        get => _defaultPool ??= new DefaultPool();
        set => _defaultPool = value ?? _defaultPool;
    }

    public bool RegisterPrefab(string prefabId, GameObject prefab)
    {
        if (prefab == null)
            throw new ArgumentException("CustomPrefabPool: failed to register network prefab. Prefab is null.");

        if (string.IsNullOrWhiteSpace(prefabId))
            throw new ArgumentException("CustomPrefabPool: failed to register network prefab. PrefabId is invalid.");
        
        if (ResourcesHelper.HasPrefab(prefabId))
        {
            Logger.LogError($"CustomPrefabPool: failed to register network prefab \"{prefabId}\". Prefab already exists in Resources with the same prefab id.");
            return false;
        }

        if (_prefabs.TryGetValue(prefabId, out GameObject? value, ignoreKeyCase: true))
        {
            var logLevel = value == prefab ? LogLevel.Warning : LogLevel.Error;
            Logger.Log(logLevel, $"CustomPrefabPool: failed to register network prefab \"{prefabId}\". There is already a prefab registered with the same prefab id.");
            return false;
        }

        _prefabs[prefabId] = prefab;

        Logger.LogDebug($"CustomPrefabPool: registered network prefab \"{prefabId}\"");
        return true;
    }

    public bool HasPrefab(GameObject prefab) 
        => _prefabs.ContainsValue(prefab) || ResourcesHelper.HasPrefab(prefab);
    
    public bool HasPrefab(string prefabId)
        => GetPrefab(prefabId) != null;

    public string? GetPrefabId(GameObject prefab)
    {
        if (prefab != null) return _prefabs.GetKeyOrDefault(prefab);
        Logger.LogError("Failed to get prefab id. GameObject is null.");
        return string.Empty;
    }

    public GameObject? GetPrefab(string prefabId) 
        => _prefabs.TryGetValue(prefabId, out var prefab, ignoreKeyCase: true) ? prefab : Resources.Load<GameObject>(prefabId);

    public GameObject? Instantiate(string prefabId, Vector3 position, Quaternion rotation)
    {
        if (string.IsNullOrWhiteSpace(prefabId))
            throw new ArgumentException("CustomPrefabPool: failed to spawn network prefab. PrefabId is null.");

        GameObject? result;
        if (!_prefabs.TryGetValue(prefabId, out var prefab, ignoreKeyCase: true))
        {
            result = DefaultPool?.Instantiate(prefabId, position, rotation);
            
            if (result == null) Logger.LogError($"CustomPrefabPool: failed to spawn network prefab \"{prefabId}\". GameObject is null.");
            return result;
        }

        var activeSelf = prefab.activeSelf;
        if (activeSelf) prefab.SetActive(value: false);

        result = Object.Instantiate(prefab, position, rotation);
        if (activeSelf) prefab.SetActive(value: true);
        
        Logger.LogInfo($"CustomPrefabPool: spawned network prefab \"{prefabId}\" at position {position}, rotation {rotation.eulerAngles}", extended: true);
        return result;
    }

    public void Destroy(GameObject gameObject) => Object.Destroy(gameObject);
}
