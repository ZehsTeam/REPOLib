using Sirenix.Utilities;
using UnityEngine;

namespace REPOLib.Extensions;

internal static class GameObjectExtensions
{
    public static void FixAudioMixerGroups(this GameObject gameObject)
    {
        if (gameObject == null)
            return;

        if (AudioManager.instance == null)
        {
            Logger.LogWarning($"Failed to fix audio mixer groups on GameObject \"{gameObject.name}\". AudioManager instance is null.");
            return;
        }

        gameObject.GetComponentsInChildren<AudioSource>().ForEach(FixAudioMixerGroup);
    }

    private static void FixAudioMixerGroup(AudioSource audioSource)
        => audioSource.FixAudioMixerGroup();
}