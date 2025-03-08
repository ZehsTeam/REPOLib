using System;
using System.Collections.Generic;
using REPOLib.Extensions;
using REPOLib.Objects;
using UnityEngine;

namespace REPOLib.Modules;

public static class Enemies 
{
    public static IReadOnlyList<EnemySetup> RegisteredEnemys => _enemysRegistered;

    private static readonly List<EnemySetup> _enemysToRegister = [];
    private static readonly List<EnemySetup> _enemysRegistered = [];
    
    private static bool _canRegisterEnemys = true;

    internal static void RegisterEnemies()
    {

        if (!_canRegisterEnemys)
        {
            return;
        }
        
        foreach (var enemy in _enemysToRegister)
        {
            if (_enemysRegistered.Contains(enemy))
            {
                continue;
            }

            if (!enemy.spawnObjects[0].TryGetComponent(out EnemyParent enemyParent))
            {
                continue;
            }

            if (EnemyDirector.instance.AddEnemy(enemy))
            {
                _enemysRegistered.Add(enemy);
                Logger.LogInfo($"Added {enemy.spawnObjects[0].name} to difficulty {enemyParent.difficulty.ToString()}", extended: true);
            }
            else
            {
                Logger.LogWarning($"Failed to add {enemy.spawnObjects[0].name} to difficulty {enemyParent.difficulty.ToString()}", extended: true);
            }
            
            _enemysToRegister.Clear();
            _canRegisterEnemys = false;

        }
    }

    public static void RegisterEnemy(EnemySetup enemySetup)
    {
        if (enemySetup == null || enemySetup.spawnObjects[0] == null || enemySetup.spawnObjects.Count == 0)
        {
            throw new ArgumentException("Failed to register enemy. EnemySetup or spawnObjects list is empty.");
        }

        if (_canRegisterEnemys)
        {
            Logger.LogError($"Failed to register enemy {enemySetup.spawnObjects[0].name}. You can only register enemys in awake!");
        }

        if (_enemysToRegister.Contains(enemySetup))
        {
            Logger.LogError($"Failed to register enemy. Enemy {enemySetup.spawnObjects[0].name} is already registered!");
            return;
        }

        //register all spawn prefabs to the network
        foreach (var spawnObject in enemySetup.spawnObjects)
        {
            NetworkPrefabs.RegisterNetworkPrefab(spawnObject.name, spawnObject);
        }
        
        _enemysToRegister.Add(enemySetup);
    }
}