using JetBrains.Annotations;
using REPOLib.Modules;
using UnityEngine;

namespace REPOLib.Objects.Sdk;

/// <summary>
///     REPOLib LevelContent class.
/// </summary>
[PublicAPI]
[CreateAssetMenu(menuName = "REPOLib/Level", order = 4, fileName = "New Level")]
public class LevelContent : Content
{
    [SerializeField]
    private Level _level = null!;

    /// <summary>
    ///     The <see cref="global::Level" /> of this content.
    /// </summary>
    public Level Level
        => this._level;

    /// <summary>
    ///     The name of the <see cref="Level" />
    /// </summary>
    public override string Name
        => this.Level?.name ?? string.Empty;

    /// <inheritdoc />
    public override void Initialize(Mod mod)
        => Levels.RegisterLevel(this.Level);
}