using REPOLib.Extensions;
using REPOLib.Objects;
using REPOLib.Objects.Sdk;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;

namespace REPOLib.Modules;

/// <summary>
/// The Levels module of REPOLib.
/// </summary>
public static class Levels
{
    private enum ModuleType
    {
        StartRoom,
        Normal,
        Passage,
        DeadEnd,
        Extraction
    }

    /// <summary>
    /// Get a <see cref="Level"/> list from <see cref="RunManager"/>.
    /// </summary>
    /// <returns>A <see cref="Level"/> list from <see cref="RunManager"/>.</returns>
    public static IReadOnlyList<Level> AllLevels => RunManager.instance?.levels ?? [];

    /// <summary>
    /// Get all levels registered with REPOLib.
    /// </summary>
    public static IReadOnlyList<Level> RegisteredLevels => _levelsRegistered;

    private static readonly List<Level> _levelsToRegister = [];
    private static readonly List<Level> _levelsRegistered = [];

    private static bool _initialLevelsRegistered;

    internal static void RegisterInitialLevels()
    {
        if (_initialLevelsRegistered)
        {
            return;
        }

        ValuablePresets.Initialize();

        Logger.LogInfo($"Adding levels.");

        foreach (var level in _levelsToRegister)
        {
            RegisterLevelWithGame(level);
        }

        _initialLevelsRegistered = true;
    }

    static void RegisterLevelWithGame(Level level)
    {
        if (_levelsRegistered.Contains(level))
        {
            return;
        }

        if (level.ValuablePresets.Count == 0)
        {
            Logger.LogWarning($"Level \"{level.name}\" does not have any valuable presets! Adding generic preset.");
            level.ValuablePresets.Add(ValuablePresets.GenericValuablePreset);
        }

        for (int i = 0; i < level.ValuablePresets.Count; i++)
        {
            var valuablePreset = level.ValuablePresets[i];

            if (ValuablePresets.AllValuablePresets.Values.Contains(valuablePreset))
            {
                continue;
            }

            // This allows custom levels to use vanilla presets, by using a proxy preset with the same name
            if (ValuablePresets.AllValuablePresets.TryGetValue(valuablePreset.name, out var foundPreset))
            {
                // Check if the mod author accidentally included a vanilla preset (and all of its valuables) in their bundle
                if (valuablePreset.GetCombinedList().Count > 0)
                {
                    Logger.LogWarning($"Proxy preset \"{valuablePreset.name}\" in level \"{level.name}\" contains valuables! This likely caused duplicate valuables to load!");
                }

                level.ValuablePresets[i] = foundPreset;
                Logger.LogInfo($"Replaced proxy preset \"{valuablePreset.name}\" in level \"{level.name}\".", extended: true);
            }
            else
            {
                ValuablePresets.RegisterValuablePreset(valuablePreset);
                Logger.LogInfo($"Registered valuable preset \"{valuablePreset.name}\" from \"{level.name}\".", extended: true);
            }
        }

        RunManager.instance.levels.Add(level);
        Logger.LogInfo($"Added level \"{level.name}\"", extended: true);

        _levelsRegistered.Add(level);
    }

    /// <summary>
    /// Registers a <see cref="Level"/>.
    /// </summary>
    /// <param name="levelContent">The <see cref="LevelContent"/> to register.</param>
    public static void RegisterLevel(LevelContent? levelContent)
    {
        if (levelContent == null)
        {
            Logger.LogError($"Failed to register level. LevelContent is null.");
            return;
        }

        Level? level = levelContent.Level;

        if (level == null)
        {
            Logger.LogError($"Failed to register level. Level is null.");
            return;
        }

        if (_levelsToRegister.Contains(level))
        {
            Logger.LogWarning($"Failed to register level \"{level.name}\". Level is already registered!");
            return;
        }

        if (_levelsToRegister.Any(x => x.name.Equals(level.name, StringComparison.OrdinalIgnoreCase)))
        {
            Logger.LogError($"Failed to register level \"{level.name}\". Level already exists with the same name.");
            return;
        }

        GameObject? connectObjectPrefab = levelContent.ConnectObject;

        if (connectObjectPrefab != null)
        {
            RegisterLevelPrefab($"Level/{level.ResourcePath}/Other/{connectObjectPrefab.name}", connectObjectPrefab);
            level.ConnectObject = connectObjectPrefab;
        }

        GameObject? blockObjectPrefab = levelContent.BlockObject;

        if (blockObjectPrefab != null)
        {
            RegisterLevelPrefab($"Level/{level.ResourcePath}/Other/{blockObjectPrefab.name}", blockObjectPrefab);
            level.BlockObject = blockObjectPrefab;
        }

        level.StartRooms = RegisterLevelModules(level, ModuleType.StartRoom, levelContent.StartRooms);

        level.ModulesNormal1 = RegisterLevelModules(level, ModuleType.Normal, levelContent.ModulesNormal1);
        level.ModulesPassage1 = RegisterLevelModules(level, ModuleType.Passage, levelContent.ModulesPassage1);
        level.ModulesDeadEnd1 = RegisterLevelModules(level, ModuleType.DeadEnd, levelContent.ModulesDeadEnd1);
        level.ModulesExtraction1 = RegisterLevelModules(level, ModuleType.Extraction, levelContent.ModulesExtraction1);

        level.ModulesNormal2 = RegisterLevelModules(level, ModuleType.Normal, levelContent.ModulesNormal2);
        level.ModulesPassage2 = RegisterLevelModules(level, ModuleType.Passage, levelContent.ModulesPassage2);
        level.ModulesDeadEnd2 = RegisterLevelModules(level, ModuleType.DeadEnd, levelContent.ModulesDeadEnd2);
        level.ModulesExtraction2 = RegisterLevelModules(level, ModuleType.Extraction, levelContent.ModulesExtraction2);

        level.ModulesNormal3 = RegisterLevelModules(level, ModuleType.Normal, levelContent.ModulesNormal3);
        level.ModulesPassage3 = RegisterLevelModules(level, ModuleType.Passage, levelContent.ModulesPassage3);
        level.ModulesDeadEnd3 = RegisterLevelModules(level, ModuleType.DeadEnd, levelContent.ModulesDeadEnd3);
        level.ModulesExtraction3 = RegisterLevelModules(level, ModuleType.Extraction, levelContent.ModulesExtraction3);

        // TODO: Register valuable presets.

        _levelsToRegister.Add(level);

        if (_initialLevelsRegistered)
        {
            RegisterLevelWithGame(level);
        }
    }

    private static List<PrefabRef> RegisterLevelModules(Level level, ModuleType moduleType, List<GameObject> modules)
    {
        List<PrefabRef> prefabRefs = [];

        foreach (var module in modules)
        {
            string prefabId = $"Level/{level.name}/{moduleType}/{module.name}";

            PrefabRef? prefabRef = RegisterLevelPrefab(prefabId, module);
            if (prefabRef == null) continue;

            prefabRefs.Add(prefabRef);
        }

        return prefabRefs;
    }

    private static PrefabRef? RegisterLevelPrefab(string prefabId, GameObject prefab)
    {
        PrefabRefResponse prefabRefResponse = NetworkPrefabs.RegisterNetworkPrefabInternal(prefabId, prefab);

        if (prefabRefResponse.Result == PrefabRefResult.Success)
        {
            Utilities.FixAudioMixerGroups(prefab);
            return prefabRefResponse.PrefabRef;
        }

        if (prefabRefResponse.Result == PrefabRefResult.PrefabAlreadyRegistered)
        {
            return prefabRefResponse.PrefabRef;
        }

        if (prefabRefResponse.Result == PrefabRefResult.DifferentPrefabAlreadyRegistered)
        {
            Logger.LogError($"Failed to register level network prefab \"{prefabId}\". A prefab is already registered with the same prefab ID.");
            return null;
        }

        Logger.LogError($"Failed to register level network prefab \"{prefabId}\". (Reason: {prefabRefResponse.Result})");
        return null;
    }

    #region Deprecated
    #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    [Obsolete("This is no longer supported. Use AllLevels or RegisteredLevels instead.", error: true)]
    public static IReadOnlyList<Level> GetLevels()
    {
        return AllLevels;
    }

    [Obsolete("This is no longer supported. Use AllLevels or RegisteredLevels instead.", error: true)]
    public static bool TryGetLevelByName(string name, [NotNullWhen(true)] out Level? level)
    {
        level = null;
        return false;
    }

    [Obsolete("This is no longer supported. Use AllLevels or RegisteredLevels instead.", error: true)]
    public static Level? GetLevelByName(string name)
    {
        return null;
    }

    [Obsolete("This is no longer supported. Use AllLevels or RegisteredLevels instead.", error: true)]
    public static bool TryGetLevelThatContainsName(string name, [NotNullWhen(true)] out Level? level)
    {
        level = null;
        return false;
    }

    [Obsolete("This is no longer supported. Use AllLevels or RegisteredLevels instead.", error: true)]
    public static Level? GetLevelThatContainsName(string name)
    {
        return null;
    }
    #pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    #endregion
}
