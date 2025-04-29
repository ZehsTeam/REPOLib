using REPOLib.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace REPOLib.Modules;

/// <summary>
/// The Valuables module of REPOLib.
/// </summary>
[PublicAPI]
public static class Valuables
{
    /// <inheritdoc cref="GetValuables"/>
    public static IReadOnlyList<GameObject> AllValuables => GetValuables();

    /// <summary>
    /// Gets all valuables registered with REPOLib.
    /// </summary>
    public static IReadOnlyList<GameObject> RegisteredValuables => ValuablesRegistered;
    private static IEnumerable<GameObject> PendingAndRegisteredValuables => ValuablesToRegister.Keys.Concat(ValuablesRegistered);
    private static readonly Dictionary<GameObject, List<string>> ValuablesToRegister = [];
    private static readonly List<GameObject> ValuablesRegistered = [];
    private static bool _initialValuablesRegistered;

    internal static void RegisterInitialValuables()
    {
        if (_initialValuablesRegistered)
            return;
        
        Logger.LogInfo($"Adding valuables to valuable presets.");
        foreach (var valuable in ValuablesToRegister.Keys)
            RegisterValuableWithGame(valuable);
        
        ValuablesToRegister.Clear();
        _initialValuablesRegistered = true;
    }

    private static void RegisterValuableWithGame(GameObject valuable)
    {
        if (ValuablesRegistered.Contains(valuable))
            return;

        var presetNames = ValuablesToRegister[valuable];
        if (!presetNames.Any(x => ValuablePresets.GetAllValuablePresets().Keys.Any(y => x == y)))
        {
            Logger.LogWarning($"Valuable \"{valuable.name}\" does not have any valid valuable preset names set. Adding generic valuable preset name.");
            presetNames.Add(ValuablePresets.GenericValuablePresetName);
        }

        foreach (var presetName in presetNames)
        {
            if (presetName == null || !ValuablePresets.GetAllValuablePresets().ContainsKey(presetName))
            {
                Logger.LogWarning($"Failed to add valuable \"{valuable.name}\" to valuable preset \"{presetName}\". The valuable preset does not exist.");
                continue;
            }

            if (ValuablePresets.GetAllValuablePresets()[presetName].AddValuable(valuable))
            {
                ValuablesRegistered.Add(valuable);
                Logger.LogDebug($"Added valuable \"{valuable.name}\" to valuable preset \"{presetName}\"");
            }
            else
            {
                Logger.LogWarning($"Failed to add valuable \"{valuable.name}\" to valuable preset \"{presetName}\"", extended: true);
            }
        }
    }

    /// <inheritdoc cref="RegisterValuable(GameObject, List{string})"/>
    public static void RegisterValuable(GameObject prefab)
        => RegisterValuable(prefab, new List<string>());

    /// <param name="presets">The list of presets for this <see cref="ValuableObject"/>.</param>
    /// <inheritdoc cref="RegisterValuable(GameObject, List{string})"/>
    /// <param name="prefab"></param>
    public static void RegisterValuable(GameObject prefab, List<LevelValuables> presets)
        => RegisterValuable(prefab, (from preset in presets select preset.name).ToList());

    /// <param name="prefab">The <see cref="GameObject"/> whose <see cref="ValuableObject"/> to register.</param>
    /// <inheritdoc cref="RegisterValuable(ValuableObject, List{string})"/>
    /// <param name="presetNames"></param>
    public static void RegisterValuable(GameObject prefab, List<string>? presetNames)
    {
        if (prefab == null)
        {
            Logger.LogError($"Failed to register valuable. Prefab is null.");
            return;
        }

        if (!prefab.TryGetComponent(out ValuableObject valuableObject))
        {
            Logger.LogError($"Failed to register valuable. Prefab does not have a ValuableObject component.");
            return;
        }

        RegisterValuable(valuableObject, presetNames);
    }

    /// <inheritdoc cref="RegisterValuable(ValuableObject, List{string})"/>
    public static void RegisterValuable(ValuableObject valuableObject)
        => RegisterValuable(valuableObject, new List<string>());
    
    /// <param name="presets">The list of presets for this <see cref="ValuableObject"/>.</param>
    /// <inheritdoc cref="RegisterValuable(ValuableObject, List{string})"/>
    /// <param name="valuableObject"></param>
    public static void RegisterValuable(ValuableObject valuableObject, List<LevelValuables> presets)
        => RegisterValuable(valuableObject, (from preset in presets select preset.name).ToList());

    /// <summary>
    /// Registers a <see cref="ValuableObject"/>.
    /// </summary>
    /// <param name="valuableObject">The <see cref="ValuableObject"/> to register.</param>
    /// <param name="presetNames">The list of preset names for this <see cref="ValuableObject"/>.</param>
    public static void RegisterValuable(ValuableObject valuableObject, List<string>? presetNames)
    {
        if (valuableObject == null)
        {
            Logger.LogError($"Failed to register valuable. ValuableObject is null.");
            return;
        }

        var prefab = valuableObject.gameObject;
        if (presetNames == null || presetNames.Count == 0)
        {
            Logger.LogWarning($"Valuable \"{valuableObject.name}\" does not have any valid valuable preset names set. Adding generic valuable preset name.", extended: true);
            presetNames = [ValuablePresets.GenericValuablePresetName];
        }

        if (ResourcesHelper.HasValuablePrefab(valuableObject))
        {
            Logger.LogError($"Failed to register valuable \"{prefab.name}\". Valuable prefab already exists in Resources with the same name.");
            return;
        }

        if (PendingAndRegisteredValuables.Any(x => x.name.Equals(prefab.name, StringComparison.OrdinalIgnoreCase)))
        {
            Logger.LogError($"Failed to register valuable \"{prefab.name}\". Valuable prefab already exists with the same name.");
            return;
        }

        if (PendingAndRegisteredValuables.Contains(prefab))
        {
            Logger.LogWarning($"Failed to register valuable \"{prefab.name}\". Valuable is already registered!");
            return;
        }

        var prefabId = ResourcesHelper.GetValuablePrefabPath(valuableObject);
        NetworkPrefabs.RegisterNetworkPrefab(prefabId, prefab);
        Utilities.FixAudioMixerGroups(prefab);

        ValuablesToRegister.Add(prefab, presetNames);
        if (_initialValuablesRegistered)
            RegisterValuableWithGame(valuableObject.gameObject);
    }

    /// <summary>
    /// Spawns a <see cref="ValuableObject"/>.
    /// </summary>
    /// <param name="valuableObject">The <see cref="ValuableObject"/> to spawn.</param>
    /// <param name="position">The position where the valuable will be spawned.</param>
    /// <param name="rotation">The rotation of the valuable.</param>
    /// <returns>The <see cref="ValuableObject"/> object that was spawned.</returns>
    public static GameObject? SpawnValuable(ValuableObject valuableObject, Vector3 position, Quaternion rotation)
    {
        if (valuableObject == null)
        {
            Logger.LogError("Failed to spawn valuable. ValuableObject is null.");
            return null;
        }

        if (!SemiFunc.IsMasterClientOrSingleplayer())
        {
            Logger.LogError($"Failed to spawn valuable \"{valuableObject.gameObject.name}\". You are not the host.");
            return null;
        }

        var prefabId = ResourcesHelper.GetValuablePrefabPath(valuableObject);
        var gameObject = NetworkPrefabs.SpawnNetworkPrefab(prefabId, position, rotation);

        if (gameObject == null)
        {
            Logger.LogError($"Failed to spawn valuable \"{valuableObject.gameObject.name}\"");
            return null;
        }

        Logger.LogInfo($"Spawned valuable \"{gameObject.name}\" at position {position}, rotation: {rotation.eulerAngles}", extended: true);
        return gameObject;
    }

    /// <summary>
    /// Gets all valuables.
    /// </summary>
    /// <returns>All valuables.</returns>
    public static IReadOnlyList<GameObject> GetValuables()
    {
        if (RunManager.instance == null)
            return [];

        return ValuablePresets.GetAllValuablePresets().Values
            .Select(levelValuables => levelValuables.GetCombinedList())
            .SelectMany(list => list)
            .Distinct()
            .ToList();
    }

    /// <summary>
    /// Tries to get a <see cref="ValuableObject"/> by name.
    /// </summary>
    /// <param name="name">The <see cref="string"/> to match.</param>
    /// <param name="valuableObject">The found <see cref="ValuableObject"/>.</param>
    /// <returns>Whether the <see cref="ValuableObject"/> was found.</returns>
    public static bool TryGetValuableByName(string name, [NotNullWhen(true)] out ValuableObject? valuableObject)
        => (valuableObject = GetValuableByName(name)) != null;

    /// <summary>
    /// Gets a <see cref="ValuableObject"/> by name.
    /// </summary>
    /// <param name="name">The <see cref="string"/> to match.</param>
    /// <returns>The <see cref="ValuableObject"/> or null.</returns>
    public static ValuableObject? GetValuableByName(string name)
        => GetValuables()
            .FirstOrDefault(x => x.name.Equals(name, StringComparison.OrdinalIgnoreCase))?
            .GetComponent<ValuableObject>() ?? null;

    /// <summary>
    /// Tries to get a <see cref="ValuableObject"/> that contains the name.
    /// </summary>
    /// <param name="name">The <see cref="string"/> to compare.</param>
    /// <param name="valuableObject">The found <see cref="ValuableObject"/>.</param>
    /// <returns>Whether the <see cref="ValuableObject"/> was found.</returns>
    public static bool TryGetValuableThatContainsName(string name,
        [NotNullWhen(true)] out ValuableObject? valuableObject)
        => (valuableObject = GetValuableThatContainsName(name)) != null;

    /// <summary>
    /// Gets a <see cref="ValuableObject"/> that contains the name.
    /// </summary>
    /// <param name="name">The <see cref="string"/> to compare.</param>
    /// <returns>The <see cref="ValuableObject"/> or null.</returns>
    public static ValuableObject? GetValuableThatContainsName(string name)
        => GetValuables()
            .SortByStringLength(x => x.name, ListExtensions.StringSortMode.Shortest)
            .FirstOrDefault(x => x.name.Contains(name, StringComparison.OrdinalIgnoreCase))
            ?.GetComponent<ValuableObject>() ?? null;

    #region Deprecated
    /// <summary>Deprecated.</summary>
    [Obsolete("prefabId is no longer supported", true)]
    public static void RegisterValuable(string prefabId, GameObject prefab)
        => RegisterValuable(prefab);

    /// <summary>Deprecated.</summary>
    [Obsolete("prefabId is no longer supported", true)]
    public static void RegisterValuable(string prefabId, GameObject prefab, List<LevelValuables> presets)
        => RegisterValuable(prefab, presets);

    /// <summary>Deprecated.</summary>
    [Obsolete("prefabId is no longer supported", true)]
    public static void RegisterValuable(string prefabId, GameObject prefab, List<string>? presetNames)
        => RegisterValuable(prefab, presetNames);
    #endregion
}
