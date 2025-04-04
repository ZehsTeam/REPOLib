using UnityEngine;

namespace REPOLib.Objects.Sdk;

/// <summary>
/// REPOLib EnemyContent class.
/// </summary>
[CreateAssetMenu(menuName = "REPOLib/Enemy", order = 3, fileName = "New Enemy")]
public class EnemyContent : Content
{
#pragma warning disable CS0649 // Field 'field' is never assigned to, and will always have its default value 'value'
    [SerializeField]
    private EnemySetup _setup = null!;
#pragma warning restore CS0649 // Field 'field' is never assigned to, and will always have its default value 'value'

    /// <summary>
    /// The <see cref="EnemySetup"/> of this content.
    /// </summary>
    public EnemySetup Setup => _setup;

    /// <summary>
    /// The name of the <see cref="Setup"/>.
    /// </summary>
    public override string Name => Setup.name;

    /// <inheritdoc/>
    public override void Initialize(Mod mod)
    {
        Modules.Enemies.RegisterEnemy(Setup);
    }
}
