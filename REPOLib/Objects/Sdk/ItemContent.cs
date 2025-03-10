using UnityEngine;

namespace REPOLib.Objects.Sdk;

[CreateAssetMenu(menuName = "REPOLib/Item", order = 2, fileName = "New Item")]
public class ItemContent : Content
{
    [SerializeField]
    private ItemAttributes _prefab;

    public ItemAttributes Prefab => _prefab;

    public override string Name => Prefab.name;

    public override void Initialize(Mod mod)
    {
        // Set prefab on item
        _prefab.item.prefab = _prefab.gameObject;

        Modules.Items.RegisterItem(Prefab.item);
    }
}
