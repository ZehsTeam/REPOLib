using REPOLib.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;

namespace REPOLib.Extensions;

internal static class EnemySetupExtensions
{
    public static List<GameObject> GetDistinctSpawnObjects(this EnemySetup? enemySetup)
    {
        if (enemySetup == null || enemySetup.spawnObjects == null)
            return [];

        return enemySetup.spawnObjects
            .Where(x => x != null)
            .Distinct(new UnityObjectNameComparer<GameObject>()).ToList();
    }

    public static List<GameObject> GetSortedSpawnObjects(this EnemySetup? enemySetup)
    {
        if (enemySetup == null || enemySetup.spawnObjects == null)
            return [];

        return enemySetup.spawnObjects
            .Where(x => x != null)
            .OrderByDescending(x => x.TryGetComponent<EnemyParent>(out _))
            .ToList();
    }

    public static GameObject? GetMainSpawnObject(this EnemySetup? enemySetup)
        => GetEnemyParent(enemySetup)?.gameObject;

    public static EnemyParent? GetEnemyParent(this EnemySetup? enemySetup)
    {
        foreach (var spawnObject in GetDistinctSpawnObjects(enemySetup))
            if (spawnObject.TryGetComponent(out EnemyParent enemyParent))
                return enemyParent;

        return null;
    }

    public static bool TryGetEnemyParent(this EnemySetup? enemySetup, [NotNullWhen(true)] out EnemyParent? enemyParent)
        => (enemyParent = enemySetup.GetEnemyParent()) != null;

    public static bool AnySpawnObjectsNameEquals(this EnemySetup enemySetup, string name)
        => enemySetup != null && enemySetup.GetDistinctSpawnObjects().Any(x => x.name.Equals(name, StringComparison.OrdinalIgnoreCase));

    public static bool AnySpawnObjectsNameEqualsAnother(this EnemySetup enemySetup, GameObject gameObject)
    {
        if (enemySetup == null || gameObject == null)
            return false;

        return enemySetup.GetDistinctSpawnObjects().Any(x => x.name.Equals(gameObject.name, StringComparison.OrdinalIgnoreCase) && x != gameObject);
    }

    public static bool NameEquals(this EnemySetup enemySetup, string? name)
    {
        if (enemySetup == null)
            return false;
        
        if (enemySetup.name.EqualsAny([name, $"Enemy - {name}"], StringComparison.OrdinalIgnoreCase))
            return true;

        if (!enemySetup.TryGetEnemyParent(out var enemyParent)) 
            return false;
        
        return enemyParent.enemyName.Equals(name, StringComparison.OrdinalIgnoreCase) 
               || enemyParent.gameObject.name.EqualsAny([name, $"Enemy - {name}"], StringComparison.OrdinalIgnoreCase);
    }

    public static bool NameContains(this EnemySetup enemySetup, string name)
    {
        if (enemySetup == null)
            return false;

        if (enemySetup.name.Contains(name, StringComparison.OrdinalIgnoreCase))
            return true;

        if (!enemySetup.TryGetEnemyParent(out var enemyParent)) 
            return false;
        
        return enemyParent.enemyName.Contains(name, StringComparison.OrdinalIgnoreCase) 
               || enemyParent.gameObject.name.Contains(name, StringComparison.OrdinalIgnoreCase);
    }
}
