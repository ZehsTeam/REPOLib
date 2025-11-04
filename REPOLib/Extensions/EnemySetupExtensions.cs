using REPOLib.Objects;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace REPOLib.Extensions;

internal static class EnemySetupExtensions
{
    public static List<PrefabRef> GetDistinctSpawnObjects(this EnemySetup enemySetup)
    {
        return enemySetup.spawnObjects
            .Where(x => x != null)
            .Distinct(new PrefabRefComparer())
            .ToList();
    }

    public static EnemyParent? GetEnemyParent(this EnemySetup enemySetup)
    {
        foreach (var spawnObject in GetDistinctSpawnObjects(enemySetup))
        {
            if (spawnObject.Prefab.TryGetComponent(out EnemyParent enemyParent))
            {
                return enemyParent;
            }
        }

        return null;
    }

    public static bool TryGetEnemyParent(this EnemySetup enemySetup, [NotNullWhen(true)] out EnemyParent? enemyParent)
    {
        enemyParent = enemySetup.GetEnemyParent();
        return enemyParent != null;
    }
}
