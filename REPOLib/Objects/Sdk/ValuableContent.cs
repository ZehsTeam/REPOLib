using UnityEngine;

namespace REPOLib.Objects.Sdk;

[CreateAssetMenu(menuName = "REPOLib/Valuable", order = 1, fileName = "New Valuable")]
public class ValuableContent : Content
{
    [SerializeField]
    private ValuableObject _prefab;
    
    public ValuableObject Prefab => _prefab;

    public override string Name => Prefab.name;

    public override void Initialize(Mod mod)
    {
        Modules.Valuables.RegisterValuable($"{mod.FullName}:{Name}", Prefab.gameObject);
    }
}
