using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace REPOLib.Extensions;

internal static class EnemyDirectorExtensions
{
    public static bool HasEnemy(this EnemyDirector enemyDirector, EnemySetup enemySetup)
    {
        if (enemySetup == null || enemySetup.spawnObjects.Count == 0)
        {
            return false;
        }

        if (!enemySetup.TryGetEnemyParent(out EnemyParent? enemyParent))
        {
            return false;
        }

        if (!TryGetList(enemyDirector, enemyParent.difficulty, out List<EnemySetup>? list))
        {
            return false;
        }

        return list.Contains(enemySetup);
    }

    internal static bool AddEnemy(this EnemyDirector enemyDirector, EnemySetup enemySetup)
    {
        if (enemySetup == null)
        {
            return false;
        }

        if (!enemySetup.TryGetEnemyParent(out EnemyParent? enemyParent))
        {
            return false;
        }

        if (!TryGetList(enemyDirector, enemyParent.difficulty, out List<EnemySetup>? list))
        {
            return false;
        }

        if (list.Contains(enemySetup))
        {
            return false;
        }

        list.Add(enemySetup);
        return true;
    }

    public static bool TryGetList(this EnemyDirector enemyDirector, EnemyParent.Difficulty difficultyType, [NotNullWhen(true)] out List<EnemySetup>? list)
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
}