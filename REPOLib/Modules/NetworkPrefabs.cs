using Photon.Pun;
using REPOLib.Objects;
using UnityEngine;

namespace REPOLib.Modules;

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
    
    private static CustomPrefabPool _customPrefabPool;

    internal static void Initialize()
    {
        if (PhotonNetwork.PrefabPool is CustomPrefabPool)
        {
            Logger.LogWarning("NetworkPrefabs failed to initialize. PhotonNetwork.PrefabPool is already a CustomPrefabPool.");
            return;
        }

        Logger.LogInfo($"Initializing NetworkPrefabs.");
        Logger.LogInfo($"PhotonNetwork.PrefabPool = {PhotonNetwork.PrefabPool.GetType()}", extended: true);

        if (PhotonNetwork.PrefabPool is DefaultPool defaultPool)
        {
            CustomPrefabPool.DefaultPool = defaultPool;
        }
        else
        {
            CustomPrefabPool.OtherPool = PhotonNetwork.PrefabPool;
        }

        PhotonNetwork.PrefabPool = CustomPrefabPool;

        Logger.LogInfo("Replaced PhotonNetwork.PrefabPool with CustomPrefabPool.");
        Logger.LogInfo($"PhotonNetwork.PrefabPool = {PhotonNetwork.PrefabPool.GetType()}", extended: true);
        Logger.LogInfo($"Finished initializing NetworkPrefabs.");
    }

    public static void RegisterNetworkPrefab(GameObject prefab)
    {
        RegisterNetworkPrefab(prefab?.name, prefab);
    }

    public static void RegisterNetworkPrefab(string prefabId, GameObject prefab)
    {
        CustomPrefabPool.RegisterPrefab(prefabId, prefab);
    }

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

    public static GameObject SpawnNetworkPrefab(string prefabId, Vector3 position, Quaternion rotation, byte group = 0, object[] data = null)
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
