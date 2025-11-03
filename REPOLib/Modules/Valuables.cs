using REPOLib.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;

namespace REPOLib.Modules;

/// <summary>
/// The Valuables module of REPOLib.
/// </summary>
public static class Valuables
{
    /// <summary>
    /// Gets all valuables <see cref="PrefabRef"/>s.
    /// </summary>
    /// <returns>All valuables.</returns>
    public static IReadOnlyList<PrefabRef> AllValuables
    {
        get
        {
            if (RunManager.instance == null)
            {
                return [];
            }

            return ValuablePresets.AllValuablePresets.Values
                .Select(levelValuables => levelValuables.GetCombinedList())
                .SelectMany(list => list)
                .Distinct()
                .ToList();
        }
    }

    /// <summary>
    /// Gets all valuables <see cref="PrefabRef"/>s registered with REPOLib.
    /// </summary>
    public static IReadOnlyList<PrefabRef> RegisteredValuables => _valuablesRegistered;

    private static readonly Dictionary<PrefabRef, List<string>> _valuablesToRegister = [];
    private static readonly List<PrefabRef> _valuablesRegistered = [];

    private static bool _initialValuablesRegistered;

    internal static void RegisterInitialValuables()
    {
        if (_initialValuablesRegistered)
        {
            return;
        }

        Logger.LogInfo($"Adding valuables to valuable presets.");

        foreach (var valuable in _valuablesToRegister.Keys)
        {
            RegisterValuableWithGame(valuable);
        }

        _valuablesToRegister.Clear();
        _initialValuablesRegistered = true;
    }

    private static void RegisterValuableWithGame(PrefabRef prefabRef)
    {
        if (_valuablesRegistered.Contains(prefabRef))
        {
            return;
        }

        List<string> presetNames = _valuablesToRegister[prefabRef];

        if (!presetNames.Any(x => ValuablePresets.AllValuablePresets.Keys.Any(y => x == y)))
        {
            Logger.LogWarning($"Valuable \"{prefabRef.PrefabName}\" does not have any valid valuable preset names set. Adding generic valuable preset name.");
            presetNames = ValuablePresets.AllValuablePresetNames;
        }

        foreach (var presetName in presetNames)
        {
            if (presetName == null || !ValuablePresets.AllValuablePresets.ContainsKey(presetName))
            {
                Logger.LogWarning($"Failed to add valuable \"{prefabRef.PrefabName}\" to valuable preset \"{presetName}\". The valuable preset does not exist.");
                continue;
            }

            if (ValuablePresets.AllValuablePresets[presetName].AddValuable(prefabRef))
            {
                _valuablesRegistered.Add(prefabRef);
                Logger.LogDebug($"Added valuable \"{prefabRef.PrefabName}\" to valuable preset \"{presetName}\"");
            }
            else
            {
                Logger.LogWarning($"Failed to add valuable \"{prefabRef.PrefabName}\" to valuable preset \"{presetName}\"", extended: true);
            }
        }
    }

    /// <inheritdoc cref="RegisterValuable(GameObject, List{string})"/>
    public static PrefabRef? RegisterValuable(GameObject prefab)
    {
        return RegisterValuable(prefab, new List<string>());
    }

    /// <param name="presets">The list of presets for this <see cref="ValuableObject"/>.</param>
    /// <inheritdoc cref="RegisterValuable(GameObject, List{string})"/>
    /// <param name="prefab">The <see cref="GameObject"/> whose <see cref="ValuableObject"/> to register.</param>
    public static PrefabRef? RegisterValuable(GameObject prefab, List<LevelValuables> presets)
    {
        return RegisterValuable(prefab, (from preset in presets select preset.name).ToList());
    }

    /// <param name="prefab">The <see cref="GameObject"/> whose <see cref="ValuableObject"/> to register.</param>
    /// <inheritdoc cref="RegisterValuable(ValuableObject, List{string})"/>
    /// <param name="presetNames"></param>
    public static PrefabRef? RegisterValuable(GameObject prefab, List<string> presetNames)
    {
        if (prefab == null)
        {
            Logger.LogError($"Failed to register valuable. Prefab is null.");
            return null;
        }

        if (!prefab.TryGetComponent(out ValuableObject valuableObject))
        {
            Logger.LogError($"Failed to register valuable \"{prefab.name}\". Prefab does not have a ValuableObject component.");
            return null;
        }

        return RegisterValuable(valuableObject, presetNames);
    }

    /// <inheritdoc cref="RegisterValuable(ValuableObject, List{string})"/>
    public static PrefabRef? RegisterValuable(ValuableObject valuableObject)
    {
        return RegisterValuable(valuableObject, new List<string>());
    }

    /// <inheritdoc cref="RegisterValuable(ValuableObject, List{string})"/>
    /// <param name="presets">The list of presets for this <see cref="ValuableObject"/>.</param>
    /// <param name="valuableObject">The <see cref="ValuableObject"/> to register.</param>
    public static PrefabRef? RegisterValuable(ValuableObject valuableObject, List<LevelValuables> presets)
    {
        return RegisterValuable(valuableObject, (from preset in presets select preset.name).ToList());
    }

    /// <summary>
    /// Registers a <see cref="ValuableObject"/>.
    /// </summary>
    /// <param name="valuableObject">The <see cref="ValuableObject"/> to register.</param>
    /// <param name="presetNames">The list of preset names for this <see cref="ValuableObject"/>.</param>
    /// <returns>The registered valuable <see cref="PrefabRef"/> or null.</returns>
    public static PrefabRef? RegisterValuable(ValuableObject valuableObject, List<string> presetNames)
    {
        if (valuableObject == null)
        {
            Logger.LogError($"Failed to register valuable. ValuableObject is null.");
            return null;
        }

        GameObject prefab = valuableObject.gameObject;
        string name = prefab.name;
        string prefabId = $"Valuables/{name}";

        if (presetNames == null || presetNames.Count == 0)
        {
            Logger.LogWarning($"Valuable \"{name}\" does not have any valid valuable preset names set. Adding generic valuable preset name.", extended: true);
            presetNames = ValuablePresets.AllValuablePresetNames;
        }

        PrefabRef? existingPrefabRef = NetworkPrefabs.GetNetworkPrefabRef(prefabId);

        if (existingPrefabRef != null)
        {
            if (prefab == existingPrefabRef.Prefab)
            {
                Logger.LogWarning($"Failed to register valuable \"{name}\". Valuable is already registered!");
            }
            else
            {
                Logger.LogError($"Failed to register valuable \"{name}\". Valuable prefab already exists with the same name.");
            }

            return null;
        }

        PrefabRef? prefabRef = NetworkPrefabs.RegisterNetworkPrefab(prefabId, prefab);

        if (prefabRef == null)
        {
            Logger.LogError($"Failed to register valuable \"{name}\". PrefabRef is null.");
            return null;
        }

        Utilities.FixAudioMixerGroups(prefab);

        _valuablesToRegister.Add(prefabRef, presetNames);

        if (_initialValuablesRegistered)
        {
            RegisterValuableWithGame(prefabRef);
        }

        return prefabRef;
    }

    #region Deprecated
    #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    [Obsolete("This is no longer supported. Use AllValuables or RegisteredValuables instead.", error: true)]
    public static bool TryGetValuableByName(string name, [NotNullWhen(true)] out PrefabRef? prefabRef)
    {
        prefabRef = null;
        return false;
    }

    #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    [Obsolete("This is no longer supported. Use AllValuables or RegisteredValuables instead.", error: true)]
    public static PrefabRef? GetValuableByName(string name)
    {
        return null;
    }

    #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    [Obsolete("This is no longer supported. Use AllValuables or RegisteredValuables instead.", error: true)]
    public static bool TryGetValuableThatContainsName(string name, [NotNullWhen(true)] out PrefabRef? prefabRef)
    {
        prefabRef = null;
        return false;
    }

    #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    [Obsolete("This is no longer supported. Use AllValuables or RegisteredValuables instead.", error: true)]
    public static PrefabRef? GetValuableThatContainsName(string name)
    {
        return null;
    }
    #endregion
}
