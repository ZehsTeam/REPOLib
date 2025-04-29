using System.Collections.Generic;
using UnityEngine;

namespace REPOLib.Modules;

/// <summary>
/// The Modules module of REPOLib.
/// </summary>
public static class Modules
{
    /// <summary>
    /// The three different difficulty levels modules spawn at. 
    /// </summary>
    [System.Flags]
    public enum Difficulty
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        One = 1,
        Two = 2,
        Three = 4,
        All = One | Two | Three,
        None = 0,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
    
    /// <summary>
    /// Defines the type of a module.
    /// </summary>
    public enum Type
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        Normal,
        Passage,
        DeadEnd,
        Extraction,
        StartRoom,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }

    /// <summary>
    /// A struct containing the information necessary to add modules into existing levels.
    /// </summary>
    public struct ModuleRegistrationInfo
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public Module module;
        public Type type;
        public Difficulty difficulty;
        public List<string> targetLevels;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        /// <summary>
        /// Constructor for a <see cref="ModuleRegistrationInfo"/> struct.
        /// </summary>
        /// <param name="module">The <see cref="Module"/> to register.</param>
        /// <param name="type">The <see cref="Type"/> of the <see cref="Module"/></param>
        /// <param name="difficulty">The <see cref="Difficulty"/> or difficulties the <see cref="Module"/> should be registered to.</param>
        /// <param name="targetLevels">The ResourcePath of the level this module should be registered to.</param>
        public ModuleRegistrationInfo(Module module, Type type, Difficulty difficulty, List<string> targetLevels)
        {
            this.module = module;
            this.type = type;
            this.difficulty = difficulty;
            this.targetLevels = targetLevels;
        }
    }
    
    /// <summary>
    /// Get all levels registered with REPOLib.
    /// </summary>
    public static IReadOnlyList<Module> RegisteredModules => _modulesRegistered;

    private static readonly List<ModuleRegistrationInfo> _modulesToRegister = [];
    private static readonly List<Module> _modulesRegistered = [];

    private static bool _initialModulesRegistered;

    internal static void RegisterInitialModules()
    {
        if (_initialModulesRegistered)
        {
            return;
        }

        Logger.LogInfo($"Adding modules.");

        foreach (var module in _modulesToRegister)
        {
            RegisterModuleWithGame(module);
        }

        _modulesToRegister.Clear();
        _initialModulesRegistered = true;
    }
    
    static void RegisterModuleWithGame(ModuleRegistrationInfo info)
    {
        foreach (var targetLevelName in info.targetLevels)
        {
            Level? targetLevel = Levels.GetLevelByName(targetLevelName);
            
            if (!targetLevel)
            {
                Logger.LogError($"Failed to register module \"{info.module.name}\" to level \"{targetLevelName}\". Could not find a level with a matching name.");
                continue;
            }
            
            RegisterModuleToLevel(info.module, info.type, info.difficulty, targetLevel!);
        }
    }
    
    /// <summary>
    /// Registers a <see cref="Module"/>.
    /// </summary>
    /// <param name="info">The <see cref="ModuleRegistrationInfo"/> to register.</param>
    public static void RegisterModule(ModuleRegistrationInfo info)
    {
        if (!info.module)
        {
            Logger.LogError($"Failed to register module. Module is null.");
            return;
        }

        if (info.difficulty.HasFlag(Difficulty.None) && info.type != Type.StartRoom)
        {
            Logger.LogError($"Failed to register module \"{info.module.name}\". Module has no assigned difficulty, and is not a Start Room.");
            return;
        }

        if (info.targetLevels == null || info.targetLevels.Count == 0)
        {
            Logger.LogError($"Failed to register module \"{info.module.name}\". Module has no levels to be assigned to.");
        }

        string prefabId = ResourcesHelper.GetModulePrefabPath(info.module.gameObject);
        
        if (!_modulesRegistered.Contains(info.module) && !ResourcesHelper.HasPrefab(prefabId))
        {
            NetworkPrefabs.RegisterNetworkPrefab(prefabId, info.module.gameObject);
            Utilities.FixAudioMixerGroups(info.module.gameObject);
        }

        if (_initialModulesRegistered)
        {
            RegisterModuleWithGame(info);
        }
        else
        {
            _modulesToRegister.Add(info);
        }
    }
    
    private static void RegisterModuleToLevel(Module module, Type type, Difficulty difficulty, Level level)
    {
        switch (type)
        {
            case Type.Normal: 
                RegisterModuleToLevelUsingCategory(module, type, difficulty, level, 
                    ref level.ModulesNormal1, ref level.ModulesNormal2, ref level.ModulesNormal3); 
                break;
            case Type.Passage:
                RegisterModuleToLevelUsingCategory(module, type, difficulty, level, 
                    ref level.ModulesPassage1, ref level.ModulesPassage2, ref level.ModulesPassage3); 
                break;
            case Type.DeadEnd:
                RegisterModuleToLevelUsingCategory(module, type, difficulty, level, 
                    ref level.ModulesDeadEnd1, ref level.ModulesDeadEnd2, ref level.ModulesDeadEnd3); 
                break;
            case Type.Extraction:
                RegisterModuleToLevelUsingCategory(module, type, difficulty, level, 
                    ref level.ModulesExtraction1, ref level.ModulesExtraction2, ref level.ModulesExtraction3); 
                break;
            case Type.StartRoom:
                if (!level.StartRooms.Contains(module.gameObject))
                {
                    level.StartRooms.Add(module.gameObject);
                    Logger.LogInfo($"Added module \"{module.name}\" to \"{level.name}\" in \"{type.ToString()}\" category.", extended: true);
                } 
                else
                {
                    Logger.LogWarning($"Module \"{module.name}\" is already registered to \"{level.name}\" in \"{type.ToString()}\" category.");
                }
                break;
        }
    }

    private static void RegisterModuleToLevelUsingCategory(Module module, Type type, Difficulty difficulty, Level level,
        ref List<GameObject> dif1, ref List<GameObject> dif2, ref List<GameObject> dif3)
    {
        if (difficulty.HasFlag(Difficulty.One))
            RegisterModuleToList(module, type, 1, level, ref dif1);
        if (difficulty.HasFlag(Difficulty.Two))
            RegisterModuleToList(module, type, 2, level, ref dif2);
        if (difficulty.HasFlag(Difficulty.Three))
            RegisterModuleToList(module, type, 3, level, ref dif3);
    }

    private static void RegisterModuleToList(Module module, Type type, int difficulty, Level level,
        ref List<GameObject> list)
    {
        if (!list.Contains(module.gameObject))
        {
            list.Add(module.gameObject);
            Logger.LogInfo($"Added module \"{module.name}\" to \"{level.name}\" difficulty {difficulty} in \"{type.ToString()}\" category.", extended: true);
        }
        else
        {
            Logger.LogWarning($"Module \"{module.name}\" is already registered to \"{level.name}\" difficulty {difficulty} in \"{type.ToString()}\" category.");
        }
    }
}
