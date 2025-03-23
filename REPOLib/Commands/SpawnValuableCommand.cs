using Photon.Pun;
using REPOLib.Extensions;
using REPOLib.Modules;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace REPOLib.Commands;

internal static class SpawnValuableCommand
{
    private static readonly Dictionary<string, GameObject> _valuablePrefabs = [];

    private static bool _initialized = false;

    [CommandInitializer]
    public static void Initialize()
    {
        Logger.LogInfo("Initializing spawn valuable command");

        CacheValuables();

        _initialized = true;
    }

    public static void CacheValuables()
    {
        _valuablePrefabs.Clear();

        if (RunManager.instance == null)
        {
            Logger.LogError($"Failed to cache LevelValuables. RunManager instance is null.");
            return;
        }

        foreach (var level in RunManager.instance.levels)
        {
            foreach (var valuablePreset in level.ValuablePresets)
            {
                if (valuablePreset.TryGetCombinedList(out var valuables))
                {
                    foreach (var valuable in valuables)
                    {
                        _valuablePrefabs.TryAdd(valuable.name.ToLower(), valuable);
                    }
                }
            }
        }
    }

    [CommandExecution(
        "Spawn Valuable",
        "Spawn an instance of a valuable with the specified (case-insensitive) name. You can optionally leave out \"Valuable \" from the prefab name.",
        requiresDeveloperMode: true
    )]
    [CommandAlias("spawnvaluable")]
    [CommandAlias("spawnval")]
    [CommandAlias("sv")]
    public static void Execute(string args)
    {
        Logger.LogInfo($"Running spawn command with args \"{args}\"", extended: true);

        if (string.IsNullOrWhiteSpace(args))
        {
            Logger.LogWarning("No args provided to spawn command.");
            return;
        }

        if (!_initialized)
        {
            Logger.LogError("Spawn command not initialized!");
            return;
        }

        if (!SemiFunc.IsMasterClientOrSingleplayer())
        {
            Logger.LogError("Only the host can spawn valuables!");
            return;
        }

        if (PlayerAvatar.instance == null)
        {
            Logger.LogWarning("Can't spawn anything, player avatar is not initialized.");
            return;
        }

        if (ValuableDirector.instance == null)
        {
            Logger.LogError("ValuableDirector not initialized.");
            return;
        }

        //TODO: Add checks to restrict command to gameplay and not lobby

        SpawnValuableByName(args);
    }

    private static void SpawnValuableByName(string name)
    {
        Vector3 spawnPos = PlayerAvatar.instance.transform.position + PlayerAvatar.instance.transform.forward * 1f;
        
        Logger.LogInfo($"Trying to spawn \"{name}\" at {spawnPos}...", extended: true);

        if (!TryGetValuableByName(name, out GameObject valuablePrefab))
        {
            Logger.LogWarning($"Spawn command failed. Unknown valuable with name \"{name}\"");
            return;
        }

        try
        {
            if (SemiFunc.IsMultiplayer())
            {
                string valuablePath = ResourcesHelper.GetValuablePrefabPath(valuablePrefab);

                if (valuablePath == string.Empty)
                {
                    Logger.LogError($"Failed to get the path of valuable \"{valuablePrefab.name}\"");
                    return;
                }

                Logger.LogInfo($"Network spawning \"{valuablePath}\" at {spawnPos}.");
                PhotonNetwork.InstantiateRoomObject(valuablePath, spawnPos, Quaternion.identity);
            }
            else
            {
                Logger.LogInfo($"Locally spawning {valuablePrefab.name} at {spawnPos}.");
                UnityEngine.Object.Instantiate(valuablePrefab, spawnPos, Quaternion.identity);
            }
        }
        catch (Exception e)
        {
            Logger.LogError($"Failed to spawn \"{name}\":\n{e}");
        }
    }

    private static bool TryGetValuableByName(string name, out GameObject prefab)
    {
        prefab = null;

        foreach (var key in _valuablePrefabs.Keys)
        {
            if (key.Contains(name, StringComparison.OrdinalIgnoreCase))
            {
                prefab = _valuablePrefabs[key];
                return true;
            }
        }

        return false;
    }
}
