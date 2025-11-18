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
            presetNames = [ValuablePresets.GenericValuablePresetName];
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

    /// <summary>
    /// Registers a <see cref="ValuableObject"/>.
    /// </summary>
    /// <param name="valuableObject">The <see cref="ValuableObject"/> to register.</param>
    /// <param name="presetNames">The list of preset names for this <see cref="ValuableObject"/>.</param>
    /// <returns>The registered valuable <see cref="PrefabRef"/> or null.</returns>
    public static PrefabRef? RegisterValuable(ValuableObject? valuableObject, List<string> presetNames)
    {
        if (valuableObject == null)
        {
            Logger.LogError($"Failed to register valuable. ValuableObject is null.");
            return null;
        }

        GameObject prefab = valuableObject.gameObject;
        string prefabId = $"Valuables/{prefab.name}";

        if (presetNames == null || presetNames.Count == 0)
        {
            Logger.LogWarning($"Valuable \"{prefab.name}\" does not have any valid valuable preset names set. Adding generic valuable preset name.", extended: true);
            presetNames = [ValuablePresets.GenericValuablePresetName];
        }

        PrefabRefResponse prefabRefResponse = NetworkPrefabs.RegisterNetworkPrefabInternal(prefabId, prefab);
        PrefabRef? prefabRef = prefabRefResponse.PrefabRef;

        if (prefabRefResponse.Result == PrefabRefResult.PrefabAlreadyRegistered)
        {
            Logger.LogWarning($"Failed to register valuable \"{prefab.name}\". Valuable is already registered!");
            return null;
        }

        if (prefabRefResponse.Result == PrefabRefResult.DifferentPrefabAlreadyRegistered)
        {
            Logger.LogError($"Failed to register valuable \"{prefab.name}\". A valuable prefab is already registered with the same name.");
            return null;
        }

        if (prefabRefResponse.Result != PrefabRefResult.Success)
        {
            Logger.LogError($"Failed to register valuable \"{prefab.name}\". (Reason: {prefabRefResponse.Result})");
            return null;
        }

        if (prefabRef == null)
        {
            Logger.LogError($"Failed to register valuable \"{prefab.name}\". PrefabRef is null.");
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

    #region RegisterValuable overloads
    /// <inheritdoc cref="RegisterValuable(ValuableObject, List{string})"/>
    public static PrefabRef? RegisterValuable(ValuableObject? valuableObject)
    {
        return RegisterValuable(valuableObject, new List<string>());
    }

    /// <inheritdoc cref="RegisterValuable(ValuableObject, List{string})"/>
    /// <param name="presets">The list of presets for this <see cref="ValuableObject"/>.</param>
    /// <param name="valuableObject">The <see cref="ValuableObject"/> to register.</param>
    public static PrefabRef? RegisterValuable(ValuableObject? valuableObject, List<LevelValuables> presets)
    {
        return RegisterValuable(valuableObject, (from preset in presets select preset.name).ToList());
    }

    /// <inheritdoc cref="RegisterValuable(ValuableObject, List{string})"/>
    /// <param name="valuableContent">The <see cref="ValuableContent"/> to register.</param>
    public static PrefabRef? RegisterValuable(ValuableContent? valuableContent)
    {
        if (valuableContent == null)
        {
            Logger.LogError($"Failed to register valuable. ValuableContent is null.");
            return null;
        }

        return RegisterValuable(valuableContent.Prefab, valuableContent.ValuablePresets.ToList());
    }

    /// <inheritdoc cref="RegisterValuable(ValuableObject, List{string})"/>
    /// <param name="prefab">The <see cref="GameObject"/> whose <see cref="ValuableObject"/> to register.</param>
    /// <param name="presetNames"></param>
    public static PrefabRef? RegisterValuable(GameObject? prefab, List<string> presetNames)
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

    /// <inheritdoc cref="RegisterValuable(GameObject, List{string})"/>
    /// <param name="presets">The list of presets for this <see cref="ValuableObject"/>.</param>
    /// <param name="prefab">The <see cref="GameObject"/> whose <see cref="ValuableObject"/> to register.</param>
    public static PrefabRef? RegisterValuable(GameObject? prefab, List<LevelValuables> presets)
    {
        return RegisterValuable(prefab, (from preset in presets select preset.name).ToList());
    }

    /// <inheritdoc cref="RegisterValuable(GameObject, List{string})"/>
    public static PrefabRef? RegisterValuable(GameObject? prefab)
    {
        return RegisterValuable(prefab, new List<string>());
    }
    #endregion

    /// <summary>
    /// Spawns a <see cref="ValuableObject"/>.
    /// </summary>
    /// <param name="prefabRef">The <see cref="PrefabRef"/> for the <see cref="ValuableObject"/> to spawn.</param>
    /// <param name="position">The position where the valuable will be spawned.</param>
    /// <param name="rotation">The rotation of the valuable.</param>
    /// <returns>The <see cref="ValuableObject"/> object that was spawned.</returns>
    public static GameObject? SpawnValuable(PrefabRef? prefabRef, Vector3 position, Quaternion rotation)
    {
        if (prefabRef == null)
        {
            Logger.LogError("Failed to spawn valuable. PrefabRef is null.");
            return null;
        }

        if (!prefabRef.IsValid())
        {
            Logger.LogError("Failed to spawn valuable. PrefabRef is not valid.");
            return null;
        }

        if (!SemiFunc.IsMasterClientOrSingleplayer())
        {
            Logger.LogError($"Failed to spawn valuable \"{prefabRef.PrefabName}\". You are not the host.");
            return null;
        }

        GameObject? gameObject = NetworkPrefabs.SpawnNetworkPrefab(prefabRef, position, rotation);

        if (gameObject == null)
        {
            Logger.LogError($"Failed to spawn valuable \"{prefabRef.prefabName}\". Spawned GameObject is null.");
            return null;
        }

        Logger.LogInfo($"Spawned valuable \"{prefabRef.prefabName}\" at position {position}, rotation: {rotation.eulerAngles}", extended: true);

        return gameObject;
    }

    #region Deprecated
    #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    [Obsolete("This is no longer supported. Use AllValuables or RegisteredValuables instead.", error: true)]
    public static IReadOnlyList<GameObject> GetValuables()
    {
        Logger.LogError($"This method is deprecated! {Utilities.GetStackTrace()}");
        return [];
    }

    [Obsolete("This is no longer supported. Use AllValuables or RegisteredValuables instead.", error: true)]
    public static bool TryGetValuableByName(string name, [NotNullWhen(true)] out ValuableObject? prefabRef)
    {
        Logger.LogError($"This method is deprecated! {Utilities.GetStackTrace()}");
        prefabRef = null;
        return false;
    }

    [Obsolete("This is no longer supported. Use AllValuables or RegisteredValuables instead.", error: false)]
    public static ValuableObject? GetValuableByName(string name)
    {
        Logger.LogError($"This method is deprecated! {Utilities.GetStackTrace()}");
        return null;
    }

    [Obsolete("This is no longer supported. Use AllValuables or RegisteredValuables instead.", error: true)]
    public static bool TryGetValuableThatContainsName(string name, [NotNullWhen(true)] out ValuableObject? prefabRef)
    {
        Logger.LogError($"This method is deprecated! {Utilities.GetStackTrace()}");
        prefabRef = null;
        return false;
    }

    [Obsolete("This is no longer supported. Use AllValuables or RegisteredValuables instead.", error: true)]
    public static ValuableObject? GetValuableThatContainsName(string name)
    {
        Logger.LogError($"This method is deprecated! {Utilities.GetStackTrace()}");
        return null;
    }
    #pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    #endregion
}
