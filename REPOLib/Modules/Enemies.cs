using REPOLib.Extensions;
using REPOLib.Objects;
using REPOLib.Objects.Sdk;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;

namespace REPOLib.Modules;

/// <summary>
/// The Enemies module of REPOLib.
/// </summary>
public static class Enemies
{
    /// <summary>
    /// Gets all <see cref="EnemySetup"/> objects.
    /// </summary>
    /// <returns>A list of all <see cref="EnemySetup"/> objects.</returns>
    public static IReadOnlyList<EnemySetup> AllEnemies => EnemyDirector.instance?.GetEnemies() ?? [];

    /// <summary>
    /// Gets all enemies registered with REPOLib.
    /// </summary>
    public static IReadOnlyList<EnemySetup> RegisteredEnemies => _enemiesRegistered;

    internal static int SpawnNextEnemiesNotDespawned = 0;

    private static readonly List<EnemySetup> _enemiesToRegister = [];
    private static readonly List<EnemySetup> _enemiesRegistered = [];

    private static bool _initialEnemiesRegistered;

    internal static void RegisterEnemies()
    {
        foreach (var enemy in _enemiesToRegister)
        {
            RegisterEnemyWithGame(enemy);
        }

        _initialEnemiesRegistered = true;
    }

    private static void RegisterEnemyWithGame(EnemySetup enemySetup)
    {
        if (_enemiesRegistered.Contains(enemySetup))
        {
            return;
        }

        if (!enemySetup.TryGetEnemyParent(out EnemyParent? enemyParent))
        {
            return;
        }

        if (EnemyDirector.instance.AddEnemy(enemySetup))
        {
            _enemiesRegistered.Add(enemySetup);
            Logger.LogInfo($"Added enemy \"{enemyParent.enemyName}\" to difficulty {enemyParent.difficulty}", extended: true);
        }
        else
        {
            Logger.LogWarning($"Failed to add enemy \"{enemyParent.enemyName}\" to difficulty {enemyParent.difficulty}", extended: true);
        }
    }

    /// <summary>
    /// Registers an <see cref="EnemySetup"/>.
    /// </summary>
    /// <param name="enemyContent">The <see cref="EnemySetup"/> to register.</param>
    public static void RegisterEnemy(EnemyContent? enemyContent)
    {
        if (enemyContent == null)
        {
            Logger.LogError($"Failed to register enemy. EnemyContent is null.");
            return;
        }

        EnemySetup? enemySetup = enemyContent.Setup;
        List<GameObject> spawnObjects = enemyContent.SpawnObjects.Where(x => x != null).ToList();

        if (enemySetup == null)
        {
            Logger.LogError($"Failed to register enemy. EnemySetup is null.");
            return;
        }

        if (spawnObjects == null || spawnObjects.Count == 0)
        {
            Logger.LogError($"Failed to register enemy \"{enemyContent.Name}\". No spawn objects found.");
            return;
        }

        if (_enemiesToRegister.Contains(enemySetup))
        {
            Logger.LogError($"Failed to register enemy \"{enemyContent.Name}\". Enemy is already registered!");
            return;
        }

        if (_enemiesToRegister.Any(x => x.name == enemySetup.name))
        {
            Logger.LogError($"Failed to register enemy \"{enemyContent.Name}\". Enemy already exists with the same name.");
            return;
        }

        Dictionary<GameObject, PrefabRef> spawnObjectRefs = [];

        List<GameObject> DistinctSpawnObjects = spawnObjects.Distinct(new UnityObjectNameComparer<GameObject>()).ToList();

        foreach (var spawnObject in spawnObjects)
        {
            if (spawnObject == null) continue;

            string prefabId = $"Enemies/{spawnObject.name}";

            PrefabRef? prefabRef = null;
            PrefabRef? existingPrefabRef = NetworkPrefabs.GetNetworkPrefabRef(prefabId);

            if (existingPrefabRef == null)
            {
                prefabRef = NetworkPrefabs.RegisterNetworkPrefab(prefabId, spawnObject);

                if (prefabRef == null)
                {
                    Logger.LogError($"Failed to register enemy \"{enemyContent.Name}\". Could not register spawn object \"{spawnObject.name}\".");
                    return;
                }

                Utilities.FixAudioMixerGroups(spawnObject);
            }
            else
            {
                GameObject existingSpawnObject = existingPrefabRef.Prefab;

                if (spawnObject != existingSpawnObject)
                {
                    Logger.LogError($"Failed to register enemy \"{enemyContent.Name}\". Spawn object already exists with the name \"{existingSpawnObject.name}\"");
                    return;
                }

                prefabRef = existingPrefabRef;
            }

            spawnObjectRefs[spawnObject] = prefabRef;
        }

        enemySetup.spawnObjects.Clear();

        foreach (var spawnObject in spawnObjects)
        {
            if (!spawnObjectRefs.TryGetValue(spawnObject, out PrefabRef prefabRef))
            {
                continue;
            }

            enemySetup.spawnObjects.Add(prefabRef);
        }

        if (_initialEnemiesRegistered)
        {
            RegisterEnemyWithGame(enemySetup);
        }
        else
        {
            _enemiesToRegister.Add(enemySetup);
        }
    }

    /// <summary>
    /// Spawns an enemy or enemies from a <see cref="EnemySetup"/>.
    /// </summary>
    /// <param name="enemySetup">The <see cref="EnemySetup"/> to spawn the enemy or enemies from.</param>
    /// <param name="position">The position where the enemy will be spawned.</param>
    /// <param name="rotation">The rotation of the enemy.</param>
    /// <param name="spawnDespawned">Whether or not this enemy will spawn despawned.</param>
    /// <returns>The <see cref="EnemyParent"/> objects from spawned enemies.</returns>
    public static List<EnemyParent>? SpawnEnemy(EnemySetup? enemySetup, Vector3 position, Quaternion rotation, bool spawnDespawned = true)
    {
        if (enemySetup == null)
        {
            Logger.LogError("Failed to spawn enemy. EnemySetup is null.");
            return null;
        }

        if (!enemySetup.TryGetEnemyParent(out EnemyParent? prefabEnemyParent))
        {
            Logger.LogError("Failed to spawn enemy. EnemyParent is null.");
            return null;
        }

        if (LevelGenerator.Instance == null)
        {
            Logger.LogError($"Failed to spawn enemy \"{prefabEnemyParent.enemyName}\". EnemySetup instance is null.");
            return null;
        }

        if (RunManager.instance == null)
        {
            Logger.LogError($"Failed to spawn enemy \"{prefabEnemyParent.enemyName}\". RunManager instance is null.");
            return null;
        }

        if (EnemyDirector.instance == null)
        {
            Logger.LogError($"Failed to spawn enemy \"{prefabEnemyParent.enemyName}\". EnemyDirector instance is null.");
            return null;
        }

        List<EnemyParent> enemyParents = [];

        foreach (PrefabRef prefabRef in enemySetup.spawnObjects)
        {
            GameObject spawnObject = prefabRef.Prefab;

            if (spawnObject == null)
            {
                Logger.LogError($"Failed to spawn enemy \"{prefabEnemyParent.enemyName}\" spawn object. GameObject is null.");
                continue;
            }

            GameObject? gameObject = NetworkPrefabs.SpawnNetworkPrefab(prefabRef, position, rotation);

            if (gameObject == null)
            {
                Logger.LogError($"Failed to spawn enemy \"{prefabEnemyParent.enemyName}\" spawn object \"{spawnObject.name}\"");
                continue;
            }

            if (!gameObject.TryGetComponent(out EnemyParent enemyParent))
            {
                continue;
            }

            enemyParents.Add(enemyParent);

            if (!spawnDespawned)
            {
                SpawnNextEnemiesNotDespawned++;
            }

            enemyParent.SetupDone = true;

            Enemy enemy = gameObject.GetComponentInChildren<Enemy>();

            if (enemy != null)
            {
                enemy.EnemyTeleported(position);
            }
            else
            {
                Logger.LogError($"Enemy \"{prefabEnemyParent.enemyName}\" spawn object \"{spawnObject.name}\" does not have an enemy component.");
            }

            LevelGenerator.Instance.EnemiesSpawnTarget++;
            EnemyDirector.instance.FirstSpawnPointAdd(enemyParent);
        }

        if (enemyParents.Count == 0)
        {
            Logger.LogInfo($"Failed to spawn enemy \"{prefabEnemyParent.enemyName}\". No spawn objects where spawned.");
            return enemyParents;
        }

        Logger.LogInfo($"Spawned enemy \"{prefabEnemyParent.enemyName}\" at position {position}", extended: true);

        RunManager.instance.EnemiesSpawnedRemoveEnd();

        return enemyParents;
    }

    #region Deprecated
    #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    [Obsolete("This is no longer supported. Use AllEnemies or RegisteredEnemies instead.", error: true)]
    public static IReadOnlyList<EnemySetup> GetEnemies()
    {
        return AllEnemies;
    }

    [Obsolete("This is no longer supported. Use AllEnemies or RegisteredEnemies instead.", error: true)]
    public static bool TryGetEnemyByName(string name, [NotNullWhen(true)] out EnemySetup? enemySetup)
    {
        enemySetup = null;
        return false;
    }

    [Obsolete("This is no longer supported. Use AllEnemies or RegisteredEnemies instead.", error: true)]
    public static EnemySetup? GetEnemyByName(string name)
    {
        return null;
    }

    [Obsolete("This is no longer supported. Use AllEnemies or RegisteredEnemies instead.", error: true)]
    public static bool TryGetEnemyThatContainsName(string name, [NotNullWhen(true)] out EnemySetup? enemySetup)
    {
        enemySetup = null;
        return false;
    }

    [Obsolete("This is no longer supported. Use AllEnemies or RegisteredEnemies instead.", error: true)]
    public static EnemySetup? GetEnemyThatContainsName(string name)
    {
        return null;
    }
    #pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    #endregion
}