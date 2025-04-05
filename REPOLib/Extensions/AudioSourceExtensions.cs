using UnityEngine;
using UnityEngine.Audio;

namespace REPOLib.Extensions;

/// <summary>
/// REPOLib's <see cref="AudioSource"/> extension methods.
/// </summary>
public static class AudioSourceExtensions
{
    /// <inheritdoc cref="FixAudioMixerGroup(AudioSource, GameObject)"/>
    public static void FixAudioMixerGroup(this AudioSource audioSource)
    {
        audioSource.FixAudioMixerGroup(audioSource.gameObject);
    }

    /// <summary>
    /// Fixes the <see cref="AudioMixerGroup"/> for an <see cref="AudioSource"/> component on a <see cref="GameObject"/>
    /// by setting the <see cref="AudioSource"/>'s <see cref="AudioMixerGroup"/> to a matching one from the game.
    /// </summary>
    /// <param name="audioSource">The <see cref="AudioSource"/> whose <see cref="AudioMixerGroup"/> to fix.</param>
    /// <param name="parentObject">The <see cref="GameObject"/> whose <see cref="AudioSource"/> component to fix.</param>
    public static void FixAudioMixerGroup(this AudioSource audioSource, GameObject parentObject)
    {
        if (audioSource == null)
        {
            return;
        }

        string fullGameObjectName;

        if (parentObject == audioSource.gameObject)
        {
            fullGameObjectName = audioSource.gameObject.name;
        }
        else
        {
            fullGameObjectName = $"{parentObject.name}/{audioSource.gameObject.name}";
        }

        if (AudioManager.instance == null)
        {
            Logger.LogWarning($"Failed to fix AudioMixerGroup on GameObject \"{fullGameObjectName}\". AudioManager instance is null.");
            return;
        }

        if (audioSource.outputAudioMixerGroup == null)
        {
            Logger.LogWarning($"Failed to fix AudioMixerGroup on GameObject \"{fullGameObjectName}\". No AudioMixerGroup is assigned.");
            return;
        }

        AudioMixer audioMixer = audioSource.outputAudioMixerGroup.audioMixer.name switch
        {
            "Master" => AudioManager.instance.MasterMixer,
            "Music" => AudioManager.instance.MusicMasterGroup.audioMixer,
            "Sound" => AudioManager.instance.SoundMasterGroup.audioMixer,
            "Spectate" => AudioManager.instance.MicrophoneSpectateGroup.audioMixer,
            _ => AudioManager.instance.SoundMasterGroup.audioMixer,
        };

        AudioMixerGroup[] audioMixerGroups = audioMixer.FindMatchingGroups(audioSource.outputAudioMixerGroup.name);
        AudioMixerGroup audioMixerGroup;

        if (audioMixerGroups.Length >= 1)
        {
            audioMixerGroup = audioMixerGroups[0];
        }
        else
        {
            audioMixer = AudioManager.instance.SoundMasterGroup.audioMixer;
            audioMixerGroup = audioMixer.FindMatchingGroups("Sound Effects")[0];

            Logger.LogWarning($"Could not find matching AudioMixerGroup for GameObject \"{fullGameObjectName}\". Using default AudioMixerGroup \"{audioMixer.name}/{audioMixerGroup.name}\"", extended: true);
        }

        audioSource.outputAudioMixerGroup = audioMixerGroup;

        Logger.LogDebug($"Fixed AudioMixerGroup on GameObject \"{fullGameObjectName}\". AudioMixerGroup \"{audioMixer.name}/{audioMixerGroup.name}\"", extended: true);
    }
}
