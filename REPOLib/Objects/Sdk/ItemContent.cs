using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;

namespace REPOLib.Objects.Sdk;

/// <summary>
/// REPOLib ItemContent class.
/// </summary>
[PublicAPI, CreateAssetMenu(menuName = "REPOLib/Item", order = 2, fileName = "New Item")]
public class ItemContent : Content
{
    [FormerlySerializedAs("_prefab"), SerializeField] 
    private ItemAttributes prefab = null!;

    /// <summary>
    /// The <see cref="ItemAttributes"/> of this content.
    /// </summary>
    public ItemAttributes Prefab => prefab;

    /// <summary>
    /// The name of the <see cref="Prefab"/>.
    /// </summary>
    public override string Name => Prefab?.name ?? string.Empty;

    /// <inheritdoc/>
    public override void Initialize(Mod mod)
    {
        prefab.item.prefab = prefab.gameObject; // Set prefab on item
        Modules.Items.RegisterItem(Prefab.item);
    }
}
