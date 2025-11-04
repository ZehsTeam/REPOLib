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
    /// Gets all cached valuable presets. See <see cref="Initialize"/>.
    /// </summary>
    public static IReadOnlyDictionary<string, LevelValuables> AllValuablePresets => _valuablePresets;
    private static readonly Dictionary<string, LevelValuables> _valuablePresets = [];

    /// <summary>
    /// All of the valuable preset names. Register your valuables to this list if you want them to spawn in all levels.
    /// </summary>
    public static List<string> AllValuablePresetNames => AllValuablePresets.Keys.ToList();

    internal static LevelValuables? GenericPreset => AllValuablePresets.GetValueOrDefault("Valuables - Generic");

    internal static void Initialize()
    {
        if (RunManager.instance == null)
        {
            Logger.LogError($"Failed to initialize ValuablePresets. RunManager instance is null.");
            return;
        }

        AddValuablePresets(RunManager.instance.levelArena.ValuablePresets);

        foreach (var level in RunManager.instance.levels)
        {
            AddValuablePresets(level.ValuablePresets);
        }
    }

    private static void AddValuablePresets(List<LevelValuables> valuablePresets)
    {
        foreach (var valuablePreset in valuablePresets)
        {
            _valuablePresets.TryAdd(valuablePreset.name, valuablePreset);
        }
    }

    internal static void RegisterValuablePreset(LevelValuables valuablePreset)
    {
        _valuablePresets.Add(valuablePreset.name, valuablePreset);
    }

    #region Deprecated
    #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    [Obsolete("Levels no longer use a generic valuables preset", error: true)]
    public const string GenericValuablePresetName = "Valuables - Generic";
    #pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    #endregion
}
