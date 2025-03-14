using System;
using UnityEngine;
using UnityEngine.Audio;

namespace REPOLib.Modules;

public class Utilities
{

    /// <summary>
    /// Fixes the mixer groups of all audio sources in the game object and its children.
    /// Assigns the audio mixer group based on the audio group found assigned in the prefab.
    /// Best to call this method in the Start, or OnEnable method of the game object.
    /// - Vyrus
    /// </summary>
    public static void FixMixerGroups(GameObject gameObject)
    {
        AudioSource[] audioSources = gameObject.GetComponentsInChildren<AudioSource>();

        foreach (AudioSource audioSource in audioSources)
        {
            // audioSource.outputAudioMixerGroup = targetMixerGroup.audioMixer.outputAudioMixerGroup;

            if (audioSource.outputAudioMixerGroup == null)
            {
                Logger.LogInfo($"No Audio Mixer Group is assigned to {audioSource.name}.");
                continue;
            }

            AudioMixerGroup masterGroup = audioSource.outputAudioMixerGroup.audioMixer.name switch
            {
                "Master" => AudioManager.instance.MasterMixer.outputAudioMixerGroup,
                "Sound" => AudioManager.instance.SoundMasterGroup,
                "Music" => AudioManager.instance.MusicMasterGroup,
                "Spectate" => AudioManager.instance.MicrophoneSpectateGroup,
                _=> AudioManager.instance.SoundMasterGroup
            };

            Logger.LogInfo($"Audio Mixer Group is now assigned to {masterGroup.name}. Audio Source: {audioSource.name}");
            audioSource.outputAudioMixerGroup = masterGroup;
            
        }
    }
}