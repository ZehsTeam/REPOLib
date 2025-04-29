using REPOLib.Modules;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;

namespace REPOLib.Objects.Sdk;

/// <summary>
/// REPOLib ValuableContent class.
/// </summary>
[PublicAPI, CreateAssetMenu(menuName = "REPOLib/Valuable", order = 1, fileName = "New Valuable")]
public class ValuableContent : Content
{
    [FormerlySerializedAs("_prefab"), SerializeField] 
    private ValuableObject prefab = null!;

    [FormerlySerializedAs("_valuablePresets"), SerializeField] 
    private string[] valuablePresets = [Modules.ValuablePresets.GenericValuablePresetName];

    /// <summary>
    /// The <see cref="ValuableObject"/> of this content.
    /// </summary>
    public ValuableObject Prefab => prefab;

    /// <summary>
    /// The list of valuable presets for this content.
    /// </summary>
    public IReadOnlyList<string> ValuablePresets => valuablePresets;

    /// <summary>
    /// The name of the <see cref="Prefab"/>.
    /// </summary>
    public override string Name => Prefab?.name ?? string.Empty;

    /// <inheritdoc/>
    public override void Initialize(Mod mod) 
        => Valuables.RegisterValuable(Prefab.gameObject, ValuablePresets.ToList());
}
