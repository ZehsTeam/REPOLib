using UnityEngine;
using UnityEngine.Audio;

namespace REPOLib.Extensions;

/// <summary>
/// REPOLib's  <see cref="GameObject"/> extension methods.
/// </summary>
public static class GameObjectExtensions
{
    /// <summary>
    /// Fixes <see cref="AudioMixerGroup"/>s for all <see cref="AudioSource"/> components in a <see cref="GameObject"/>
    /// and its children by setting the <see cref="AudioSource"/>'s <see cref="AudioMixerGroup"/>
    /// to a matching one from the game.
    /// </summary>
    /// <param name="gameObject">The <see cref="GameObject"/> whose <see cref="AudioSource"/> components to fix.</param>
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
