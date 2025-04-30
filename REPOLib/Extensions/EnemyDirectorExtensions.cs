using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace REPOLib.Extensions;

internal static class EnemyDirectorExtensions
{
    public static bool HasEnemy(this EnemyDirector enemyDirector, EnemySetup enemySetup)
    {
        if (enemySetup == null || enemySetup.spawnObjects.Count == 0)
            return false;

        foreach (GameObject? spawnObject in enemySetup.spawnObjects)
        {
            if (!spawnObject.TryGetComponent(out EnemyParent enemyParent))
                continue;

            if (!TryGetList(enemyDirector, enemyParent.difficulty, out List<EnemySetup>? list))
                continue;

            return list.Contains(enemySetup);
        }

        return false;
    }

    internal static bool AddEnemy(this EnemyDirector enemyDirector, EnemySetup enemySetup)
    {
        if (enemySetup == null)
            return false;

        foreach (GameObject? spawnObject in enemySetup.spawnObjects)
        {
            if (!spawnObject.TryGetComponent(out EnemyParent enemyParent))
                continue;

            if (!TryGetList(enemyDirector, enemyParent.difficulty, out List<EnemySetup>? list))
                continue;

            if (list.Contains(enemySetup))
                continue;

            list.Add(enemySetup);
            return true;
        }

        return false;
    }

    private static bool TryGetList(this EnemyDirector enemyDirector, EnemyParent.Difficulty difficultyType,
        [NotNullWhen(true)] out List<EnemySetup>? list)
    {
        list = difficultyType switch
        { EnemyParent.Difficulty.Difficulty1 => enemyDirector.enemiesDifficulty1,
          EnemyParent.Difficulty.Difficulty2 => enemyDirector.enemiesDifficulty2,
          EnemyParent.Difficulty.Difficulty3 => enemyDirector.enemiesDifficulty3,
          _ => null };

        return list != null;
    }

    public static List<EnemySetup> GetEnemies(this EnemyDirector enemyDirector) =>
    [ 
        ..enemyDirector.enemiesDifficulty1,
        ..enemyDirector.enemiesDifficulty2,
        ..enemyDirector.enemiesDifficulty3 
    ];

    public static bool TryGetEnemyByName(this EnemyDirector enemyDirector, string? name, [NotNullWhen(true)] out EnemySetup? enemySetup)
        => (enemySetup = enemyDirector.GetEnemyByName(name)) != null;

    public static EnemySetup? GetEnemyByName(this EnemyDirector enemyDirector, string? name)
        => enemyDirector.GetEnemies()
                        .FirstOrDefault(x => x.NameEquals(name));

    public static bool TryGetEnemyThatContainsName(this EnemyDirector enemyDirector, string name, [NotNullWhen(true)] out EnemySetup? enemySetup)
        => (enemySetup = enemyDirector.GetEnemyThatContainsName(name)) != null;

    public static EnemySetup? GetEnemyThatContainsName(this EnemyDirector enemyDirector, string name)
        => enemyDirector.GetEnemies()
                        .FirstOrDefault(x => x.NameContains(name));
}