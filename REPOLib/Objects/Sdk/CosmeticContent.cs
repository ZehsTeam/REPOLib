using REPOLib.Modules;
using UnityEngine;

namespace REPOLib.Objects.Sdk;

/// <summary>
/// REPOLib CosmeticContent class.
/// </summary>
[CreateAssetMenu(menuName = "REPOLib/Cosmetic", order = 5, fileName = "New Cosmetic")]
public class CosmeticContent : Content
{
    #pragma warning disable CS0649 // Field 'field' is never assigned to, and will always have its default value 'value'
    [SerializeField]
    private string? _assetId;

    [SerializeField]
    private CosmeticAsset? _asset;

    [SerializeField]
    private GameObject? _prefab;

    [SerializeField]
    private PrefabRef? _prefabRef;
    #pragma warning restore CS0649 // Field 'field' is never assigned to, and will always have its default value 'value'

    /// <summary>
    /// The <see cref="string"/> of this content.
    /// </summary>
    public string? AssetId => _assetId;

    /// <summary>
    /// The <see cref="CosmeticAsset"/> of this content.
    /// </summary>
    public CosmeticAsset? Asset => _asset;

    /// <summary>
    /// The <see cref="GameObject"/> of this content.
    /// </summary>
    public GameObject? Prefab => _prefab;

    /// <summary>
    /// The <see cref="PrefabRef"/> of this content.
    /// </summary>
    public PrefabRef? PrefabRef => _prefabRef;

    /// <summary>
    /// The name of the <see cref="Prefab"/>.
    /// </summary>
    public override string Name => PrefabRef?.PrefabName ?? Prefab?.name ?? string.Empty;

    /// <inheritdoc/>
    public override AssetBundle? Bundle { get; set; }

    /// <inheritdoc/>
    public override void Initialize(Mod mod)
    {
        Cosmetics.RegisterCosmetic(this);
    }
}
