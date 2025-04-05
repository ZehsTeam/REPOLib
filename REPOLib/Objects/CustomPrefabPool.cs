using BepInEx.Logging;
using Photon.Pun;
using REPOLib.Extensions;
using REPOLib.Modules;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace REPOLib.Objects;

/// <summary>
/// REPOLib's custom prefab pool.
/// </summary>
public class CustomPrefabPool : IPunPrefabPool
{
    /// <summary>
    /// Registered custom network prefabs.
    /// </summary>
    public readonly Dictionary<string, GameObject> Prefabs = [];

    /// <summary>
    /// The DefaultPool.
    /// </summary>
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

    /// <inheritdoc cref="CustomPrefabPool"/>
    public CustomPrefabPool()
    {

    }

    /// <summary>
    /// Register a <see cref="GameObject"/> as a network prefab.
    /// </summary>
    /// <param name="prefabId">The ID for this <see cref="GameObject"/>.</param>
    /// <param name="prefab">The <see cref="GameObject"/> to register.</param>
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

        Logger.LogDebug($"CustomPrefabPool: registered network prefab \"{prefabId}\"", extended: true);
        return true;
    }

    /// <summary>
    /// Check if a <see cref="GameObject"/> is a network prefab.
    /// </summary>
    /// <param name="prefab">The <see cref="GameObject"/> to check.</param>
    /// <returns>Whether or not the <see cref="GameObject"/> is a network prefab.</returns>
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

    /// <summary>
    /// Check if a <see cref="GameObject"/> with the specified ID is a network prefab.
    /// </summary>
    /// <param name="prefabId">The <see cref="GameObject"/> ID to check.</param>
    /// <returns>Whether or not the <see cref="GameObject"/> is a network prefab.</returns>
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

    /// <summary>
    /// Gets the network prefab ID for a <see cref="GameObject"/> if it has one.
    /// </summary>
    /// <param name="prefab">The <see cref="GameObject"/> whose network prefab ID we want.</param>
    /// <returns>The network prefab ID or null.</returns>
    public string? GetPrefabId(GameObject prefab)
    {
        if (prefab == null)
        {
            Logger.LogError("Failed to get prefab id. GameObject is null.");
            return string.Empty;
        }

        return Prefabs.GetKeyOrDefault(prefab);
    }

    /// <summary>
    /// Gets the <see cref="GameObject"/> for a network prefab ID if it has one.
    /// </summary>
    /// <param name="prefabId">The network prefab ID whose <see cref="GameObject"/> we want.</param>
    /// <returns>The <see cref="GameObject"/> or null.</returns>
    public GameObject? GetPrefab(string prefabId)
    {
        return Prefabs.GetValueOrDefault(prefabId, ignoreKeyCase: true);
    }

    /// <summary>
    /// Spawns a network prefab.
    /// </summary>
    /// <param name="prefabId">The network prefab ID for the <see cref="GameObject"/> to spawn.</param>
    /// <param name="position">The position where the <see cref="GameObject"/> will be spawned.</param>
    /// <param name="rotation">The rotation of the <see cref="GameObject"/>.</param>
    /// <returns>The <see cref="GameObject"/> or null.</returns>
    /// <exception cref="ArgumentException"></exception>
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

    /// <summary>
    /// Destroys a <see cref="GameObject"/>.
    /// </summary>
    /// <param name="gameObject">The <see cref="GameObject"/> to destroy.</param>
    public void Destroy(GameObject gameObject)
    {
        Object.Destroy(gameObject);
    }
}
