using JetBrains.Annotations;
using REPOLib.Modules;
using UnityEngine;

namespace REPOLib.Objects.Sdk;

/// <summary>
///     REPOLib EnemyContent class.
/// </summary>
[PublicAPI]
[CreateAssetMenu(menuName = "REPOLib/Enemy", order = 3, fileName = "New Enemy")]
public class EnemyContent : Content
{
    [SerializeField]
    private EnemySetup _setup = null!;

    /// <summary>
    ///     The <see cref="EnemySetup" /> of this content.
    /// </summary>
    public EnemySetup Setup
        => this._setup;

    /// <summary>
    ///     The name of the <see cref="Setup" />.
    /// </summary>
    public override string Name
        => this.Setup?.name ?? string.Empty;

    /// <inheritdoc />
    public override void Initialize(Mod mod)
        => Enemies.RegisterEnemy(this.Setup);
}