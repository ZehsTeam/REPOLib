using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using REPOLib.Modules;
using UnityEngine;

namespace REPOLib.Objects.Sdk;

/// <summary>
///     REPOLib ValuableContent class.
/// </summary>
[PublicAPI]
[CreateAssetMenu(menuName = "REPOLib/Valuable", order = 1, fileName = "New Valuable")]
public class ValuableContent : Content
{
    [SerializeField]
    private ValuableObject _prefab = null!;

    [SerializeField]
    private string[] _valuablePresets = 
    [ 
        Modules.ValuablePresets.GenericValuablePresetName 
    ];

    /// <summary>
    ///     The <see cref="ValuableObject" /> of this content.
    /// </summary>
    public ValuableObject Prefab
        => this._prefab;

    /// <summary>
    ///     The list of valuable presets for this content.
    /// </summary>
    public IReadOnlyList<string> ValuablePresets
        => this._valuablePresets;

    /// <summary>
    ///     The name of the <see cref="Prefab" />.
    /// </summary>
    public override string Name
        => this.Prefab?.name ?? string.Empty;

    /// <inheritdoc />
    public override void Initialize(Mod mod)
        => Valuables.RegisterValuable(this.Prefab.gameObject, this.ValuablePresets.ToList());
}