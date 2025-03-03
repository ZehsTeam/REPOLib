using System.Collections.Generic;
using UnityEngine;

namespace REPOLib.Extensions;

internal static class LevelValuablesExtension
{
    public static bool HasValuable(this LevelValuables levelValuables, GameObject prefab)
    {
        if (!prefab.TryGetComponent(out ValuableObject valuableObject))
        {
            return false;
        }

        List<GameObject> gameObjects = GetList(levelValuables, valuableObject.volumeType);
        return gameObjects.Contains(prefab);
    }

    public static void AddValuable(this LevelValuables levelValuables, GameObject prefab)
    {
        if (levelValuables.HasValuable(prefab))
        {
            return;
        }

        if (!prefab.TryGetComponent(out ValuableObject valuableObject))
        {
            return;
        }

        List<GameObject> gameObjects = GetList(levelValuables, valuableObject.volumeType);
        gameObjects.Add(prefab);
    }

    public static List<GameObject> GetList(this LevelValuables levelValuables, ValuableVolume.Type volumeType)
    {
        List<GameObject> result = volumeType switch
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

        if (result == null)
        {
            Logger.LogWarning($"LevelValuablesExtension.GetList: Unknown ValuableVolume.Type \"{volumeType}\"");
            return [];
        }

        return result;
    }
}
