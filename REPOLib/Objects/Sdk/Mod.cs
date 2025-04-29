using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace REPOLib.Objects.Sdk;

/// <summary>
///     A REPOLib mod.
/// </summary>
[PublicAPI]
[CreateAssetMenu(menuName = "REPOLib/Mod", order = 0, fileName = "New Mod")]
public class Mod : ScriptableObject
{
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
    private string[] _dependencies = [ $"Zehs-REPOLib-{MyPluginInfo.PLUGIN_VERSION}" ];

    [SerializeField]
    private Sprite _icon = null!;

    [SerializeField]
    private TextAsset _readme = null!;

    /// <summary>
    ///     The name of this mod.
    /// </summary>
    public string ModName
        => this._name;

    /// <summary>
    ///     The author of this mod.
    /// </summary>
    public string Author
        => this._author;

    /// <summary>
    ///     The version of this mod.
    /// </summary>
    public string Version
        => this._version;

    /// <summary>
    ///     The description of this mod.
    /// </summary>
    public string Description
        => this._description;

    /// <summary>
    ///     The website URL of this mod.
    /// </summary>
    public string WebsiteUrl
        => this._websiteUrl;

    /// <summary>
    ///     The dependencies of this mod.
    /// </summary>
    public IReadOnlyList<string> Dependencies
        => this._dependencies;

    /// <summary>
    ///     The icon of this mod.
    /// </summary>
    public Sprite Icon
        => this._icon;

    /// <summary>
    ///     The readme of this mod.
    /// </summary>
    public TextAsset Readme
        => this._readme;

    /// <summary>
    ///     The full name of this mod.<br />
    ///     Format is $"{<see cref="Author" />}-{<see cref="ModName" />}".
    /// </summary>
    public string FullName
        => $"{this.Author}-{this.ModName}";

    /// <summary>
    ///     The Thunderstore identifier of this mod, also known as dependency string.<br />
    ///     Format is $"{<see cref="Author" />}-{<see cref="ModName" />}-{<see cref="Version" />}".
    /// </summary>
    public string Identifier
        => $"{this.Author}-{this.ModName}-{this.Version}";
}