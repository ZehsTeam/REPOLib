using Photon.Pun;
using REPOLib.Objects;
using REPOLib.Objects.Sdk;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace REPOLib.Modules;

/// <summary>
/// The NetworkPrefabs module of REPOLib.
/// </summary>
public static class NetworkPrefabs
{
    /// <summary>
    /// Gets a read-only dictionary mapping network prefab IDs to their corresponding <see cref="GameObject"/>s.
    /// </summary>
    public static IReadOnlyDictionary<string, GameObject> Prefabs => _prefabs;
    private static readonly Dictionary<string, GameObject> _prefabs = [];

    /// <summary>
    /// Gets a read-only dictionary mapping network prefab IDs to their corresponding <see cref="PrefabRef"/> objects.
    /// </summary>
    public static IReadOnlyDictionary<string, PrefabRef> PrefabRefs => _prefabRefs;
    private static readonly Dictionary<string, PrefabRef> _prefabRefs = [];

    /// <inheritdoc cref="RegisterNetworkPrefab(string, GameObject)"/>
    public static PrefabRef? RegisterNetworkPrefab(GameObject? prefab)
    {
        string prefabId = prefab?.name ?? string.Empty;
        return RegisterNetworkPrefab(prefabId, prefab);
    }

    /// <summary>
    /// Register a <see cref="GameObject"/> as a network prefab.
    /// </summary>
    /// <param name="prefabId">The ID for this <see cref="GameObject"/>.</param>
    /// <param name="prefab">The <see cref="GameObject"/> to register.</param>
    /// <returns>The registered network <see cref="PrefabRef"/> or null.</returns>
    public static PrefabRef? RegisterNetworkPrefab(string prefabId, GameObject? prefab)
    {
        PrefabRefResponse PrefabRefResponse = RegisterNetworkPrefabInternal(prefabId, prefab);

        switch (PrefabRefResponse.Result)
        {
            case PrefabRefResult.Success:
                return PrefabRefResponse.PrefabRef;
            case PrefabRefResult.PrefabIdNullOrEmpty:
                Logger.LogError("Failed to register network prefab. PrefabId is null.");
                return null;
            case PrefabRefResult.PrefabNull:
                Logger.LogError("Failed to register network prefab. Prefab is null.");
                return null;
            case PrefabRefResult.PrefabAlreadyRegistered:
                Logger.LogWarning($"Failed to register network prefab \"{prefabId}\". Prefab is already registered.");
                return PrefabRefResponse.PrefabRef;
            case PrefabRefResult.DifferentPrefabAlreadyRegistered:
                Logger.LogError($"Failed to register network prefab \"{prefabId}\". A prefab is already registered with the same ID. (GameObject: \"{PrefabRefResponse.PrefabRef!.Prefab.name}\")");
                return null;
            default:
                return PrefabRefResponse.PrefabRef;
        }
    }

    /// <inheritdoc cref="RegisterNetworkPrefab(string, GameObject)"/>
    /// <param name="networkPrefabContent">The <see cref="NetworkPrefabContent"/> to register.</param>
    public static PrefabRef? RegisterNetworkPrefab(NetworkPrefabContent? networkPrefabContent)
    {
        if (networkPrefabContent == null)
        {
            Logger.LogError("Failed to register network prefab. NetworkPrefabContent is null.");
            return null;
        }

        return RegisterNetworkPrefab(networkPrefabContent.Prefab);
    }

    internal static PrefabRefResponse RegisterNetworkPrefabInternal(string prefabId, GameObject? prefab)
    {
        if (prefab == null)
        {
            return new PrefabRefResponse(PrefabRefResult.PrefabNull, null);
        }

        if (string.IsNullOrEmpty(prefabId))
        {
            return new PrefabRefResponse(PrefabRefResult.PrefabIdNullOrEmpty, null);
        }

        PrefabRef? existingPrefabRef = GetNetworkPrefabRef(prefabId);

        if (existingPrefabRef != null)
        {
            GameObject existingPrefab = existingPrefabRef.Prefab;

            if (prefab == existingPrefab)
            {
                return new PrefabRefResponse(PrefabRefResult.PrefabAlreadyRegistered, existingPrefabRef);
            }
            else
            {
                return new PrefabRefResponse(PrefabRefResult.DifferentPrefabAlreadyRegistered, existingPrefabRef);
            }
        }

        var prefabRef = new PrefabRef
        {
            prefabName = prefab.name,
            resourcePath = prefabId
        };

        _prefabs.Add(prefabId, prefab);
        _prefabRefs.Add(prefabId, prefabRef);

        return new PrefabRefResponse(PrefabRefResult.Success, prefabRef);
    }

    /// <summary>
    /// Check if a network prefab with the specified ID is registered.
    /// </summary>
    /// <param name="prefabId">The <see cref="PrefabRef"/> ID to check.</param>
    /// <returns>Whether or not the network prefab is registered.</returns>
    public static bool HasNetworkPrefab(string prefabId)
    {
        return _prefabs.ContainsKey(prefabId);
    }

    /// <summary>
    /// Gets a network <see cref="PrefabRef"/>.
    /// </summary>
    /// <param name="prefabId">The ID for the network <see cref="PrefabRef"/>.</param>
    /// <returns>The network <see cref="PrefabRef"/> or null.</returns>
    public static PrefabRef? GetNetworkPrefabRef(string prefabId)
    {
        return _prefabRefs.GetValueOrDefault(prefabId);
    }

    /// <summary>
    /// Tries to get a network <see cref="PrefabRef"/>.
    /// </summary>
    /// <param name="prefabId">The ID for the network <see cref="PrefabRef"/>.</param>
    /// <param name="prefabRef">The network <see cref="PrefabRef"/>.</param>
    /// <returns>Whether or not the network <see cref="PrefabRef"/> was found.</returns>
    public static bool TryGetNetworkPrefabRef(string prefabId, [NotNullWhen(true)] out PrefabRef? prefabRef)
    {
        prefabRef = GetNetworkPrefabRef(prefabId);
        return prefabRef != null;
    }

    internal static bool TryGetNetworkPrefab(string prefabId, [NotNullWhen(true)] out GameObject? prefab)
    {
        prefab = _prefabs.GetValueOrDefault(prefabId);
        return prefab != null;
    }

    /// <summary>
    /// Spawns a network prefab.
    /// </summary>
    /// <param name="prefabRef">The <see cref="PrefabRef"/> for the <see cref="GameObject"/> to spawn.</param>
    /// <param name="position">The position where the <see cref="GameObject"/> will be spawned.</param>
    /// <param name="rotation">The rotation of the <see cref="GameObject"/>.</param>
    /// <param name="group">The interest group. See: https://doc.photonengine.com/pun/current/gameplay/interestgroups</param>
    /// <param name="data">Custom instantiation data. See: https://doc.photonengine.com/pun/current/gameplay/instantiation#custom-instantiation-data</param>
    /// <returns>The spawned <see cref="GameObject"/> or null.</returns>
    public static GameObject? SpawnNetworkPrefab(PrefabRef? prefabRef, Vector3 position, Quaternion rotation, byte group = 0, object[]? data = null)
    {
        if (prefabRef == null)
        {
            Logger.LogError("Failed to spawn network prefab. PrefabRef is null.");
            return null;
        }

        if (!prefabRef.IsValid())
        {
            Logger.LogError("Failed to spawn network prefab. PrefabRef is not valid.");
            return null;
        }

        if (!SemiFunc.IsMasterClientOrSingleplayer())
        {
            Logger.LogError($"Failed to spawn network prefab \"{prefabRef.PrefabName}\". You are not the host.");
            return null;
        }

        if (SemiFunc.IsMultiplayer())
        {
            return PhotonNetwork.InstantiateRoomObject(prefabRef.ResourcePath, position, rotation, group, data);
        }
        else
        {
            return Object.Instantiate(prefabRef.Prefab, position, rotation);
        }
    }
}

/// <summary>
/// A component for easily spawning a network prefab.
/// </summary>
public class REPOLibSpawnNetworkPrefab : MonoBehaviour
{
    #pragma warning disable CS0649 // Field 'field' is never assigned to, and will always have its default value 'value'
    [SerializeField]
    private PrefabRef? _prefabRef;

    [SerializeField]
    private Transform? _positionTransform;
    #pragma warning restore CS0649 // Field 'field' is never assigned to, and will always have its default value 'value'

    /// <summary>
    /// The <see cref="global::PrefabRef"/> of your network prefab.
    /// </summary>
    public PrefabRef? PrefabRef => _prefabRef;

    /// <summary>
    /// The <see cref="Transform"/> to spawn the network prefab at.
    /// </summary>
    public Transform? PositionTransform => _positionTransform;

    private void Start()
    {
        if (!SemiFunc.IsMasterClientOrSingleplayer())
            return;

        if (PrefabRef == null)
        {
            Logger.LogError("REPOLibSpawnNetworkPrefab: Failed to spawn network prefab. PrefabRef is null.");
            return;
        }

        if (PositionTransform == null)
        {
            Logger.LogError("REPOLibSpawnNetworkPrefab: Failed to spawn network prefab. PositionTransform is null.");
            return;
        }

        if (PrefabRef.Prefab == null)
        {
            Logger.LogError($"REPOLibSpawnNetworkPrefab: Failed to spawn network prefab. Could not find prefab \"{PrefabRef.PrefabName}\" with prefabId \"{PrefabRef.ResourcePath}\"");
            return;
        }

        NetworkPrefabs.SpawnNetworkPrefab(_prefabRef, PositionTransform.position, PositionTransform.rotation);
    }
}
