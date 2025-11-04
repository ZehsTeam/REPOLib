using REPOLib.Modules;
using System.Collections.Generic;
using UnityEngine;

namespace REPOLib.Objects.Sdk;

/// <summary>
/// REPOLib LevelContent class.
/// </summary>
[CreateAssetMenu(menuName = "REPOLib/Level", order = 4, fileName = "New Level")]
public class LevelContent : Content
{
    #pragma warning disable CS0649 // Field 'field' is never assigned to, and will always have its default value 'value'
    [SerializeField]
    private Level? _level;

    [SerializeField]
    private GameObject? _connectObject;

    [SerializeField]
    private GameObject? _blockObject;

    [SerializeField]
    private List<GameObject> _startRooms = [];

    [Space]
    [Header("Difficulty 1")]
    [SerializeField]
    private List<GameObject> _modulesNormal1 = [];

    [SerializeField]
    private List<GameObject> _modulesPassage1 = [];

    [SerializeField]
    private List<GameObject> _modulesDeadEnd1 = [];

    [SerializeField]
    private List<GameObject> _modulesExtraction1 = [];

    [Space]
    [Header("Difficulty 2")]
    [SerializeField]
    private List<GameObject> _modulesNormal2 = [];

    [SerializeField]
    private List<GameObject> _modulesPassage2 = [];

    [SerializeField]
    private List<GameObject> _modulesDeadEnd2 = [];

    [SerializeField]
    private List<GameObject> _modulesExtraction2 = [];

    [Space]
    [Header("Difficulty 3")]
    [SerializeField]
    private List<GameObject> _modulesNormal3 = [];

    [SerializeField]
    private List<GameObject> _modulesPassage3 = [];

    [SerializeField]
    private List<GameObject> _modulesDeadEnd3 = [];

    [SerializeField]
    private List<GameObject> _modulesExtraction3 = [];
    #pragma warning restore CS0649 // Field 'field' is never assigned to, and will always have its default value 'value'

    /// <summary>
    /// The <see cref="global::Level"/> of this content.
    /// </summary>
    public Level? Level => _level;

    /// <summary>
    /// The name of the <see cref="Level"/>
    /// </summary>
    public override string Name => Level?.name ?? string.Empty;

    // TODO: Document this.
    #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public GameObject? ConnectObject => _connectObject;

    public GameObject? BlockObject => _blockObject;

    public List<GameObject> StartRooms => _startRooms;

    public List<GameObject> ModulesNormal1 => _modulesNormal1;
    public List<GameObject> ModulesPassage1 => _modulesPassage1;
    public List<GameObject> ModulesDeadEnd1 => _modulesDeadEnd1;
    public List<GameObject> ModulesExtraction1 => _modulesExtraction1;

    public List<GameObject> ModulesNormal2 => _modulesNormal2;
    public List<GameObject> ModulesPassage2 => _modulesPassage2;
    public List<GameObject> ModulesDeadEnd2 => _modulesDeadEnd2;
    public List<GameObject> ModulesExtraction2 => _modulesExtraction2;

    public List<GameObject> ModulesNormal3 => _modulesNormal3;
    public List<GameObject> ModulesPassage3 => _modulesPassage3;
    public List<GameObject> ModulesDeadEnd3 => _modulesDeadEnd3;
    public List<GameObject> ModulesExtraction3 => _modulesExtraction3;
    #pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

    /// <inheritdoc/>
    public override void Initialize(Mod mod)
    {
        Levels.RegisterLevel(this);
    }
}
