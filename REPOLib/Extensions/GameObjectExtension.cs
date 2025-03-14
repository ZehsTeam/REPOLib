using UnityEngine;
using UnityEngine.Audio;

namespace REPOLib.Extensions;

internal static class GameObjectExtension
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

        AudioSource[] audioSources = gameObject.GetComponentsInChildren<AudioSource>();

        foreach (AudioSource audioSource in audioSources)
        {
            if (audioSource.outputAudioMixerGroup == null)
            {
                Logger.LogWarning($"Failed to fix audio mixer groups on GameObject \"{gameObject.name}\". No audio mixer group is assigned.");
                continue;
            }

            AudioMixer audioMixer = audioSource.outputAudioMixerGroup.audioMixer.name switch
            {
                "Master" =>   AudioManager.instance.MasterMixer,
                "Music" =>    AudioManager.instance.MusicMasterGroup.audioMixer,
                "Sound" =>    AudioManager.instance.SoundMasterGroup.audioMixer,
                "Spectate" => AudioManager.instance.MicrophoneSpectateGroup.audioMixer,
                _ =>          AudioManager.instance.SoundMasterGroup.audioMixer,
            };

            AudioMixerGroup[] audioMixerGroups = audioMixer.FindMatchingGroups(audioSource.outputAudioMixerGroup.name);
            AudioMixerGroup audioMixerGroup;

            if (audioMixerGroups.Length >= 1)
            {
                audioMixerGroup = audioMixerGroups[0];
            }
            else
            {
                audioMixerGroup = AudioManager.instance.SoundMasterGroup;
                Logger.LogWarning($"Could not find matching audio mixer group for GameObject \"{gameObject.name}\" -> AudioSource \"{audioSource.name}\". Using default audio mixer group \"{audioMixerGroup.name}\"", extended: true);
            }

            audioSource.outputAudioMixerGroup = audioMixerGroup;

            Logger.LogInfo($"Fixed audio mixer group on GameObject \"{gameObject.name}\" -> AudioSource \"{audioSource.name}\". AudioMixerGroup \"{audioMixer.name} > {audioMixerGroup.name}\"", extended: true);
        }
    }
}
