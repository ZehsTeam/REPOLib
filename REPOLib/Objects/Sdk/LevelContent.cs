using UnityEngine;

namespace REPOLib.Objects.Sdk;

[CreateAssetMenu(menuName = "REPOLib/Level", order = 4, fileName = "New Level")]
public class LevelContent : Content
{
    [SerializeField]
    private Level _level;
    
    public Level Level => _level;

    public override string Name => Level.name;

    public override void Initialize(Mod mod)
    {
        Modules.Levels.RegisterLevel(Level);
    }
}
