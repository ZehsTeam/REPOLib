using REPOLib.Modules;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace REPOLib.Objects.Sdk;

[CreateAssetMenu(menuName = "REPOLib/Valuable", order = 1, fileName = "New Valuable")]
public class ValuableContent : Content
{
    #pragma warning disable CS0649 // Field 'field' is never assigned to, and will always have its default value 'value'
    [SerializeField]
    private ValuableObject _prefab;
    #pragma warning restore CS0649 // Field 'field' is never assigned to, and will always have its default value 'value'

    [SerializeField]
    private string[] _valuablePresets = [Valuables.GenericValuablePresetName];
    
    public ValuableObject Prefab => _prefab;
    public IReadOnlyList<string> ValuablePresets => _valuablePresets;

    public override string Name => Prefab.name;

    public override void Initialize(Mod mod)
    {
        Valuables.RegisterValuable(Prefab.gameObject, ValuablePresets.ToList());
    }
}
