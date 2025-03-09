using System.IO;
using UnityEngine;

namespace REPOLib.Modules;

public static class ResourcesHelper
{
    #region Folder Paths
    public static string GetValuablesFolderPath(ValuableVolume.Type volumeType)
    {
        string folder = volumeType switch
        {
            ValuableVolume.Type.Tiny =>     "01 tiny/",
            ValuableVolume.Type.Small =>    "02 small/",
            ValuableVolume.Type.Medium =>   "03 medium/",
            ValuableVolume.Type.Big =>      "04 big/",
            ValuableVolume.Type.Wide =>     "05 wide/",
            ValuableVolume.Type.Tall =>     "06 tall/",
            ValuableVolume.Type.VeryTall => "07 very tall/",
            _ => string.Empty
        };

        return Path.Combine("valuables/", folder);
    }

    public static string GetItemsFolderPath()
    {
        return "items/";
    }

    public static string GetEnemiesFolderPath()
    {
        return "enemies/";
    }
    #endregion

    #region Paths
    public static string GetValuablePrefabPath(ValuableObject valuableObject)
    {
        if (valuableObject == null)
        {
            return string.Empty;
        }

        string folderPath = GetValuablesFolderPath(valuableObject.volumeType);

        return Path.Combine(folderPath, valuableObject.gameObject.name);
    }

    public static string GetItemPrefabPath(Item item)
    {
        if (item == null || item.prefab == null)
        {
            return string.Empty;
        }

        string folderPath = GetItemsFolderPath();

        return Path.Combine(folderPath, item.prefab.name);
    }

    public static string GetEnemyPrefabPath(EnemySetup enemySetup)
    {
        if (enemySetup == null || enemySetup.spawnObjects == null)
        {
            return string.Empty;
        }

        string folderPath = GetEnemiesFolderPath();

        foreach (var spawnObject in enemySetup.spawnObjects)
        {
            if (spawnObject == null) continue;

            if (spawnObject.TryGetComponent(out EnemyParent enemyParent))
            {
                return Path.Combine(folderPath, spawnObject.name);
            }
        }
        
        return string.Empty;
    }
    #endregion

    public static bool HasValuablePrefab(ValuableObject valuableObject)
    {
        if (valuableObject == null)
        {
            return false;
        }

        string prefabPath = GetValuablePrefabPath(valuableObject);

        return Resources.Load<GameObject>(prefabPath) != null;
    }

    public static bool HasItemPrefab(Item item)
    {
        if (item == null)
        {
            return false;
        }

        string prefabPath = GetItemPrefabPath(item);

        return Resources.Load<GameObject>(prefabPath) != null;
    }

    public static bool HasEnemyPrefab(EnemySetup enemySetup)
    {
        if (enemySetup == null)
        {
            return false;
        }

        string folderPath = GetEnemiesFolderPath();

        foreach (var spawnObject in enemySetup.spawnObjects)
        {
            if (spawnObject == null) continue;

            string prefabPath = Path.Combine(folderPath, spawnObject.name);

            if (Resources.Load<GameObject>(prefabPath) != null)
            {
                return true;
            }
        }

        return false;
    }

    public static bool HasPrefab(GameObject prefab)
    {
        return Resources.Load<GameObject>(prefab?.name) != null;
    }
}
