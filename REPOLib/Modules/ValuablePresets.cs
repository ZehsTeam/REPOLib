using System;
using System.Collections.Generic;

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
    /// The generic valuables preset name.
    /// </summary>
    public const string GenericValuablePresetName = "Valuables - Generic";

    /// <summary>
    /// Gets the generic valuables preset.
    /// </summary>
    public static LevelValuables GenericValuablePreset => AllValuablePresets.GetValueOrDefault(GenericValuablePresetName);

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

        AddGenericValuablePresetToVanillaLevels();
    }

    private static void AddGenericValuablePresetToVanillaLevels()
    {
        LevelValuables valuablePreset = GenericValuablePreset;

        if (valuablePreset == null)
        {
            Logger.LogError($"Failed to add \"{GenericValuablePresetName}\" valuable preset to vanilla levels. Valuable preset is null.");
            return;
        }

        valuablePreset.tiny.Clear();
        valuablePreset.small.Clear();
        valuablePreset.medium.Clear();
        valuablePreset.big.Clear();
        valuablePreset.wide.Clear();
        valuablePreset.tall.Clear();
        valuablePreset.veryTall.Clear();

        foreach (var level in RunManager.instance.levels)
        {
            if (level.ValuablePresets.Contains(valuablePreset))
            {
                Logger.LogInfo($"Level \"{level.name}\" already has the valuable preset \"{valuablePreset.name}\"", extended: true);
                continue;
            }

            level.ValuablePresets.Add(valuablePreset);

            Logger.LogInfo($"Added valuable preset \"{valuablePreset.name}\" to level \"{level.name}\"", extended: true);
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
        RemoveMissingValuables(valuablePreset, valuablePreset.tiny);
        RemoveMissingValuables(valuablePreset, valuablePreset.small);
        RemoveMissingValuables(valuablePreset, valuablePreset.medium);
        RemoveMissingValuables(valuablePreset, valuablePreset.big);
        RemoveMissingValuables(valuablePreset, valuablePreset.wide);
        RemoveMissingValuables(valuablePreset, valuablePreset.tall);
        RemoveMissingValuables(valuablePreset, valuablePreset.veryTall);

        _valuablePresets.Add(valuablePreset.name, valuablePreset);
    }

    private static void RemoveMissingValuables(LevelValuables valuablePreset, List<PrefabRef> prefabRefs)
    {
        int count = prefabRefs.Count;

        for (int i = count - 1; i >= 0; i--)
        {
            PrefabRef prefabRef = prefabRefs[i];

            if (!prefabRef.IsValid() || prefabRef.Prefab == null)
            {
                Logger.LogWarning($"Valuable preset \"{valuablePreset.name}\" has an invalid valuable PrefabRef at index {i}");
                prefabRefs.RemoveAt(i);
                continue;
            }
        }
    }

    #region Deprecated
    #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    [Obsolete("Use GenericValuablePreset instead.", error: true)]
    public static LevelValuables GenericPreset => GenericValuablePreset;
    #pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    #endregion
}
