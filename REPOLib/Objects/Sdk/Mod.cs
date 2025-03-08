using UnityEngine;

namespace REPOLib.Objects.Sdk;

[CreateAssetMenu(menuName = "REPOLib/Mod", order = 0, fileName = "New Mod")]
public class Mod : ScriptableObject
{
    [SerializeField]
    private string _name;

    [SerializeField]
    private string _author;

    [SerializeField]
    private string _version;
    
    [SerializeField]
    private string _description;

    [SerializeField]
    private Sprite _icon;
    
    public string Name => _name;
    public string Author => _author;
    public string Version => _version;
    public string Description => _description;
    public Sprite Icon => _icon;

    public string FullName => $"{Author}-{Name}";
    // also known as a dependency string
    public string Identifier => $"{Author}-{Name}-{Version}";
}
