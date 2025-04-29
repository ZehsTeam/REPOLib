using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;

namespace REPOLib.Objects.Sdk;

/// <summary>
/// REPOLib EnemyContent class.
/// </summary>
[PublicAPI, CreateAssetMenu(menuName = "REPOLib/Enemy", order = 3, fileName = "New Enemy")]
public class EnemyContent : Content
{
    [FormerlySerializedAs("_setup"), SerializeField] 
    private EnemySetup setup = null!;

    /// <summary>
    /// The <see cref="EnemySetup"/> of this content.
    /// </summary>
    public EnemySetup Setup => setup;

    /// <summary>
    /// The name of the <see cref="Setup"/>.
    /// </summary>
    public override string Name => Setup?.name ?? string.Empty;

    /// <inheritdoc/>
    public override void Initialize(Mod mod) => Modules.Enemies.RegisterEnemy(Setup);
}
