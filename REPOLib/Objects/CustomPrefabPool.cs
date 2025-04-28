using BepInEx.Logging;
using Photon.Pun;
using REPOLib.Extensions;
using REPOLib.Modules;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace REPOLib.Objects;

internal class CustomPrefabPool : IPunPrefabPool
{
    public readonly Dictionary<string, GameObject> Prefabs = [];

    public DefaultPool DefaultPool
    {
        get
        {
            _defaultPool ??= new DefaultPool();
            return _defaultPool;
        }
        set
        {
            if (value != null)
            {
                _defaultPool = value;
            }
        }
    }

    private DefaultPool? _defaultPool;

    public CustomPrefabPool()
    {

    }

    public bool RegisterPrefab(string prefabId, GameObject prefab)
    {
        if (prefab == null)
        {
            throw new ArgumentException("CustomPrefabPool: failed to register network prefab. Prefab is null.");
        }

        if (string.IsNullOrWhiteSpace(prefabId))
        {
            throw new ArgumentException("CustomPrefabPool: failed to register network prefab. PrefabId is invalid.");
        }

        if (ResourcesHelper.HasPrefab(prefabId))
        {
            Logger.LogError($"CustomPrefabPool: failed to register network prefab \"{prefabId}\". Prefab already exists in Resources with the same prefab id.");
            return false;
        }

        if (Prefabs.TryGetValue(prefabId, out GameObject? value, ignoreKeyCase: true))
        {
            LogLevel logLevel = value == prefab ? LogLevel.Warning : LogLevel.Error;
            Logger.Log(logLevel, $"CustomPrefabPool: failed to register network prefab \"{prefabId}\". There is already a prefab registered with the same prefab id.");
            return false;
        }

        Prefabs[prefabId] = prefab;

        Logger.LogDebug($"CustomPrefabPool: registered network prefab \"{prefabId}\"");
        return true;
    }

    public bool HasPrefab(GameObject prefab)
    {
        if (Prefabs.ContainsValue(prefab))
        {
            return true;
        }

        if (ResourcesHelper.HasPrefab(prefab))
        {
            return true;
        }

        return false;
    }

    public bool HasPrefab(string prefabId)
    {
        if (Prefabs.ContainsKey(prefabId, ignoreKeyCase: true))
        {
            return true;
        }

        if (ResourcesHelper.HasPrefab(prefabId))
        {
            return true;
        }

        return false;
    }

    public string? GetPrefabId(GameObject prefab)
    {
        if (prefab == null)
        {
            Logger.LogError("Failed to get prefab id. GameObject is null.");
            return string.Empty;
        }

        return Prefabs.GetKeyOrDefault(prefab);
    }

    public GameObject? GetPrefab(string prefabId)
    {
        return Prefabs.GetValueOrDefault(prefabId, ignoreKeyCase: true);
    }

    public GameObject? Instantiate(string prefabId, Vector3 position, Quaternion rotation)
    {
        if (string.IsNullOrWhiteSpace(prefabId))
        {
            throw new ArgumentException("CustomPrefabPool: failed to spawn network prefab. PrefabId is null.");
        }

        if (position == null)
        {
            position = Vector3.zero;
            Logger.LogError($"CustomPrefabPool: tried to spawn network prefab \"{prefabId}\" with an invalid position. Using default position.");
        }

        if (rotation == null)
        {
            rotation = Quaternion.identity;
            Logger.LogError($"CustomPrefabPool: tried to spawn network prefab \"{prefabId}\" with an invalid rotation. Using default rotation.");
        }

        GameObject result;

        if (!Prefabs.TryGetValue(prefabId, out GameObject? prefab, ignoreKeyCase: true))
        {
            result = DefaultPool.Instantiate(prefabId, position, rotation);

            if (result == null)
            {
                Logger.LogError($"CustomPrefabPool: failed to spawn network prefab \"{prefabId}\". GameObject is null.");
            }

            return result;
        }

        bool activeSelf = prefab.activeSelf;

        if (activeSelf)
        {
            prefab.SetActive(value: false);
        }

        result = Object.Instantiate(prefab, position, rotation);

        if (activeSelf)
        {
            prefab.SetActive(value: true);
        }

        Logger.LogInfo($"CustomPrefabPool: spawned network prefab \"{prefabId}\" at position {position}, rotation {rotation.eulerAngles}", extended: true);

        return result;
    }

    public void Destroy(GameObject gameObject)
    {
        Object.Destroy(gameObject);
    }
}
