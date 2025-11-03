using System;
using System.Collections.Generic;
using System.Linq;

namespace REPOLib.Modules;

/// <summary>
/// ValuablePresets for Valuables.
/// </summary>
public static class ValuablePresets
{
    /// <summary>
    /// Gets all cached valuable presets. See <see cref="CacheValuablePresets"/>.
    /// </summary>
    public static IReadOnlyDictionary<string, LevelValuables> AllValuablePresets => _valuablePresets;

    [Obsolete("Levels no longer use a generic valuables preset", error: true)]
    public static LevelValuables? GenericPreset => null;

    private static readonly Dictionary<string, LevelValuables> _valuablePresets = [];

    [Obsolete("Levels no longer use a generic valuables preset", error: true)]
    public const string GenericValuablePresetName = "Valuables - Generic";

    /// <summary>
    /// All of the valuable preset names. Register your valuables to this list if you want them to spawn in all levels.
    /// </summary>
    public static List<string> AllValuablePresetNames => AllValuablePresets.Keys.ToList();

    /// <summary>
    /// Caches all valuable presets from levels, for getting with <see cref="AllValuablePresets"/>.
    /// </summary>
    public static void CacheValuablePresets()
    {
        if (RunManager.instance == null)
        {
            Logger.LogError($"Failed to cache LevelValuables. RunManager instance is null.");
            return;
        }

        foreach (var level in RunManager.instance.levels)
        {
            foreach (var valuablePreset in level.ValuablePresets)
            {
                _valuablePresets.TryAdd(valuablePreset.name, valuablePreset);
            }
        }
    }

    internal static void RegisterValuablePreset(LevelValuables valuablePreset)
    {
        _valuablePresets.Add(valuablePreset.name, valuablePreset);
    }
}
