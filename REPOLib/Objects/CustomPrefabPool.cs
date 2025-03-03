using BepInEx.Logging;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

namespace REPOLib.Objects;

public class CustomPrefabPool : IPunPrefabPool
{
    public readonly Dictionary<string, GameObject> Prefabs = [];

    public IPunPrefabPool OriginalPool
    {
        get
        {
            _originalPool ??= new DefaultPool();
            return _originalPool;
        }
        set
        {
            if (value != null && value is not CustomPrefabPool)
            {
                _originalPool = value;
            }
        }
    }

    private IPunPrefabPool _originalPool;

    public CustomPrefabPool()
    {
    }

    public CustomPrefabPool(IPunPrefabPool existingPool)
    {
        OriginalPool = existingPool;
    }

    public void RegisterPrefab(string prefabId, GameObject prefab)
    {
        if (Prefabs.TryGetValue(prefabId, out GameObject value))
        {
            LogLevel logLevel = value == prefab ? LogLevel.Warning: LogLevel.Error;
            Logger.Log(logLevel, $"CustomPrefabPool failed to register network prefab. Network prefab already exists with the prefab id: \"{prefabId}\"");
            return;
        }

        Prefabs[prefabId] = prefab;

        Logger.LogInfo($"CustomPrefabPool registered network prefab: \"{prefabId}\"", extended: true);
    }

    public GameObject Instantiate(string prefabId, Vector3 position, Quaternion rotation)
    {
        if (!TryGetPrefab(ref prefabId, out GameObject prefab))
        {
            return OriginalPool.Instantiate(prefabId, position, rotation);
        }

        bool activeSelf = prefab.activeSelf;

        if (activeSelf)
        {
            prefab.SetActive(value: false);
        }

        GameObject result = Object.Instantiate(prefab, position, rotation);

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
