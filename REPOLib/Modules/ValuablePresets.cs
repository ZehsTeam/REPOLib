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

    internal static string GenericValuablePresetName => "Valuables - Generic";
    internal static LevelValuables? GenericValuablePreset => AllValuablePresets.GetValueOrDefault(GenericValuablePresetName);

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
}
