using UnityEngine;

namespace REPOLib.Objects.Sdk;

[CreateAssetMenu(menuName = "REPOLib/Item", order = 2, fileName = "New Item")]
public class ItemContent : Content
{
    #pragma warning disable CS0649 // Field 'field' is never assigned to, and will always have its default value 'value'
    [SerializeField]
    private ItemAttributes _prefab = null!;
    #pragma warning restore CS0649 // Field 'field' is never assigned to, and will always have its default value 'value'

    public ItemAttributes Prefab => _prefab;

    public override string Name => Prefab.name;

    public override void Initialize(Mod mod)
    {
        // Set prefab on item
        _prefab.item.prefab = _prefab.gameObject;

        Modules.Items.RegisterItem(Prefab.item);
    }
}
