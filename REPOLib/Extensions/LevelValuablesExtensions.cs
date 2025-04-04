using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;

namespace REPOLib.Extensions;

/// <summary>
/// REPOLib's <see cref="LevelValuables"/> extension methods.
/// </summary>
public static class LevelValuablesExtensions
{
    /// <summary>
    /// Check if the <see cref="LevelValuables"/> has the specified valuable <see cref="GameObject"/>.
    /// </summary>
    /// <param name="levelValuables">The <see cref="LevelValuables"/> object.</param>
    /// <param name="prefab">The <see cref="GameObject"/> to check.</param>
    /// <returns>Whether or not the <see cref="LevelValuables"/> object has the valuable <see cref="GameObject"/>.</returns>
    public static bool HasValuable(this LevelValuables levelValuables, GameObject prefab)
    {
        if (!prefab.TryGetComponent(out ValuableObject valuableObject))
        {
            return false;
        }

        if (!TryGetList(levelValuables, valuableObject.volumeType, out List<GameObject>? list))
        {
            return false;
        }

        return list.Contains(prefab);
    }

    internal static bool AddValuable(this LevelValuables levelValuables, GameObject prefab)
    {
        if (!prefab.TryGetComponent(out ValuableObject valuableObject))
        {
            return false;
        }

        if (!TryGetList(levelValuables, valuableObject.volumeType, out List<GameObject>? list))
        {
            return false;
        }

        if (list.Contains(prefab))
        {
            return false;
        }

        list.Add(prefab);
        return true;
    }

    /// <summary>
    /// Tries to get a list of valuable <see cref="GameObject"/>s for the specified <see cref="ValuableVolume.Type"/>.
    /// </summary>
    /// <param name="levelValuables">The <see cref="LevelValuables"/> object.</param>
    /// <param name="volumeType">The specified <see cref="ValuableVolume.Type"/>.</param>
    /// <param name="list">The list of valuable <see cref="GameObject"/>s.</param>
    /// <returns>Whether or not the <see cref="ValuableVolume.Type"/> is in valid range and the corresponding list isn't null.</returns>
    public static bool TryGetList(this LevelValuables levelValuables, ValuableVolume.Type volumeType, [NotNullWhen(true)] out List<GameObject>? list)
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

    /// <summary>
    /// Tries to get a list of valuable <see cref="GameObject"/>s for all <see cref="ValuableVolume.Type"/>s.
    /// </summary>
    /// <param name="levelValuables">The <see cref="LevelValuables"/> object.</param>
    /// <param name="list">The list of all valuable <see cref="GameObject"/>s.</param>
    /// <returns>Always true.</returns>
    public static bool TryGetCombinedList(this LevelValuables levelValuables, out List<GameObject> list)
    {
        var allValuables = new List<List<GameObject>>()
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

    /// <summary>
    /// Gets a list of valuable <see cref="GameObject"/>s for all <see cref="ValuableVolume.Type"/>s.
    /// </summary>
    /// <param name="levelValuables">The <see cref="LevelValuables"/> object.</param>
    /// <returns>The list of all valuable <see cref="GameObject"/>s.</returns>
    public static List<GameObject> GetCombinedList(this LevelValuables levelValuables)
    {
        if (levelValuables.TryGetCombinedList(out List<GameObject> list))
        {
            return list;
        }

        return [];
    }
}
