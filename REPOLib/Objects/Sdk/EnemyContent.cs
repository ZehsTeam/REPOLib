using UnityEngine;

namespace REPOLib.Objects.Sdk;

[CreateAssetMenu(menuName = "REPOLib/Enemy", order = 2, fileName = "New Enemy")]
public class EnemyContent : Content
{
    [SerializeField]
    private EnemySetup _prefab;
    
    public EnemySetup Prefab => _prefab;

    public override string Name => Prefab.name;

    public override void Initialize(Mod mod)
    {
        Modules.Enemies.RegisterEnemy(Prefab);
    }
}
