using REPOLib.Modules;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace REPOLib.Objects.Sdk;

[CreateAssetMenu(menuName = "REPOLib/Valuable", order = 1, fileName = "New Valuable")]
public class ValuableContent : Content
{
    [SerializeField]
    private ValuableObject _prefab;

    [SerializeField]
    private string[] _valuablePresets = [Modules.ValuablePresets.GenericValuablePresetName];
    
    public ValuableObject Prefab => _prefab;
    public IReadOnlyList<string> ValuablePresets => _valuablePresets;

    public override string Name => Prefab.name;

    public override void Initialize(Mod mod)
    {
        Valuables.RegisterValuable(Prefab.gameObject, ValuablePresets.ToList());
    }
}
