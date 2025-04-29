using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Audio;

namespace REPOLib.Extensions;

[PublicAPI]
internal static class AudioSourceExtensions
{
    public static void FixAudioMixerGroup(this AudioSource audioSource)
        => audioSource.FixAudioMixerGroup(audioSource.gameObject);

    public static void FixAudioMixerGroup(this AudioSource audioSource, GameObject rootObject)
    {
        if (audioSource == null)
            return;

        string fullGameObjectName = rootObject == audioSource.gameObject
            ? audioSource.gameObject.name
            : $"{rootObject.name}/{audioSource.gameObject.name}";

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

        AudioMixer? audioMixer = audioSource.outputAudioMixerGroup.audioMixer.name switch
        { 
            "Master" => AudioManager.instance.MasterMixer,
            "Music" => AudioManager.instance.MusicMasterGroup.audioMixer,
            "Sound" => AudioManager.instance.SoundMasterGroup.audioMixer,
            "Spectate" => AudioManager.instance.MicrophoneSpectateGroup.audioMixer,
            _ => AudioManager.instance.SoundMasterGroup.audioMixer 
        };

        AudioMixerGroup audioMixerGroup;
        AudioMixerGroup[] audioMixerGroups = audioMixer.FindMatchingGroups(audioSource.outputAudioMixerGroup.name);

        if (audioMixerGroups.Length < 1)
        {
            audioMixer = AudioManager.instance.SoundMasterGroup.audioMixer;
            audioMixerGroup = audioMixer.FindMatchingGroups("Sound Effects")[0];
            Logger.LogWarning(
                $"Could not find matching AudioMixerGroup for GameObject \"{fullGameObjectName}\". Using default AudioMixerGroup \"{audioMixer.name}/{audioMixerGroup.name}\"",
                true);
        }
        else
        {
            audioMixerGroup = audioMixerGroups[0];
        }

        audioSource.outputAudioMixerGroup = audioMixerGroup;
        Logger.LogDebug($"Fixed AudioMixerGroup on GameObject \"{fullGameObjectName}\". AudioMixerGroup \"{audioMixer.name}/{audioMixerGroup.name}\"");
    }
}