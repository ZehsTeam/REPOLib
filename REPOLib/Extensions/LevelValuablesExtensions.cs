using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;

namespace REPOLib.Extensions;

internal static class LevelValuablesExtensions
{
    public static bool AddValuable(this LevelValuables levelValuables, PrefabRef prefabRef)
    {
        GameObject prefab = prefabRef.Prefab;
        if (prefab == null) return false;

        if (!prefab.TryGetComponent(out ValuableObject valuableObject))
        {
            return false;
        }

        if (!TryGetList(levelValuables, valuableObject.volumeType, out List<PrefabRef>? list))
        {
            return false;
        }

        if (list.Contains(prefabRef))
        {
            return false;
        }

        list.Add(prefabRef);
        return true;
    }

    public static bool TryGetList(this LevelValuables levelValuables, ValuableVolume.Type volumeType, [NotNullWhen(true)] out List<PrefabRef>? list)
    {
        list = volumeType switch
        {
            ValuableVolume.Type.Tiny => levelValuables.tiny,
            ValuableVolume.Type.Small => levelValuables.small,
            ValuableVolume.Type.Medium => levelValuables.medium,
            ValuableVolume.Type.Big => levelValuables.big,
            ValuableVolume.Type.Wide => levelValuables.wide,
            ValuableVolume.Type.Tall => levelValuables.tall,
            ValuableVolume.Type.VeryTall => levelValuables.veryTall,
            _ => null
        };

        return list != null;
    }

    public static bool TryGetCombinedList(this LevelValuables levelValuables, out List<PrefabRef> list)
    {
        var allValuables = new List<List<PrefabRef>>()
        {
            levelValuables.tiny,
            levelValuables.small,
            levelValuables.medium,
            levelValuables.big,
            levelValuables.wide,
            levelValuables.tall,
            levelValuables.veryTall,
        };

        list = allValuables.SelectMany(valuables => valuables)
            .Where(x => x != null)
            .Distinct()
            .ToList();

        return list != null;
    }

    public static List<PrefabRef> GetCombinedList(this LevelValuables levelValuables)
    {
        if (levelValuables.TryGetCombinedList(out List<PrefabRef> list))
        {
            return list;
        }

        return [];
    }
}
