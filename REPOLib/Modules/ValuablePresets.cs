using System.Collections.Generic;

namespace REPOLib.Modules;

public static class ValuablePresets
{
    public static IReadOnlyDictionary<string, LevelValuables> AllValuablePresets => _valuablePresets;
    public static LevelValuables GenericPreset => AllValuablePresets[GenericValuablePresetName];
    
    private static readonly Dictionary<string, LevelValuables> _valuablePresets = [];
    
    public const string GenericValuablePresetName = "Valuables - Generic";
    
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
