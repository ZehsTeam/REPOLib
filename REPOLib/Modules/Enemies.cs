using REPOLib.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace REPOLib.Modules;

public static class Enemies 
{
    public static IReadOnlyList<EnemySetup> AllEnemies => GetEnemies();
    public static IReadOnlyList<EnemySetup> RegisteredEnemies => _enemiesRegistered;

    internal static int SpawnNextEnemiesNotDespawned = 0;

    private static readonly List<EnemySetup> _enemiesToRegister = [];
    private static readonly List<EnemySetup> _enemiesRegistered = [];
    
    private static bool _canRegisterEnemies = true;

    internal static void RegisterEnemies()
    {
        if (!_canRegisterEnemies)
        {
            return;
        }
        
        foreach (var enemy in _enemiesToRegister)
        {
            if (_enemiesRegistered.Contains(enemy))
            {
                continue;
            }

            if (!enemy.spawnObjects[0].TryGetComponent(out EnemyParent enemyParent))
            {
                continue;
            }

            if (EnemyDirector.instance.AddEnemy(enemy))
            {
                _enemiesRegistered.Add(enemy);
                Logger.LogInfo($"Added enemy \"{enemy.spawnObjects[0].name}\" to difficulty {enemyParent.difficulty.ToString()}", extended: true);
            }
            else
            {
                Logger.LogWarning($"Failed to add enemy \"{enemy.spawnObjects[0].name}\" to difficulty {enemyParent.difficulty.ToString()}", extended: true);
            }
        }
        
        _enemiesToRegister.Clear();
        _canRegisterEnemies = false;
    }

    public static void RegisterEnemy(EnemySetup enemySetup)
    {
        if (enemySetup == null || enemySetup.spawnObjects == null || enemySetup.spawnObjects.Count == 0)
        {
            throw new ArgumentException("Failed to register enemy. EnemySetup or spawnObjects list is empty.");
        }

        EnemyParent enemyParent = enemySetup.GetEnemyParent();

        if (enemyParent == null)
        {
            Logger.LogError($"Failed to register enemy \"{enemySetup.name}\". No enemy prefab found in spawnObjects list.");
            return;
        }

        if (!_canRegisterEnemies)
        {
            Logger.LogError($"Failed to register enemy \"{enemyParent.enemyName}\". You can only register enemies in awake!");
        }

        if (ResourcesHelper.HasEnemyPrefab(enemySetup))
        {
            Logger.LogError($"Failed to register enemy \"{enemyParent.enemyName}\". Enemy prefab already exists in Resources with the same name.");
            return;
        }

        if (_enemiesToRegister.Contains(enemySetup))
        {
            Logger.LogError($"Failed to register enemy \"{enemyParent.enemyName}\". Enemy is already registered!");
            return;
        }

        foreach (var spawnObject in enemySetup.GetDistinctSpawnObjects())
        {
            foreach (var previousEnemy in _enemiesToRegister)
            {
                if (previousEnemy.AnySpawnObjectsNameEquals(spawnObject.name))
                {
                    Logger.LogError($"Failed to register enemy \"{enemyParent.enemyName}\". Enemy \"{previousEnemy.name}\" already has a spawn object called \"{spawnObject.name}\"");
                    return;
                }
            }
        }
        
        // Register all spawn prefabs to the network
        foreach (var spawnObject in enemySetup.GetDistinctSpawnObjects())
        {
            string prefabId = ResourcesHelper.GetEnemyPrefabPath(spawnObject);
            NetworkPrefabs.RegisterNetworkPrefab(prefabId, spawnObject);

            Utilities.FixAudioMixerGroups(spawnObject);
        }
        
        _enemiesToRegister.Add(enemySetup);
    }

    public static List<EnemyParent> SpawnEnemy(EnemySetup enemySetup, Vector3 position, Quaternion rotation, bool spawnDespawned = true)
    {
        if (enemySetup == null)
        {
            Logger.LogError("Failed to spawn enemy. EnemySetup is null.");
            return null;
        }

        if (!enemySetup.TryGetEnemyParent(out EnemyParent prefabEnemyParent))
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

        foreach (GameObject spawnObject in enemySetup.GetSortedSpawnObjects())
        {
            if (spawnObject == null)
            {
                Logger.LogError($"Failed to spawn enemy \"{prefabEnemyParent.enemyName}\" spawn object. GameObject is null.");
                continue;
            }

            string prefabId = ResourcesHelper.GetEnemyPrefabPath(spawnObject);
            GameObject gameObject = NetworkPrefabs.SpawnNetworkPrefab(prefabId, position, rotation);

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

            SpawnNextEnemiesNotDespawned++;

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
            Logger.LogInfo($"Failed to spawn enemy \"{prefabEnemyParent.enemyName}\". No spawn objects where spawned.", extended: true);
            return enemyParents;
        }

        Logger.LogInfo($"Spawned enemy \"{prefabEnemyParent.enemyName}\" at position {position}", extended: true);

        RunManager.instance.EnemiesSpawnedRemoveEnd();

        return enemyParents;
    }

    public static IReadOnlyList<EnemySetup> GetEnemies()
    {
        if (EnemyDirector.instance == null)
        {
            return [];
        }

        return EnemyDirector.instance.GetEnemies();
    }

    public static bool TryGetEnemyByName(string name, out EnemySetup enemySetup)
    {
        enemySetup = GetEnemyByName(name);
        return enemySetup != null;
    }

    public static EnemySetup GetEnemyByName(string name)
    {
        return EnemyDirector.instance?.GetEnemyByName(name);
    }

    public static bool TryGetEnemyThatContainsName(string name, out EnemySetup enemySetup)
    {
        enemySetup = GetEnemyThatContainsName(name);
        return enemySetup != null;
    }

    public static EnemySetup GetEnemyThatContainsName(string name)
    {
        return EnemyDirector.instance?.GetEnemyThatContainsName(name);
    }
}