using System.Collections.Generic;
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
    private string _version = "1.0.0";
    
    [SerializeField]
    private string _description;
    
    [SerializeField]
    private string _websiteUrl;

    [SerializeField]
    private string[] _dependencies = [$"Zehs-REPOLib-{MyPluginInfo.PLUGIN_VERSION}"];

    [SerializeField]
    private Sprite _icon;

    [SerializeField]
    private TextAsset _readme;

    public string Name => _name;
    public string Author => _author;
    public string Version => _version;
    public string Description => _description;
    public string WebsiteUrl => _websiteUrl;
    public IReadOnlyList<string> Dependencies => _dependencies;
    public Sprite Icon => _icon;
    public TextAsset Readme => _readme;

    public string FullName => $"{Author}-{Name}";
    // also known as a dependency string
    public string Identifier => $"{Author}-{Name}-{Version}";
}
