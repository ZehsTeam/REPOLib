using System.Collections.Generic;
using UnityEngine;

namespace REPOLib.Extensions;

public static class EnemyDirectorExtension
{
    

    public static bool AddEnemy(this EnemyDirector enemyDirector, EnemySetup enemySetup)
    {
        if (enemySetup == null)
            return false;

        foreach (var spawnObject in enemySetup.spawnObjects)
        {
            if (spawnObject.TryGetComponent(out EnemyParent enemyParent))
            {
                if (TryGetList(enemyDirector, enemyParent.difficulty, out List<EnemySetup> list))
                {
                    list.Add(enemySetup);
                    return true;
                }
            }
        }
        
        return false;
    }

    public static bool HasEnemy(this EnemyDirector enemyDirector, EnemySetup enemySetup)
    {
        if (enemySetup == null || enemySetup.spawnObjects.Count == 0)
            return false;

        foreach (var spawnObject in enemySetup.spawnObjects)
        {
            if (spawnObject.TryGetComponent(out EnemyParent enemyParent))
            {
                if (TryGetList(enemyDirector, enemyParent.difficulty, out List<EnemySetup> list))
                {
                    return list.Contains(enemySetup);
                }
            }
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
}