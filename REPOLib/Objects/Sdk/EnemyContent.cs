using UnityEngine;

namespace REPOLib.Objects.Sdk;

[CreateAssetMenu(menuName = "REPOLib/Enemy", order = 3, fileName = "New Enemy")]
public class EnemyContent : Content
{
    #pragma warning disable CS0649 // Field 'field' is never assigned to, and will always have its default value 'value'
    [SerializeField]
    private EnemySetup _setup;
    #pragma warning restore CS0649 // Field 'field' is never assigned to, and will always have its default value 'value'

    public EnemySetup Setup => _setup;

    public override string Name => Setup.name;

    public override void Initialize(Mod mod)
    {
        Modules.Enemies.RegisterEnemy(Setup);
    }
}
