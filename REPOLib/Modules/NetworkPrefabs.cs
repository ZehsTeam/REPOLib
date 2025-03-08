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
        if (CustomPrefabPool.Contains(prefabId))
        {
            return;
        }
        
        CustomPrefabPool.RegisterPrefab(prefabId, prefab);
    }
}
