using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace REPOLib.Extensions;

internal static class LevelValuablesExtensions
{
    public static bool HasValuable(this LevelValuables levelValuables, GameObject prefab)
    {
        if (!prefab.TryGetComponent(out ValuableObject valuableObject))
            return false;

        return TryGetList(levelValuables, valuableObject.volumeType, out List<GameObject>? list) &&
               list.Contains(prefab);
    }

    internal static bool AddValuable(this LevelValuables levelValuables, GameObject prefab)
    {
        if (!prefab.TryGetComponent(out ValuableObject valuableObject))
            return false;

        if (!TryGetList(levelValuables, valuableObject.volumeType, out List<GameObject>? list))
            return false;

        if (list.Contains(prefab))
            return false;

        list.Add(prefab);
        return true;
    }

    public static bool TryGetList(this LevelValuables levelValuables, ValuableVolume.Type volumeType,
        [NotNullWhen(true)] out List<GameObject>? list)
    {
        list = volumeType switch
        { ValuableVolume.Type.Tiny => levelValuables.tiny,
          ValuableVolume.Type.Small => levelValuables.small,
          ValuableVolume.Type.Medium => levelValuables.medium,
          ValuableVolume.Type.Big => levelValuables.big,
          ValuableVolume.Type.Wide => levelValuables.wide,
          ValuableVolume.Type.Tall => levelValuables.tall,
          ValuableVolume.Type.VeryTall => levelValuables.veryTall,
          _ => null };

        return list != null;
    }
    
    public static IEnumerable<GameObject> GetAllValuables(this LevelValuables levelValuables) => 
    [
        ..levelValuables.tiny,
        ..levelValuables.small,
        ..levelValuables.medium,
        ..levelValuables.big,
        ..levelValuables.wide,
        ..levelValuables.tall,
        ..levelValuables.veryTall 
    ];

    public static bool TryGetCombinedList(this LevelValuables levelValuables, out List<GameObject> list)
    {
        list = levelValuables.GetAllValuables()
                           .Where(x => x != null)
                           .Distinct()
                           .ToList();

        return true;
    }

    public static List<GameObject> GetCombinedList(this LevelValuables levelValuables)
        => levelValuables.TryGetCombinedList(out List<GameObject> list) ? list : [];
}