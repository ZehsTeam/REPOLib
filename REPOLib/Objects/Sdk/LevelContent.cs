using UnityEngine;

namespace REPOLib.Objects.Sdk;

[CreateAssetMenu(menuName = "REPOLib/Level", order = 4, fileName = "New Level")]
public class LevelContent : Content
{
    #pragma warning disable CS0649 // Field 'field' is never assigned to, and will always have its default value 'value'
    [SerializeField]
    private Level _level;
    #pragma warning restore CS0649 // Field 'field' is never assigned to, and will always have its default value 'value'

    public Level Level => _level;

    public override string Name => Level.name;

    public override void Initialize(Mod mod)
    {
        Modules.Levels.RegisterLevel(Level);
    }
}
