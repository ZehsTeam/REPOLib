using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;

namespace REPOLib.Objects.Sdk;

/// <summary>
/// REPOLib LevelContent class.
/// </summary>
[PublicAPI, CreateAssetMenu(menuName = "REPOLib/Level", order = 4, fileName = "New Level")]
public class LevelContent : Content
{
    [FormerlySerializedAs("_level"), SerializeField] 
    private Level level = null!;

    /// <summary>
    /// The <see cref="global::Level"/> of this content.
    /// </summary>
    public Level Level => level;

    /// <summary>
    /// The name of the <see cref="Level"/>
    /// </summary>
    public override string Name => Level?.name ?? string.Empty;

    /// <inheritdoc/>
    public override void Initialize(Mod mod) => Modules.Levels.RegisterLevel(Level);
}
