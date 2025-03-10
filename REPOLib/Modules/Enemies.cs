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
        if (enemySetup == null || enemySetup.spawnObjects == null || enemySetup.spawnObjects.Count == 0)
        {
            throw new ArgumentException("Failed to register enemy. EnemySetup or spawnObjects list is empty.");
        }

        EnemyParent enemyParent = null;

        foreach (var spawnObject in enemySetup.spawnObjects)
        {
            if (spawnObject.TryGetComponent(out enemyParent))
            {
                break;
            }
        }

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

        // Register all spawn prefabs to the network
        foreach (var spawnObject in enemySetup.spawnObjects)
        {
            if (spawnObject == null)
            {
                Logger.LogWarning($"Enemy \"{enemyParent.enemyName}\" has a null entry in the spawnObjects list.");
                continue;
            }

            string prefabId = ResourcesHelper.GetEnemyPrefabPath(spawnObject);
            NetworkPrefabs.RegisterNetworkPrefab(prefabId, spawnObject);
        }
        
        _enemiesToRegister.Add(enemySetup);
    }
}