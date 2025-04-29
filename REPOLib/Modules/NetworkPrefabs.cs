using Photon.Pun;
using REPOLib.Objects;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using UnityEngine;

namespace REPOLib.Modules;

/// <summary>
/// The NetworkPrefabs module of REPOLib.
/// </summary>
[PublicAPI]
public static class NetworkPrefabs
{
    internal static CustomPrefabPool CustomPrefabPool
    {
        get => _customPrefabPool ??= new CustomPrefabPool();
        private set => _customPrefabPool = value;
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
            CustomPrefabPool.DefaultPool = defaultPool;
        else if (PhotonNetwork.PrefabPool is not Objects.CustomPrefabPool)
            Logger.LogWarning($"PhotonNetwork has an unknown prefab pool assigned. PhotonNetwork.PrefabPool = {PhotonNetwork.PrefabPool.GetType()}");

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
        => RegisterNetworkPrefab(prefab?.name!, prefab!);

    /// <summary>
    /// Register a <see cref="GameObject"/> as a network prefab.
    /// </summary>
    /// <param name="prefabId">The ID for this <see cref="GameObject"/>.</param>
    /// <param name="prefab">The <see cref="GameObject"/> to register.</param>
    public static void RegisterNetworkPrefab(string prefabId, GameObject prefab)
        => CustomPrefabPool.RegisterPrefab(prefabId, prefab);

    /// <summary>
    /// Check if a <see cref="GameObject"/> with the specified ID is a network prefab.
    /// </summary>
    /// <param name="prefabId">The <see cref="GameObject"/> ID to check.</param>
    /// <returns>Whether the <see cref="GameObject"/> is a network prefab.</returns>
    public static bool HasNetworkPrefab(string prefabId)
        => CustomPrefabPool.HasPrefab(prefabId);

    /// <summary>
    /// Gets a network prefab.
    /// </summary>
    /// <param name="prefabId">The network prefab ID for the <see cref="GameObject"/>.</param>
    /// <returns>The <see cref="GameObject"/> or null.</returns>
    public static GameObject? GetNetworkPrefab(string prefabId)
        => CustomPrefabPool.GetPrefab(prefabId);

    /// <summary>
    /// Tries to get a network prefab.
    /// </summary>
    /// <param name="prefabId">The network prefab ID for the <see cref="GameObject"/>.</param>
    /// <param name="prefab">The network prefab <see cref="GameObject"/>.</param>
    /// <returns>Whether the <see cref="GameObject"/> was found.</returns>
    public static bool TryGetNetworkPrefab(string prefabId, [NotNullWhen(true)] out GameObject? prefab)
        => (prefab = GetNetworkPrefab(prefabId)) != null;

    /// <summary>
    /// Spawns a network prefab.
    /// </summary>
    /// <param name="prefabId">The network prefab ID for the <see cref="GameObject"/> to spawn.</param>
    /// <param name="position">The position where the <see cref="GameObject"/> will be spawned.</param>
    /// <param name="rotation">The rotation of the <see cref="GameObject"/>.</param>
    /// <param name="group">The interest group. See: https://doc.photonengine.com/pun/current/gameplay/interestgroups</param>
    /// <param name="data">Custom instantiation data. See: https://doc.photonengine.com/pun/current/gameplay/instantiation#custom-instantiation-data</param>
    /// <returns>The spawned <see cref="GameObject"/> or null.</returns>
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

        if (SemiFunc.IsMasterClientOrSingleplayer())
            return SemiFunc.IsMultiplayer()
                ? PhotonNetwork.InstantiateRoomObject(prefabId, position, rotation, group, data)
                : Object.Instantiate(CustomPrefabPool.GetPrefab(prefabId), position, rotation);
        
        Logger.LogError($"Failed to spawn network prefab \"{prefabId}\". You are not the host.");
        return null;
    }
}
