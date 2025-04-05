using REPOLib.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;

namespace REPOLib.Extensions;

/// <summary>
/// REPOLib's <see cref="EnemySetup"/> extension methods.
/// </summary>
public static class EnemySetupExtensions
{
    /// <summary>
    /// Gets unique <see cref="EnemySetup.spawnObjects"/> from the specified <see cref="EnemySetup"/>.
    /// </summary>
    /// <param name="enemySetup">The <see cref="EnemySetup"/> whose unique spawn objects to get.</param>
    /// <returns>A list of unique <see cref="EnemySetup.spawnObjects"/>.</returns>
    public static List<GameObject> GetDistinctSpawnObjects(this EnemySetup enemySetup)
    {
        if (enemySetup == null || enemySetup.spawnObjects == null)
        {
            return [];
        }

        return enemySetup.spawnObjects
            .Where(x => x != null)
            .Distinct(new UnityObjectNameComparer<GameObject>()).ToList();
    }

    /// <summary>
    /// Gets a sorted list of <see cref="EnemySetup.spawnObjects"/>.
    /// </summary>
    /// <param name="enemySetup">The <see cref="EnemySetup"/> whose spawn objects we want.</param>
    /// <returns>A sorted list of  <see cref="EnemySetup.spawnObjects"/>.</returns>
    public static List<GameObject> GetSortedSpawnObjects(this EnemySetup enemySetup)
    {
        if (enemySetup == null || enemySetup.spawnObjects == null)
        {
            return [];
        }

        return enemySetup.spawnObjects
            .Where(x => x != null)
            .OrderByDescending(x => x.TryGetComponent<EnemyParent>(out _))
            .ToList();
    }

    /// <summary>
    /// Gets the <see cref="EnemyParent"/> <see cref="GameObject"/> of the specified <see cref="EnemySetup"/>.
    /// </summary>
    /// <param name="enemySetup">The <see cref="EnemySetup"/> whose <see cref="EnemyParent"/> <see cref="GameObject"/> we want.</param>
    /// <returns>The <see cref="EnemyParent"/> <see cref="GameObject"/> or null.</returns>
    public static GameObject? GetMainSpawnObject(this EnemySetup enemySetup)
    {
        return GetEnemyParent(enemySetup)?.gameObject;
    }

    /// <summary>
    /// Gets the <see cref="EnemyParent"/> of the specified <see cref="EnemySetup"/>.
    /// </summary>
    /// <param name="enemySetup">The <see cref="EnemySetup"/> whose <see cref="EnemyParent"/> we want.</param>
    /// <returns>The found <see cref="EnemyParent"/> or null.</returns>
    public static EnemyParent? GetEnemyParent(this EnemySetup enemySetup)
    {
        foreach (var spawnObject in GetDistinctSpawnObjects(enemySetup))
        {
            if (spawnObject.TryGetComponent(out EnemyParent enemyParent))
            {
                return enemyParent;
            }
        }

        return null;
    }

    /// <summary>
    /// Tries to get the <see cref="EnemyParent"/> of the specified <see cref="EnemySetup"/>.
    /// </summary>
    /// <param name="enemySetup">The <see cref="EnemySetup"/> whose <see cref="EnemyParent"/> we want.</param>
    /// <param name="enemyParent">The found <see cref="EnemyParent"/>.</param>
    /// <returns>Whether or not the <see cref="EnemyParent"/> was found.</returns>
    public static bool TryGetEnemyParent(this EnemySetup enemySetup, [NotNullWhen(true)] out EnemyParent? enemyParent)
    {
        enemyParent = enemySetup.GetEnemyParent();
        return enemyParent != null;
    }

    /// <summary>
    /// Checks if the <see cref="EnemySetup"/> contains a spawn object with the specified <see cref="string"/>.
    /// </summary>
    /// <param name="enemySetup">The <see cref="EnemySetup"/> whose spawn objects to check against.</param>
    /// <param name="name">The <see cref="string"/> to check.</param>
    /// <returns></returns>
    public static bool AnySpawnObjectsNameEquals(this EnemySetup enemySetup, string name)
    {
        if (enemySetup == null)
        {
            return false;
        }

        return enemySetup.GetDistinctSpawnObjects().Any(x => x.name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if the <see cref="EnemySetup"/> contains a unique spawn object with the same name as the specified <see cref="GameObject"/>.
    /// </summary>
    /// <param name="enemySetup">The <see cref="EnemySetup"/> whose spawn objects to check against.</param>
    /// <param name="gameObject">The <see cref="GameObject"/> whose name to compare.</param>
    /// <returns></returns>
    public static bool AnySpawnObjectsNameEqualsThatIsNotTheSameObject(this EnemySetup enemySetup, GameObject gameObject)
    {
        if (enemySetup == null || gameObject == null)
        {
            return false;
        }

        return enemySetup.GetDistinctSpawnObjects().Any(x => x.name.Equals(gameObject.name, StringComparison.OrdinalIgnoreCase) && x != gameObject);
    }

    /// <summary>
    /// Check if an <see cref="EnemySetup"/> or its <see cref="EnemyParent"/> matches the specified name (case insensitive).
    /// </summary>
    /// <param name="enemySetup">The <see cref="EnemySetup"/> to match against.</param>
    /// <param name="name">The <see cref="string"/> to match.</param>
    /// <returns>Whether or not there was a match.</returns>
    public static bool NameEquals(this EnemySetup enemySetup, string name)
    {
        if (enemySetup == null)
        {
            return false;
        }

        if (enemySetup.name.EqualsAny([name, $"Enemy - {name}"], StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (enemySetup.TryGetEnemyParent(out EnemyParent? enemyParent))
        {
            if (enemyParent.enemyName.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (enemyParent.gameObject.name.EqualsAny([name, $"Enemy - {name}"], StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Check if an <see cref="EnemySetup"/> or its <see cref="EnemyParent"/> contains the specified name (case insensitive).
    /// </summary>
    /// <param name="enemySetup">The <see cref="EnemySetup"/> to match against.</param>
    /// <param name="name">The <see cref="string"/> to compare.</param>
    /// <returns>Whether or not the <see cref="EnemySetup"/> contains the name.</returns>
    public static bool NameContains(this EnemySetup enemySetup, string name)
    {
        if (enemySetup == null)
        {
            return false;
        }

        if (enemySetup.name.Contains(name, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (enemySetup.TryGetEnemyParent(out EnemyParent? enemyParent))
        {
            if (enemyParent.enemyName.Contains(name, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (enemyParent.gameObject.name.Contains(name, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
