using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace REPOLib.Objects.Sdk;

[CreateAssetMenu(menuName = "REPOLib/Valuable", order = 1, fileName = "New Valuable")]
public class ValuableContent : Content
{
    [SerializeField]
    private ValuableObject _prefab;

    [SerializeField]
    private bool _addToAllLevels = true;
    
    [SerializeField]
    private List<string> _levelNames;
    
    public ValuableObject Prefab => _prefab;
    public bool AddToAllLevels => _addToAllLevels;
    public IReadOnlyList<string> LevelNames => _levelNames;

    public override string Name => Prefab.name;

    public override void Initialize(Mod mod)
    {
        List<string> presets = AddToAllLevels ? null : _levelNames;
        Modules.Valuables.RegisterValuable($"{mod.FullName}:{Name}", Prefab.gameObject, presets);
    }
}
