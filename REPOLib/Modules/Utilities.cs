using REPOLib.Extensions;
using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace REPOLib.Modules;

/// <summary>
/// Utility methods from REPOLib.
/// </summary>
[PublicAPI]
public static class Utilities
{
    private static readonly List<GameObject> PrefabsToFix = [];
    private static readonly List<GameObject> FixedPrefabs = [];

    internal static void FixAudioMixerGroupsOnPrefabs()
    {
        foreach (var prefab in PrefabsToFix)
        {
            prefab.FixAudioMixerGroups();
            FixedPrefabs.Add(prefab);
        }

        PrefabsToFix.Clear();
    }

    /// <summary>
    /// Fixes the audio mixer groups of all audio sources in the game object and its children.
    /// Assigns the audio mixer group based on the audio group found assigned in the prefab.
    /// - Vyrus + Zehs
    /// </summary>
    public static void FixAudioMixerGroups(GameObject prefab)
    {
        if (prefab == null || PrefabsToFix.Contains(prefab) || FixedPrefabs.Contains(prefab))
            return;
        
        if (AudioManager.instance == null)
        {
            PrefabsToFix.Add(prefab);
            return;
        }

        prefab.FixAudioMixerGroups();
        FixedPrefabs.Add(prefab);
    }

    internal static void SafeInvokeEvent(Action? action)
    {
        try
        {
            action?.Invoke();
        }
        catch (Exception e)
        {
            Logger.LogError($"Exception occured while invoking event: {e}");
        }
    }
}