using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace REPOLib.Extensions;

/// <summary>
/// REPOLib's <see cref="EnemyDirector"/> extension methods.
/// </summary>
public static class EnemyDirectorExtensions
{
    /// <summary>
    /// Checks if the <see cref="EnemyDirector"/> contains the specified <see cref="EnemySetup"/>.
    /// </summary>
    /// <param name="enemyDirector">The <see cref="EnemyDirector"/> whose enemies to check.</param>
    /// <param name="enemySetup">The <see cref="EnemySetup"/> to check against.</param>
    /// <returns>Whether or not the <see cref="EnemyDirector"/> contains the specified <see cref="EnemySetup"/>.</returns>
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

            if (!TryGetList(enemyDirector, enemyParent.difficulty, out List<EnemySetup>? list))
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

            if (!TryGetList(enemyDirector, enemyParent.difficulty, out List<EnemySetup>? list))
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

    /// <summary>
    /// Tries to get a list of <see cref="EnemySetup"/> with the specified <see cref="EnemyParent.Difficulty"/>
    /// from the <see cref="EnemyDirector"/>.
    /// </summary>
    /// <param name="enemyDirector">The <see cref="EnemyDirector"/> whose enemies to get.</param>
    /// <param name="difficultyType">The <see cref="EnemyParent.Difficulty"/> we want.</param>
    /// <param name="list">The list of <see cref="EnemySetup"/> objects.</param>
    /// <returns>Whether or not the the defined <see cref="EnemyParent.Difficulty"/> is in valid range and the corresponding enemy list is not null.</returns>
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

    /// <summary>
    /// Gets all <see cref="EnemySetup"/> objects.
    /// </summary>
    /// <param name="enemyDirector">The <see cref="EnemyDirector"/> whose enemies to get.</param>
    /// <returns>A list of all <see cref="EnemySetup"/> objects.</returns>
    public static List<EnemySetup> GetEnemies(this EnemyDirector enemyDirector)
    {
        return [
            .. enemyDirector.enemiesDifficulty1,
            .. enemyDirector.enemiesDifficulty2,
            .. enemyDirector.enemiesDifficulty3,
        ];
    }

    /// <summary>
    /// Tries to get an <see cref="EnemySetup"/> by name or by its <see cref="EnemyParent"/> name.
    /// </summary>
    /// <param name="enemyDirector">The <see cref="EnemyDirector"/> from which to check.</param>
    /// <param name="name">The <see cref="string"/> to match.</param>
    /// <param name="enemySetup">The found <see cref="EnemySetup"/>.</param>
    /// <returns>Whether or not the <see cref="EnemySetup"/> was found.</returns>
    public static bool TryGetEnemyByName(this EnemyDirector enemyDirector, string name, [NotNullWhen(true)] out EnemySetup? enemySetup)
    {
        enemySetup = enemyDirector.GetEnemyByName(name);
        return enemySetup != null;
    }

    /// <summary>
    /// Gets an <see cref="EnemySetup"/> by name or by its <see cref="EnemyParent"/> name.
    /// </summary>
    /// <param name="enemyDirector">The <see cref="EnemyDirector"/> from which to check.</param>
    /// <param name="name">The <see cref="string"/> to match.</param>
    /// <returns>The <see cref="EnemySetup"/> or null.</returns>
    public static EnemySetup? GetEnemyByName(this EnemyDirector enemyDirector, string name)
    {
        return enemyDirector.GetEnemies()
            .FirstOrDefault(x => x.NameEquals(name));
    }

    /// <summary>
    /// Tries to get an <see cref="EnemySetup"/> that contains the name or by its <see cref="EnemyParent"/> that contains the name.
    /// </summary>
    /// <param name="enemyDirector">The <see cref="EnemyDirector"/> from which to check.</param>
    /// <param name="name">The <see cref="string"/> to compare.</param>
    /// <param name="enemySetup">The found <see cref="EnemySetup"/>.</param>
    /// <returns>Whether or not the <see cref="EnemySetup"/> was found.</returns>
    public static bool TryGetEnemyThatContainsName(this EnemyDirector enemyDirector, string name, out EnemySetup enemySetup)
    {
        enemySetup = enemyDirector.GetEnemyThatContainsName(name);
        return enemySetup != null;
    }

    /// <summary>
    /// Gets an <see cref="EnemySetup"/> that contains the name or by its <see cref="EnemyParent"/> that contains the name.
    /// </summary>
    /// <param name="enemyDirector">The <see cref="EnemyDirector"/> from which to check.</param>
    /// <param name="name">The <see cref="string"/> to compare.</param>
    /// <returns>The <see cref="EnemySetup"/> or null.</returns>
    public static EnemySetup GetEnemyThatContainsName(this EnemyDirector enemyDirector, string name)
    {
        return enemyDirector.GetEnemies()
            .FirstOrDefault(x => x.NameContains(name));
    }
}