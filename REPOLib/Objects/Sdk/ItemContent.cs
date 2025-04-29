using JetBrains.Annotations;
using REPOLib.Modules;
using UnityEngine;

namespace REPOLib.Objects.Sdk;

/// <summary>
///     REPOLib ItemContent class.
/// </summary>
[PublicAPI]
[CreateAssetMenu(menuName = "REPOLib/Item", order = 2, fileName = "New Item")]
public class ItemContent : Content
{
    [SerializeField]
    private ItemAttributes _prefab = null!;

    /// <summary>
    ///     The <see cref="ItemAttributes" /> of this content.
    /// </summary>
    public ItemAttributes Prefab
        => this._prefab;

    /// <summary>
    ///     The name of the <see cref="Prefab" />.
    /// </summary>
    public override string Name
        => this.Prefab?.name ?? string.Empty;

    /// <inheritdoc />
    public override void Initialize(Mod mod)
    {
        this._prefab.item.prefab = this._prefab.gameObject; // Set prefab on item
        Items.RegisterItem(this.Prefab.item);
    }
}