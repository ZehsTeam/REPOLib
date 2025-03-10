using UnityEngine;

namespace REPOLib.Objects.Sdk;

[CreateAssetMenu(menuName = "REPOLib/Enemy", order = 3, fileName = "New Enemy")]
public class EnemyContent : Content
{
    [SerializeField]
    private EnemySetup _setup;
    
    public EnemySetup Setup => _setup;

    public override string Name => Setup.name;

    public override void Initialize(Mod mod)
    {
        Modules.Enemies.RegisterEnemy(Setup);
    }
}
