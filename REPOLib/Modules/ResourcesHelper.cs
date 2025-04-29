using System.Linq;
using JetBrains.Annotations;
using REPOLib.Extensions;
using UnityEngine;

namespace REPOLib.Modules;

/// <summary>
/// // TODO: Document this.
/// </summary>
[PublicAPI]
public static class ResourcesHelper
{
    #region Folder Paths
    /// <summary>
    /// // TODO: Document this.
    /// </summary>
    /// <param name="volumeType"></param>
    /// <returns></returns>
    public static string GetValuablesFolderPath(ValuableVolume.Type volumeType)
    {
        var folder = volumeType switch
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

    /// <summary>
    /// // TODO: Document this.
    /// </summary>
    /// <returns></returns>
    public static string GetItemsFolderPath() => "Items";

    /// <summary>
    /// // TODO: Document this.
    /// </summary>
    /// <returns></returns>
    public static string GetEnemiesFolderPath() => "Enemies";

    /// <summary>
    /// // TODO: Document this.
    /// </summary>
    /// <param name="level"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string GetLevelPrefabsFolderPath(Level level, LevelPrefabType type)
    {
        var folder = type switch
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
    /// <summary>
    /// // TODO: Document this.
    /// </summary>
    /// <param name="valuableObject"></param>
    /// <returns></returns>
    public static string GetValuablePrefabPath(ValuableObject valuableObject)
    {
        if (valuableObject == null)
            return string.Empty;

        var folderPath = GetValuablesFolderPath(valuableObject.volumeType);
        return $"{folderPath}/{valuableObject.gameObject.name}";
    }

    /// <summary>
    /// // TODO: Document this.
    /// </summary>
    /// <param name="prefab"></param>
    /// <returns></returns>
    public static string GetValuablePrefabPath(GameObject prefab)
    {
        if (prefab == null) return string.Empty;
        return prefab.TryGetComponent(out ValuableObject valuableObject) ? GetValuablePrefabPath(valuableObject) : string.Empty;
    }

    /// <summary>
    /// // TODO: Document this.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public static string GetItemPrefabPath(Item item)
        => item == null ? string.Empty : GetItemPrefabPath(item.prefab);

    /// <summary>
    /// // TODO: Document this.
    /// </summary>
    /// <param name="prefab"></param>
    /// <returns></returns>
    public static string GetItemPrefabPath(GameObject prefab)
    {
        if (prefab == null)
            return string.Empty;

        var folderPath = GetItemsFolderPath();
        return $"{folderPath}/{prefab.name}";
    }

    /// <summary>
    /// // TODO: Document this.
    /// </summary>
    /// <param name="enemySetup"></param>
    /// <returns></returns>
    public static string GetEnemyPrefabPath(EnemySetup enemySetup)
    {
        if (enemySetup == null || enemySetup.spawnObjects == null)
            return string.Empty;

        var mainSpawnObject = enemySetup.GetMainSpawnObject();
        if (mainSpawnObject == null)
            return string.Empty;

        var folderPath = GetEnemiesFolderPath();
        return $"{folderPath}/{mainSpawnObject.name}";
    }

    /// <summary>
    /// // TODO: Document this.
    /// </summary>
    /// <param name="prefab"></param>
    /// <returns></returns>
    public static string GetEnemyPrefabPath(GameObject prefab)
    {
        if (prefab == null) return string.Empty;
        var folderPath = GetEnemiesFolderPath();
        return $"{folderPath}/{prefab.name}";
    }

    /// <summary>
    /// // TODO: Document this.
    /// </summary>
    public enum LevelPrefabType
    {
        /// <summary>
        /// // TODO: Document this.
        /// </summary>
        Module,
        /// <summary>
        /// // TODO: Document this.
        /// </summary>
        Other,
        /// <summary>
        /// // TODO: Document this.
        /// </summary>
        StartRoom
    }

    /// <summary>
    /// // TODO: Document this.
    /// </summary>
    /// <param name="level"></param>
    /// <param name="prefab"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string GetLevelPrefabPath(Level level, GameObject prefab, LevelPrefabType type)
    {
        if (prefab == null) return string.Empty;
        var folderPath = GetLevelPrefabsFolderPath(level, type);
        return $"{folderPath}/{prefab.name}";
    }
    #endregion

    /// <summary>
    /// // TODO: Document this.
    /// </summary>
    /// <param name="valuableObject"></param>
    /// <returns></returns>
    public static bool HasValuablePrefab(ValuableObject valuableObject)
    {
        if (valuableObject == null) return false;
        var prefabPath = GetValuablePrefabPath(valuableObject);
        return Resources.Load<GameObject>(prefabPath) != null;
    }

    /// <summary>
    /// // TODO: Document this.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public static bool HasItemPrefab(Item item)
    {
        if (item == null) return false;
        var prefabPath = GetItemPrefabPath(item);
        return Resources.Load<GameObject>(prefabPath) != null;
    }

    /// <summary>
    /// // TODO: Document this.
    /// </summary>
    /// <param name="enemySetup"></param>
    /// <returns></returns>
    public static bool HasEnemyPrefab(EnemySetup enemySetup)
    {
        if (enemySetup == null)
            return false;
        
        foreach (var prefabPath in enemySetup.GetDistinctSpawnObjects().Select(GetEnemyPrefabPath))
            if (Resources.Load<GameObject>(prefabPath) != null)
                return true;

        return false;
    }

    /// <summary>
    /// // TODO: Document this.
    /// </summary>
    /// <param name="prefab"></param>
    /// <returns></returns>
    public static bool HasPrefab(GameObject prefab)
        => Resources.Load<GameObject>(prefab?.name) != null;
    

    /// <summary>
    /// // TODO: Document this.
    /// </summary>
    /// <param name="prefabId"></param>
    /// <returns></returns>
    public static bool HasPrefab(string prefabId)
        => Resources.Load<GameObject>(prefabId) != null;
}
