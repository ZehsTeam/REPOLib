using REPOLib.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace REPOLib.Modules;

/// <summary>
/// The Levels module of REPOLib.
/// </summary>
[PublicAPI]
public static class Levels
{
    /// <inheritdoc cref="GetLevels"/>
    public static IReadOnlyList<Level> AllLevels => GetLevels();

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
            return;
        
        ValuablePresets.CacheValuablePresets();
        Logger.LogInfo($"Adding levels.");

        foreach (var level in _levelsToRegister)
            RegisterLevelWithGame(level);

        _levelsToRegister.Clear();
        _initialLevelsRegistered = true;
    }

    private static void RegisterLevelWithGame(Level level)
    {
        if (_levelsRegistered.Contains(level))
            return;

        if (level.ValuablePresets.Count == 0)
        {
            Logger.LogWarning($"Level \"{level.name}\" does not have any valuable presets! Adding generic preset.");
            level.ValuablePresets.Add(ValuablePresets.GenericPreset);
        }

        for (var i = 0; i < level.ValuablePresets.Count; i++)
        {
            var valuablePreset = level.ValuablePresets[i];
            if (ValuablePresets.AllValuablePresets.Values.Contains(valuablePreset))
                continue;
            
            // This allows custom levels to use vanilla presets, by using a proxy preset with the same name
            if (ValuablePresets.AllValuablePresets.TryGetValue(valuablePreset.name, out var foundPreset))
            {
                // Check if the mod author accidentally included a vanilla preset (and all of its valuables) in their bundle
                if (valuablePreset.GetCombinedList().Count > 0)
                    Logger.LogWarning($"Proxy preset \"{valuablePreset.name}\" in level \"{level.name}\" contains valuables! This likely caused duplicate valuables to load!");

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
    /// <param name="level">The <see cref="Level"/> to register.</param>
    public static void RegisterLevel(Level level)
    {
        if (level == null)
        {
            Logger.LogError($"Failed to register level. Level is null.");
            return;
        }

        if (_levelsToRegister.Any(x => x.name.Equals(level.name, StringComparison.OrdinalIgnoreCase)))
        {
            Logger.LogError($"Failed to register level \"{level.name}\". Level already exists with the same name.");
            return;
        }

        if (_levelsToRegister.Contains(level))
        {
            Logger.LogWarning($"Failed to register level \"{level.name}\". Level is already registered!");
            return;
        }

        List<(GameObject, ResourcesHelper.LevelPrefabType)> modules =
            new[]
            {
                level.ModulesExtraction1,
                level.ModulesExtraction2,
                level.ModulesExtraction3,
                level.ModulesNormal1,
                level.ModulesNormal2,
                level.ModulesNormal3,
                level.ModulesPassage1,
                level.ModulesPassage2,
                level.ModulesPassage3,
                level.ModulesDeadEnd1,
                level.ModulesDeadEnd2,
                level.ModulesDeadEnd3
            }
                .SelectMany(list => list)
                .Select(prefab => (prefab, ResourcesHelper.LevelPrefabType.Module))
                .ToList();
        
        modules.AddRange(level.StartRooms.Select(prefab => (prefab, ResourcesHelper.LevelPrefabType.StartRoom)));
        if (level.ConnectObject != null)
            modules.Add((level.ConnectObject, ResourcesHelper.LevelPrefabType.Other));

        if (level.BlockObject != null)
            modules.Add((level.BlockObject, ResourcesHelper.LevelPrefabType.Other));

        foreach (var (prefab, type) in modules)
        {
            var prefabId = ResourcesHelper.GetLevelPrefabPath(level, prefab, type);

            // allow duplicate prefabs
            if (ResourcesHelper.HasPrefab(prefabId))
                continue;

            NetworkPrefabs.RegisterNetworkPrefab(prefabId, prefab);
            Utilities.FixAudioMixerGroups(prefab);
        }

        if (_initialLevelsRegistered)
            RegisterLevelWithGame(level);
        else
            _levelsToRegister.Add(level);
    }

    /// <summary>
    /// Get a <see cref="Level"/> list from <see cref="RunManager"/>.
    /// </summary>
    /// <returns>A <see cref="Level"/> list from <see cref="RunManager"/>.</returns>
    public static IReadOnlyList<Level> GetLevels()
        => RunManager.instance == null ? [] : RunManager.instance.levels;

    /// <summary>
    /// Tries to get a <see cref="Level"/> by name.
    /// </summary>
    /// <param name="name">The <see cref="string"/> to match.</param>
    /// <param name="level">The found <see cref="Level"/>.</param>
    /// <returns>Whether the <see cref="Level"/> was found.</returns>
    public static bool TryGetLevelByName(string? name, [NotNullWhen(true)] out Level? level)
        => (level = GetLevelByName(name)) != null;

    /// <summary>
    /// Get a <see cref="Level"/> by name.
    /// </summary>
    /// <param name="name">The <see cref="string"/> to match.</param>
    /// <returns>The <see cref="Level"/> or null.</returns>
    public static Level? GetLevelByName(string? name)
        => GetLevels().FirstOrDefault(level => level.name.EqualsAny([name, $"Level - {name}"], StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Tries to get a <see cref="Level"/> that contains the specified name.
    /// </summary>
    /// <param name="name">The <see cref="string"/> to compare.</param>
    /// <param name="level">The found <see cref="Level"/>.</param>
    /// <returns>Whether the <see cref="Level"/> was found.</returns>
    public static bool TryGetLevelThatContainsName(string name, [NotNullWhen(true)] out Level? level)
        => (level = GetLevelThatContainsName(name)) != null;

    /// <summary>
    /// Gets a <see cref="Level"/> that contains the specified name.
    /// </summary>
    /// <param name="name">The <see cref="string"/> to compare.</param>
    /// <returns>The <see cref="Level"/> or null.</returns>
    public static Level? GetLevelThatContainsName(string name)
        => GetLevels().FirstOrDefault(level => level.name.Contains(name, StringComparison.OrdinalIgnoreCase));
}
