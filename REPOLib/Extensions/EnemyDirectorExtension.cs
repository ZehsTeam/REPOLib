using System.Collections.Generic;
using System.Linq;

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

    public static bool TryGetEnemyByName(this EnemyDirector enemyDirector, string name, out EnemySetup enemySetup)
    {
        enemySetup = enemyDirector.GetEnemyByName(name);
        return enemySetup != null;
    }

    public static EnemySetup GetEnemyByName(this EnemyDirector enemyDirector, string name)
    {
        return enemyDirector.GetEnemies()
            .FirstOrDefault(x => x.NameEquals(name));
    }

    public static bool TryGetEnemyThatContainsName(this EnemyDirector enemyDirector, string name, out EnemySetup enemySetup)
    {
        enemySetup = enemyDirector.GetEnemyThatContainsName(name);
        return enemySetup != null;
    }

    public static EnemySetup GetEnemyThatContainsName(this EnemyDirector enemyDirector, string name)
    {
        return enemyDirector.GetEnemies()
            .FirstOrDefault(x => x.NameContains(name));
    }
}