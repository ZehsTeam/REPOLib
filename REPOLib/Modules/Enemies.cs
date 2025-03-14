using REPOLib.Extensions;
using System;
using System.Collections.Generic;

namespace REPOLib.Modules;

public static class Enemies 
{
    public static IReadOnlyList<EnemySetup> RegisteredEnemies => _enemiesRegistered;

    private static readonly List<EnemySetup> _enemiesToRegister = [];
    private static readonly List<EnemySetup> _enemiesRegistered = [];
    
    private static bool _initialEnemiesRegistered = true;

    internal static void RegisterEnemies()
    {
        if (!_initialEnemiesRegistered)
        {
            return;
        }
        
        foreach (var enemy in _enemiesToRegister)
        {
            RegisterEnemyInternal(enemy);
        }
        
        _enemiesToRegister.Clear();
        _initialEnemiesRegistered = true;
    }

    private static void RegisterEnemyInternal(EnemySetup enemy)
    {
        if (_enemiesRegistered.Contains(enemy))
        {
            return;
        }

        if (!enemy.spawnObjects[0].TryGetComponent(out EnemyParent enemyParent))
        {
            return;
        }

        if (EnemyDirector.instance.AddEnemy(enemy))
        {
            _enemiesRegistered.Add(enemy);
            Logger.LogDebug($"Added enemy \"{enemy.spawnObjects[0].name}\" to difficulty {enemyParent.difficulty.ToString()}", extended: true);
        }
        else
        {
            Logger.LogWarning($"Failed to add enemy \"{enemy.spawnObjects[0].name}\" to difficulty {enemyParent.difficulty.ToString()}", extended: true);
        }
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

        if (!_initialEnemiesRegistered)
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
        }

        if (_initialEnemiesRegistered)
        {
            RegisterEnemyInternal(enemySetup);
        }
        else
        {
            _enemiesToRegister.Add(enemySetup);
        }
    }
}