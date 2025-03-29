using REPOLib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace REPOLib.Modules;

public static class Levels
{
    public static IReadOnlyList<Level> AllLevels => GetLevels();
    public static IReadOnlyList<Level> RegisteredLevels => _levelsRegistered;
    
    private static readonly List<Level> _levelsToRegister = [];
    private static readonly List<Level> _levelsRegistered = [];

    private static bool _canRegisterLevels = true;

    internal static void RegisterLevels()
    {
        if (!_canRegisterLevels)
        {
            return;
        }
        
        ValuablePresets.CacheValuablePresets();

        Logger.LogInfo($"Adding levels.");
        
        foreach (var level in _levelsToRegister)
        {
            if (_levelsRegistered.Contains(level))
            {
                continue;
            }

            if (level.ValuablePresets.Count == 0)
            {
                Logger.LogWarning($"Level \"{level.name}\" does not have any valuable presets! Adding generic preset.");
                level.ValuablePresets.Add(ValuablePresets.GenericPreset);
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
                        Logger.LogWarning($"Proxy preset \"{valuablePreset.name}\" in level \"{level.name}\" contains valuables! This might have caused duplicate valuables to load!");
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

        _levelsToRegister.Clear();
        _canRegisterLevels = false;
    }

    public static void RegisterLevel(Level level)
    {
        if (level == null)
        {
            Logger.LogError($"Failed to register level. Level is null.");
            return;
        }

        if (!_canRegisterLevels)
        {
            Logger.LogError($"Failed to register level \"{level.name}\". You can only register levels from your plugins awake!");
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
        
        foreach (var prefab in level.StartRooms)
        {
            modules.Add((prefab, ResourcesHelper.LevelPrefabType.StartRoom));
        }

        if (level.ConnectObject != null)
        {
            modules.Add((level.ConnectObject, ResourcesHelper.LevelPrefabType.Other));
        }

        if (level.BlockObject != null)
        {
            modules.Add((level.BlockObject, ResourcesHelper.LevelPrefabType.Other));
        }
        
        foreach (var (prefab, type) in modules)
        {
            // maybe check if the prefab is already registered?
            
            string prefabId = ResourcesHelper.GetLevelPrefabPath(level, prefab, type);
            NetworkPrefabs.RegisterNetworkPrefab(prefabId, prefab);

            Utilities.FixAudioMixerGroups(prefab);
        }

        _levelsToRegister.Add(level);
    }

    public static IReadOnlyList<Level> GetLevels()
    {
        if (RunManager.instance == null)
        {
            return [];
        }

        return RunManager.instance.levels;
    }

    public static bool TryGetLevelByName(string name, out Level level)
    {
        level = GetLevelByName(name);
        return level != null;
    }

    public static Level GetLevelByName(string name)
    {
        foreach (var level in GetLevels())
        {
            if (level.name.EqualsAny([name, $"Level - {name}"], StringComparison.OrdinalIgnoreCase))
            {
                return level;
            }
        }

        return default;
    }

    public static bool TryGetLevelThatContainsName(string name, out Level valuableObject)
    {
        valuableObject = GetLevelThatContainsName(name);
        return valuableObject != null;
    }

    public static Level GetLevelThatContainsName(string name)
    {
        foreach (var level in GetLevels())
        {
            if (level.name.Contains(name, StringComparison.OrdinalIgnoreCase))
            {
                return level;
            }
        }

        return default;
    }
}
