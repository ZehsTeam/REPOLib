﻿using REPOLib.Extensions;
using UnityEngine;

namespace REPOLib.Modules;

// TODO: Document this.
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public static class ResourcesHelper
{
    #region Folder Paths
    public static string GetValuablesFolderPath(ValuableVolume.Type volumeType)
    {
        string folder = volumeType switch
        {
            ValuableVolume.Type.Tiny =>     "01 Tiny",
            ValuableVolume.Type.Small =>    "02 Small",
            ValuableVolume.Type.Medium =>   "03 Medium",
            ValuableVolume.Type.Big =>      "04 Big",
            ValuableVolume.Type.Wide =>     "05 Wide",
            ValuableVolume.Type.Tall =>     "06 Tall",
            ValuableVolume.Type.VeryTall => "07 Very Tall",
            _ => string.Empty
        };

        return $"Valuables/{folder}";
    }

    public static string GetItemsFolderPath()
    {
        return "Items";
    }

    public static string GetEnemiesFolderPath()
    {
        return "Enemies";
    }

    public static string GetModulesFolderPath()
    {
        return "MiscModules";
    }
    
    public static string GetLevelPrefabsFolderPath(Level level, LevelPrefabType type)
    {
        string folder = type switch
        {
            LevelPrefabType.Module => "Modules",
            LevelPrefabType.Other => "Other",
            LevelPrefabType.StartRoom => "Start Room",
            _ => string.Empty
        };

        return $"Level/{level.ResourcePath}/{folder}";
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

        return $"{folderPath}/{valuableObject.gameObject.name}";
    }

    public static string GetValuablePrefabPath(GameObject prefab)
    {
        if (prefab == null)
        {
            return string.Empty;
        }

        if (prefab.TryGetComponent(out ValuableObject valuableObject))
        {
            return GetValuablePrefabPath(valuableObject);
        }

        return string.Empty;
    }

    public static string GetItemPrefabPath(Item item)
    {
        if (item == null)
        {
            return string.Empty;
        }

        return GetItemPrefabPath(item.prefab);
    }

    public static string GetItemPrefabPath(GameObject prefab)
    {
        if (prefab == null)
        {
            return string.Empty;
        }

        string folderPath = GetItemsFolderPath();

        return $"{folderPath}/{prefab.name}";
    }

    public static string GetEnemyPrefabPath(EnemySetup enemySetup)
    {
        if (enemySetup == null || enemySetup.spawnObjects == null)
        {
            return string.Empty;
        }

        GameObject? mainSpawnObject = enemySetup.GetMainSpawnObject();

        if (mainSpawnObject == null)
        {
            return string.Empty;
        }

        string folderPath = GetEnemiesFolderPath();

        return $"{folderPath}/{mainSpawnObject.name}";
    }

    public static string GetEnemyPrefabPath(GameObject prefab)
    {
        if (prefab == null)
        {
            return string.Empty;
        }

        string folderPath = GetEnemiesFolderPath();

        return $"{folderPath}/{prefab.name}";
    }

    public static string GetModulePrefabPath(GameObject prefab)
    {
        if (prefab == null)
        {
            return string.Empty;
        }

        string folderPath = GetModulesFolderPath();

        return $"{folderPath}/{prefab.name}";
    }
    
    public enum LevelPrefabType
    {
        Module,
        Other,
        StartRoom
    }

    public static string GetLevelPrefabPath(Level level, GameObject prefab, LevelPrefabType type)
    {
        if (prefab == null)
        {
            return string.Empty;
        }

        string folderPath = GetLevelPrefabsFolderPath(level, type);

        return $"{folderPath}/{prefab.name}";
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

        foreach (var spawnObject in enemySetup.GetDistinctSpawnObjects())
        {
            string prefabPath = GetEnemyPrefabPath(spawnObject);

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

    public static bool HasPrefab(string prefabId)
    {
        return Resources.Load<GameObject>(prefabId) != null;
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
