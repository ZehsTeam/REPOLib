using BepInEx.Logging;
using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace REPOLib.Objects;

public class CustomPrefabPool : IPunPrefabPool
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

    private DefaultPool _defaultPool;

    public IPunPrefabPool OtherPool
    {
        get => _otherPool;
        set
        {
            if (value is DefaultPool || value is CustomPrefabPool)
            {
                return;
            }

            _otherPool = value;
        }
    }

    private IPunPrefabPool _otherPool;

    public CustomPrefabPool()
    {
    }
    
    public bool RegisterPrefab(string prefabId, GameObject prefab)
    {
        if (prefab == null)
        {
            throw new ArgumentException("CustomPrefabPool failed to register network prefab. Prefab is null.");
        }

        if (string.IsNullOrWhiteSpace(prefabId))
        {
            throw new ArgumentException("CustomPrefabPool failed to register network prefab. PrefabId is invalid.");
        }

        if (Prefabs.TryGetValue(prefabId, out GameObject value))
        {
            LogLevel logLevel = value == prefab ? LogLevel.Warning: LogLevel.Error;
            Logger.Log(logLevel, $"CustomPrefabPool failed to register network prefab. Network prefab already exists with the prefab id: \"{prefabId}\"");
            return false;
        }

        Prefabs[prefabId] = prefab;

        Logger.LogInfo($"CustomPrefabPool registered network prefab: \"{prefabId}\"", extended: true);
        return true;
    }

    public bool Contains(string prefabId)
    {
        return Prefabs.ContainsKey(prefabId);
    }

    public GameObject Instantiate(string prefabId, Vector3 position, Quaternion rotation)
    {
        GameObject result;

        if (!TryGetPrefab(ref prefabId, out GameObject prefab))
        {
            result = DefaultPool.Instantiate(prefabId, position, rotation);

            if (result == null && OtherPool != null)
            {
                result = OtherPool.Instantiate(prefabId, position, rotation);
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

        Logger.LogInfo($"CustomPrefabPool spawned network prefab: \"{prefabId}\" at position {position}, rotation {rotation.eulerAngles}", extended: true);

        return result;
    }

    public bool TryGetPrefab(ref string prefabId, out GameObject prefab)
    {
        if (Prefabs.TryGetValue(prefabId, out prefab))
        {
            return true;
        }

        string strippedPrefabId = prefabId.Substring(prefabId.LastIndexOf('/') + 1);

        if (Prefabs.TryGetValue(strippedPrefabId, out prefab))
        {
            prefabId = strippedPrefabId;
            return true;
        }

        return false;
    }

    public void Destroy(GameObject gameObject)
    {
        Object.Destroy(gameObject);
    }
}
