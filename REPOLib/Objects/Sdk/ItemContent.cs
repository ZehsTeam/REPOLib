using UnityEngine;

namespace REPOLib.Objects.Sdk;

/// <summary>
/// REPOLib ItemContent class.
/// </summary>
[CreateAssetMenu(menuName = "REPOLib/Item", order = 2, fileName = "New Item")]
public class ItemContent : Content
{
    #pragma warning disable CS0649 // Field 'field' is never assigned to, and will always have its default value 'value'
    [SerializeField]
    private ItemAttributes _prefab = null!;
#pragma warning restore CS0649 // Field 'field' is never assigned to, and will always have its default value 'value'

    /// <summary>
    /// The <see cref="ItemAttributes"/> of this content.
    /// </summary>
    public ItemAttributes Prefab => _prefab;

    /// <summary>
    /// The name of the <see cref="Prefab"/>.
    /// </summary>
    public override string Name => Prefab.name;

    /// <inheritdoc/>
    public override void Initialize(Mod mod)
    {
        // Set prefab on item
        _prefab.item.prefab = _prefab.gameObject;

        Modules.Items.RegisterItem(Prefab.item);
    }
}
