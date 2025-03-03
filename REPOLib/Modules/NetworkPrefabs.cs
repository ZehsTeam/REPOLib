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
        Logger.LogInfo($"Initializing NetworkPrefabs.");

        if (PhotonNetwork.PrefabPool == null)
        {
            Logger.LogError("NetworkPrefabs failed to initialize. PhotonNetwork.PrefabPool is null.");
            return;
        }

        if (PhotonNetwork.PrefabPool is CustomPrefabPool)
        {
            Logger.LogWarning("NetworkPrefabs failed to initialize. PhotonNetwork.PrefabPool is already a CustomPrefabPool.");
            return;
        }

        Logger.LogInfo($"PhotonNetwork.PrefabPool = {PhotonNetwork.PrefabPool.GetType()}", extended: true);

        CustomPrefabPool.OriginalPool = PhotonNetwork.PrefabPool;
        PhotonNetwork.PrefabPool = CustomPrefabPool;

        Logger.LogInfo("Replaced PhotonNetwork.PrefabPool with CustomPrefabPool.");
        Logger.LogInfo($"PhotonNetwork.PrefabPool = {PhotonNetwork.PrefabPool.GetType()}", extended: true);
        Logger.LogInfo($"Finished initializing NetworkPrefabs.");
    }

    public static void RegisterNetworkPrefab(GameObject prefab)
    {
        RegisterNetworkPrefab(prefab.name, prefab);
    }

    public static void RegisterNetworkPrefab(string prefabId, GameObject prefab)
    {
        CustomPrefabPool.RegisterPrefab(prefabId, prefab);
    }
}
