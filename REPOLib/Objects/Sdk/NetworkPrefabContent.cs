using REPOLib.Modules;
using UnityEngine;

namespace REPOLib.Objects.Sdk;

/// <summary>
/// REPOLib NetworkPrefabContent class.
/// </summary>
[CreateAssetMenu(menuName = "REPOLib/Network Prefab", order = 5, fileName = "New Network Prefab")]
public class NetworkPrefabContent : Content
{
    #pragma warning disable CS0649 // Field 'field' is never assigned to, and will always have its default value 'value'
    [SerializeField]
    private GameObject? _prefab;
    #pragma warning restore CS0649 // Field 'field' is never assigned to, and will always have its default value 'value'

    /// <summary>
    /// The network prefab of this content.
    /// </summary>
    public GameObject? Prefab => _prefab;

    /// <summary>
    /// The name of the <see cref="Prefab"/>.
    /// </summary>
    public override string Name => Prefab?.name ?? string.Empty;

    /// <inheritdoc/>
    public override void Initialize(Mod mod)
    {
        NetworkPrefabs.RegisterNetworkPrefab(this);
    }
}
