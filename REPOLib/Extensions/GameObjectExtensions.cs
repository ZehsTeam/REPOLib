using UnityEngine;

namespace REPOLib.Extensions;

public static class GameObjectExtensions
{
    public static void FixAudioMixerGroups(this GameObject gameObject)
    {
        if (gameObject == null)
        {
            return;
        }

        if (AudioManager.instance == null)
        {
            Logger.LogWarning($"Failed to fix audio mixer groups on GameObject \"{gameObject.name}\". AudioManager instance is null.");
            return;
        }

        foreach (AudioSource audioSource in gameObject.GetComponentsInChildren<AudioSource>())
        {
            audioSource.FixAudioMixerGroup(gameObject);
        }
    }
}
