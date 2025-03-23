using System;
using System.Collections.Generic;

namespace REPOLib.Extensions;

public static class EnemyDirectorExtension
{
    public static bool HasEnemy(this EnemyDirector enemyDirector, EnemySetup enemySetup)
    {
        if (enemySetup == null || enemySetup.spawnObjects.Count == 0)
        {
            return false;
        }

        foreach (var spawnObject in enemySetup.spawnObjects)
        {
            if (!spawnObject.TryGetComponent(out EnemyParent enemyParent))
            {
                continue;
            }

            if (!TryGetList(enemyDirector, enemyParent.difficulty, out List<EnemySetup> list))
            {
                continue;
            }

            return list.Contains(enemySetup);
        }

        return false;
    }

    internal static bool AddEnemy(this EnemyDirector enemyDirector, EnemySetup enemySetup)
    {
        if (enemySetup == null)
        {
            return false;
        }

        foreach (var spawnObject in enemySetup.spawnObjects)
        {
            if (!spawnObject.TryGetComponent(out EnemyParent enemyParent))
            {
                continue;
            }

            if (!TryGetList(enemyDirector, enemyParent.difficulty, out List<EnemySetup> list))
            {
                continue;
            }

            if (list.Contains(enemySetup))
            {
                continue;
            }

            list.Add(enemySetup);
            return true;
        }
        
        return false;
    }
    
    public static bool TryGetList(this EnemyDirector enemyDirector, EnemyParent.Difficulty difficultyType, out List<EnemySetup> list)
    {
        list = difficultyType switch
        {
            EnemyParent.Difficulty.Difficulty1 => enemyDirector.enemiesDifficulty1,
            EnemyParent.Difficulty.Difficulty2 => enemyDirector.enemiesDifficulty2,
            EnemyParent.Difficulty.Difficulty3 => enemyDirector.enemiesDifficulty3,
            _ => null
        };

        return list != null;
    }

    public static List<EnemySetup> GetEnemies(this EnemyDirector enemyDirector)
    {
        return [
            .. enemyDirector.enemiesDifficulty1,
            .. enemyDirector.enemiesDifficulty2,
            .. enemyDirector.enemiesDifficulty3,
        ];
    }

    public static EnemySetup GetEnemyByName(this EnemyDirector enemyDirector, string name)
    {
        foreach (var enemySetup in GetEnemies(enemyDirector))
        {
            if (enemySetup.name.Contains(name, StringComparison.OrdinalIgnoreCase))
            {
                return enemySetup;
            }

            if (!enemySetup.TryGetEnemyParent(out EnemyParent enemyParent))
            {
                continue;
            }

            if (enemyParent.gameObject.name.Contains(name, StringComparison.OrdinalIgnoreCase))
            {
                return enemySetup;
            }

            if (enemyParent.enemyName.Contains(name, StringComparison.OrdinalIgnoreCase))
            {
                return enemySetup;
            }
        }

        return null;
    }

    public static bool TryGetEnemyByName(this EnemyDirector enemyDirector, string name, out EnemySetup enemySetup)
    {
        enemySetup = enemyDirector.GetEnemyByName(name);
        return enemySetup != null;
    }
}