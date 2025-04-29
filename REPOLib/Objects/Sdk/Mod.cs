using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;

namespace REPOLib.Objects.Sdk;

/// <summary>
/// A REPOLib mod.
/// </summary>
[PublicAPI, CreateAssetMenu(menuName = "REPOLib/Mod", order = 0, fileName = "New Mod")]
public class Mod : ScriptableObject
{
    [FormerlySerializedAs("_name"), SerializeField] 
    private string modName = null!;

    [FormerlySerializedAs("_author"), SerializeField] 
    private string author = null!;

    [FormerlySerializedAs("_version"), SerializeField] 
    private string version = "1.0.0";

    [FormerlySerializedAs("_description"), SerializeField] 
    private string description = null!;

    [FormerlySerializedAs("_websiteUrl"), SerializeField] 
    private string websiteUrl = null!;

    [FormerlySerializedAs("_dependencies"), SerializeField] 
    private string[] dependencies = [$"Zehs-REPOLib-{MyPluginInfo.PLUGIN_VERSION}"];

    [FormerlySerializedAs("_icon"), SerializeField] 
    private Sprite icon = null!;

    [FormerlySerializedAs("_readme"), SerializeField] 
    private TextAsset readme = null!;

    /// <summary>
    /// The name of this mod.
    /// </summary>
    public string ModName => modName;

    /// <summary>
    /// The author of this mod.
    /// </summary>
    public string Author => author;

    /// <summary>
    /// The version of this mod.
    /// </summary>
    public string Version => version;

    /// <summary>
    /// The description of this mod.
    /// </summary>
    public string Description => description;

    /// <summary>
    /// The website URL of this mod.
    /// </summary>
    public string WebsiteUrl => websiteUrl;

    /// <summary>
    /// The dependencies of this mod.
    /// </summary>
    public IReadOnlyList<string> Dependencies => dependencies;

    /// <summary>
    /// The icon of this mod.
    /// </summary>
    public Sprite Icon => icon;

    /// <summary>
    /// The readme of this mod.
    /// </summary>
    public TextAsset Readme => readme;

    /// <summary>
    /// The full name of this mod.<br/>
    /// Format is $"{<see cref="Author"/>}-{<see cref="ModName"/>}".
    /// </summary>
    public string FullName => $"{Author}-{ModName}";

    /// <summary>
    /// The Thunderstore identifier of this mod, also known as dependency string.<br/>
    /// Format is $"{<see cref="Author"/>}-{<see cref="ModName"/>}-{<see cref="Version"/>}".
    /// </summary>
    public string Identifier => $"{Author}-{ModName}-{Version}";
}
