using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace REPOLib.Modules;

/// <summary>
///     ValuablePresets for Valuables.
/// </summary>
[PublicAPI]
public static class ValuablePresets
{
    /// <summary>
    ///     The generic valuables preset name.
    /// </summary>
    public const string GenericValuablePresetName = "Valuables - Generic";

    private static readonly Dictionary<string, LevelValuables> _valuablePresets = [];

    /// <summary>
    ///     Gets all cached valuable presets. See <see cref="CacheValuablePresets" />.
    /// </summary>
    public static IReadOnlyDictionary<string, LevelValuables> AllValuablePresets
        => _valuablePresets;

    /// <summary>
    ///     Gets the generic valuables preset.
    /// </summary>
    public static LevelValuables GenericPreset
        => AllValuablePresets[GenericValuablePresetName];

    /// <summary>
    ///     Caches all valuable presets from levels, for getting with <see cref="AllValuablePresets" />.
    /// </summary>
    public static void CacheValuablePresets()
    {
        if (RunManager.instance == null)
        {
            Logger.LogError("Failed to cache LevelValuables. RunManager instance is null.");
            return;
        }

        foreach (LevelValuables? valuablePreset in RunManager.instance.levels.SelectMany(level => level.ValuablePresets))
            _valuablePresets.TryAdd(valuablePreset.name, valuablePreset);
    }

    internal static void RegisterValuablePreset(LevelValuables valuablePreset)
        => _valuablePresets.Add(valuablePreset.name, valuablePreset);
}