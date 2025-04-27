using UnityEngine;

namespace REPOLib.Objects.Sdk;

/// <summary>
/// REPOLib LevelContent class.
/// </summary>
[CreateAssetMenu(menuName = "REPOLib/Level", order = 4, fileName = "New Level")]
public class LevelContent : Content
{
    #pragma warning disable CS0649 // Field 'field' is never assigned to, and will always have its default value 'value'
    [SerializeField]
    private Level _level = null!;
    #pragma warning restore CS0649 // Field 'field' is never assigned to, and will always have its default value 'value'

    /// <summary>
    /// The <see cref="global::Level"/> of this content.
    /// </summary>
    public Level Level => _level;

    /// <summary>
    /// The name of the <see cref="Level"/>
    /// </summary>
    public override string Name => Level?.name ?? string.Empty;

    /// <inheritdoc/>
    public override void Initialize(Mod mod)
    {
        Modules.Levels.RegisterLevel(Level);
    }
}
