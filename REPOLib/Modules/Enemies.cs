using REPOLib.Extensions;
using System;
using System.Collections.Generic;

namespace REPOLib.Modules;

public static class Enemies 
{
    public static IReadOnlyList<EnemySetup> RegisteredEnemies => _enemiesRegistered;

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
        if (enemySetup == null || enemySetup.spawnObjects.Count == 0 || enemySetup.spawnObjects[0] == null)
        {
            throw new ArgumentException("Failed to register enemy. EnemySetup or spawnObjects list is empty.");
        }

        if (!_canRegisterEnemies)
        {
            Logger.LogError($"Failed to register enemy \"{enemySetup.spawnObjects[0].name}\". You can only register enemies in awake!");
        }

        if (_enemiesToRegister.Contains(enemySetup))
        {
            Logger.LogError($"Failed to register enemy \"{enemySetup.spawnObjects[0].name}\". Enemy is already registered!");
            return;
        }

        // Register all spawn prefabs to the network
        foreach (var spawnObject in enemySetup.spawnObjects)
        {
            NetworkPrefabs.RegisterNetworkPrefab(spawnObject.name, spawnObject);
        }
        
        _enemiesToRegister.Add(enemySetup);
    }
}