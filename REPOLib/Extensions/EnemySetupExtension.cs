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
}
