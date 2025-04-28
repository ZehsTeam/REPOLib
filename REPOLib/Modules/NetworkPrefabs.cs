using Photon.Pun;
using REPOLib.Objects;
using UnityEngine;

namespace REPOLib.Modules;

/// <summary>
/// The NetworkPrefabs module of REPOLib.
/// </summary>
public static class NetworkPrefabs
{
    internal static CustomPrefabPool CustomPrefabPool
    {
        get
        {
            _customPrefabPool ??= new CustomPrefabPool();
            return _customPrefabPool;
        }
        private set
        {
            _customPrefabPool = value;
        }
    }

    private static CustomPrefabPool? _customPrefabPool;

    internal static void Initialize()
    {
        if (PhotonNetwork.PrefabPool is CustomPrefabPool)
        {
            Logger.LogWarning("NetworkPrefabs failed to initialize. PhotonNetwork.PrefabPool is already a CustomPrefabPool.");
            return;
        }

        Logger.LogInfo($"Initializing NetworkPrefabs.");
        Logger.LogDebug($"PhotonNetwork.PrefabPool = {PhotonNetwork.PrefabPool.GetType()}");

        if (PhotonNetwork.PrefabPool is DefaultPool defaultPool)
        {
            CustomPrefabPool.DefaultPool = defaultPool;
        }
        else if (PhotonNetwork.PrefabPool is not Objects.CustomPrefabPool)
        {
            Logger.LogWarning($"PhotonNetwork has an unknown prefab pool assigned. PhotonNetwork.PrefabPool = {PhotonNetwork.PrefabPool.GetType()}");
        }

        PhotonNetwork.PrefabPool = CustomPrefabPool;

        Logger.LogInfo("Replaced PhotonNetwork.PrefabPool with CustomPrefabPool.");
        Logger.LogDebug($"PhotonNetwork.PrefabPool = {PhotonNetwork.PrefabPool.GetType()}");
        Logger.LogInfo($"Finished initializing NetworkPrefabs.");
    }

    /// <summary>
    /// Register a <see cref="GameObject"/> as a network prefab.
    /// </summary>
    /// <param name="prefab">The <see cref="GameObject"/> to register.</param>
    public static void RegisterNetworkPrefab(GameObject prefab)
    {
        RegisterNetworkPrefab(prefab?.name!, prefab!);
    }

    /// <summary>
    /// Register a <see cref="GameObject"/> as a network prefab.
    /// </summary>
    /// <param name="prefabId">The ID for this <see cref="GameObject"/>.</param>
    /// <param name="prefab">The <see cref="GameObject"/> to register.</param>
    public static void RegisterNetworkPrefab(string prefabId, GameObject prefab)
    {
        CustomPrefabPool.RegisterPrefab(prefabId, prefab);
    }

    /// <summary>
    /// Check if a <see cref="GameObject"/> with the specified ID is a network prefab.
    /// </summary>
    /// <param name="prefabId">The <see cref="GameObject"/> ID to check.</param>
    /// <returns>Whether or not the <see cref="GameObject"/> is a network prefab.</returns>
    public static bool HasNetworkPrefab(string prefabId)
    {
        return CustomPrefabPool.HasPrefab(prefabId);
    }

    #region Disabled
    //public static bool HasNetworkPrefab(GameObject prefab)
    //{
    //    return CustomPrefabPool.HasPrefab(prefab);
    //}

    //public static string GetNetworkPrefabId(GameObject prefab)
    //{
    //    return CustomPrefabPool.GetPrefabId(prefab);
    //}

    //public static bool TryGetNetworkPrefabId(GameObject prefab, out string prefabId)
    //{
    //    prefabId = GetNetworkPrefabId(prefab);
    //    return !string.IsNullOrEmpty(prefabId);
    //}

    //public static GameObject SpawnNetworkPrefab(GameObject prefab, Vector3 position, Quaternion rotation, byte group = 0, object[] data = null)
    //{
    //    if (prefab == null)
    //    {
    //        Logger.LogError("Failed to spawn network prefab. GameObject is null.");
    //        return null;
    //    }

    //    if (!TryGetNetworkPrefabId(prefab, out string prefabId))
    //    {
    //        Logger.LogError($"Failed to spawn network prefab \"{prefab.name}\". GameObject is not registered as a network prefab.");
    //        return null;
    //    }

    //    return SpawnNetworkPrefab(prefabId, position, rotation, group, data);
    //}
    #endregion

    /// <summary>
    /// Spawns a network prefab.
    /// </summary>
    /// <param name="prefabId">The network prefab ID for the <see cref="GameObject"/> to spawn.</param>
    /// <param name="position">The position where the <see cref="GameObject"/> will be spawned.</param>
    /// <param name="rotation">The rotation of the <see cref="GameObject"/>.</param>
    /// <param name="group">The interest group. See: https://doc.photonengine.com/pun/current/gameplay/interestgroups</param>
    /// <param name="data">Custom instantiation data. See: https://doc.photonengine.com/pun/current/gameplay/instantiation#custom-instantiation-data</param>
    /// <returns>The <see cref="GameObject"/> or null.</returns>
    public static GameObject? SpawnNetworkPrefab(string prefabId, Vector3 position, Quaternion rotation, byte group = 0, object[]? data = null)
    {
        if (string.IsNullOrWhiteSpace(prefabId))
        {
            Logger.LogError("Failed to spawn network prefab. PrefabId is null.");
            return null;
        }

        if (!HasNetworkPrefab(prefabId))
        {
            Logger.LogError($"Failed to spawn network prefab \"{prefabId}\". PrefabId is not registered as a network prefab.");
            return null;
        }

        if (!SemiFunc.IsMasterClientOrSingleplayer())
        {
            Logger.LogError($"Failed to spawn network prefab \"{prefabId}\". You are not the host.");
            return null;
        }

        if (SemiFunc.IsMultiplayer())
        {
            return PhotonNetwork.InstantiateRoomObject(prefabId, position, rotation, group, data);
        }
        else
        {
            return Object.Instantiate(CustomPrefabPool.GetPrefab(prefabId), position, rotation);
        }
    }
}
