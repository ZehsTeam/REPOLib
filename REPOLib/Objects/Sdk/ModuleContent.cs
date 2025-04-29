using System.Collections.Generic;
using UnityEngine;

namespace REPOLib.Objects.Sdk;

/// <summary>
/// REPOLib ModuleContent class.
/// </summary>
[CreateAssetMenu(menuName = "REPOLib/Module", order = 4, fileName = "New Module")]
public class ModuleContent : Content
{
    #pragma warning disable CS0649 // Field 'field' is never assigned to, and will always have its default value 'value'
    [SerializeField]
    private Module _prefab = null!;
    #pragma warning restore CS0649 // Field 'field' is never assigned to, and will always have its default value 'value'

    [SerializeField] 
    private Modules.Modules.Type _type = Modules.Modules.Type.Normal;
    
    [SerializeField]
    private Modules.Modules.Difficulty _difficulties = Modules.Modules.Difficulty.None;

    [SerializeField] 
    private List<string> _targetLevels = [];
    
    /// <summary>
    /// The <see cref="global::Module"/> of this content.
    /// </summary>
    public Module Prefab => _prefab;

    /// <summary>
    /// The name of the <see cref="Module"/>
    /// </summary>
    public override string Name => Prefab?.name ?? string.Empty;

    /// <summary>
    /// The type of the <see cref="Module"/>
    /// </summary>
    public Modules.Modules.Type Type => _type;
    
    /// <summary>
    /// The <see cref="Modules.Modules.Difficulty"/> at which this module will be generated at.
    /// </summary>
    public Modules.Modules.Difficulty Difficulty => _difficulties;
    
    /// <summary>
    /// The names of the Levels that this module should be added to
    /// </summary>
    public List<string> TargetLevels => _targetLevels;
    
    /// <inheritdoc/>
    public override void Initialize(Mod mod)
    {
        Modules.Modules.RegisterModule(new REPOLib.Modules.Modules.ModuleRegistrationInfo(Prefab, Type, Difficulty, TargetLevels));
    }
}
