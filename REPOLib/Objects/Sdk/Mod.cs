using System.Collections.Generic;
using UnityEngine;

namespace REPOLib.Objects.Sdk;

/// <summary>
/// A REPOLib mod.
/// </summary>
[CreateAssetMenu(menuName = "REPOLib/Mod", order = 0, fileName = "New Mod")]
public class Mod : ScriptableObject
{
    #pragma warning disable CS0649 // Field 'field' is never assigned to, and will always have its default value 'value'
    [SerializeField]
    private string _name = null!;

    [SerializeField]
    private string _author = null!;

    [SerializeField]
    private string _version = "1.0.0";

    [SerializeField]
    private string _description = null!;

    [SerializeField]
    private string _websiteUrl = null!;

    [SerializeField]
    private string[] _dependencies = [$"Zehs-REPOLib-{MyPluginInfo.PLUGIN_VERSION}"];

    [SerializeField]
    private Sprite _icon = null!;

    [SerializeField]
    private TextAsset _readme = null!;
    #pragma warning restore CS0649 // Field 'field' is never assigned to, and will always have its default value 'value'

    /// <summary>
    /// The name of this mod.
    /// </summary>
    public string Name => _name;

    /// <summary>
    /// The author of this mod.
    /// </summary>
    public string Author => _author;

    /// <summary>
    /// The version of this mod.
    /// </summary>
    public string Version => _version;

    /// <summary>
    /// The description of this mod.
    /// </summary>
    public string Description => _description;

    /// <summary>
    /// The website URL of this mod.
    /// </summary>
    public string WebsiteUrl => _websiteUrl;

    /// <summary>
    /// The dependencies of this mod.
    /// </summary>
    public IReadOnlyList<string> Dependencies => _dependencies;

    /// <summary>
    /// The icon of this mod.
    /// </summary>
    public Sprite Icon => _icon;

    /// <summary>
    /// The readme of this mod.
    /// </summary>
    public TextAsset Readme => _readme;

    /// <summary>
    /// The full name of this mod.<br/>
    /// Format is $"{<see cref="Author"/>}-{<see cref="Name"/>}".
    /// </summary>
    public string FullName => $"{Author}-{Name}";

    /// <summary>
    /// The Thunderstore identifier of this mod, also known as dependency string.<br/>
    /// Format is $"{<see cref="Author"/>}-{<see cref="Name"/>}-{<see cref="Version"/>}".
    /// </summary>
    public string Identifier => $"{Author}-{Name}-{Version}";
}
