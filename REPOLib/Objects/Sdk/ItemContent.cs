using REPOLib.Modules;
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
    private ItemAttributes? _prefab;
    #pragma warning restore CS0649 // Field 'field' is never assigned to, and will always have its default value 'value'

    /// <summary>
    /// The <see cref="ItemAttributes"/> of this content.
    /// </summary>
    public ItemAttributes? Prefab => _prefab;

    /// <summary>
    /// The name of the <see cref="Prefab"/>.
    /// </summary>
    public override string Name => Prefab?.name ?? string.Empty;

    /// <inheritdoc/>
    public override void Initialize(Mod mod)
    {
        Items.RegisterItem(this);
    }
}
