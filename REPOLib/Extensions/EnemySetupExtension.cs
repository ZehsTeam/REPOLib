using REPOLib.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace REPOLib.Extensions;

public static class EnemySetupExtension
{
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

    public static GameObject GetMainSpawnObject(this EnemySetup enemySetup)
    {
        return GetEnemyParent(enemySetup)?.gameObject;
    }

    public static EnemyParent GetEnemyParent(this EnemySetup enemySetup)
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

    public static bool TryGetEnemyParent(this EnemySetup enemySetup, out EnemyParent enemyParent)
    {
        enemyParent = enemySetup.GetEnemyParent();
        return enemyParent != null;
    }

    public static bool AnySpawnObjectsNameEquals(this EnemySetup enemySetup, string name, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
    {
        if (enemySetup == null)
        {
            return false;
        }

        return enemySetup.GetDistinctSpawnObjects().Any(x => x.name.Equals(name, comparisonType));
    }

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

        if (enemySetup.TryGetEnemyParent(out EnemyParent enemyParent))
        {
            if (enemyParent.enemyName.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        if (enemyParent.gameObject.name.EqualsAny([name, $"Enemy - {name}"], StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

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

        if (enemySetup.TryGetEnemyParent(out EnemyParent enemyParent))
        {
            if (enemyParent.enemyName.Contains(name, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        if (enemyParent.gameObject.name.Contains(name, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }
}
