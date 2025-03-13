using System;
using UnityEngine;
using UnityEngine.Audio;

namespace REPOLib.Modules;

public class Utilities
{
    public enum AudioMixerGroupType
    {
        Persistent,
        Sound,
        Music,
        Microphone,
        MicrophoneSpectate,
        TTS,
        TTSSpectate
    }
    
    /// <summary>
    /// Fixes the mixer groups of all audio sources in the game object and its children.
    /// Assigns the audio mixer group based on the audio group type.
    /// Best to call this method in the Start or OnEnable method of the game object.
    /// - Vyrus
    /// </summary>
    public static void FixMixerGroups(GameObject gameObject, AudioMixerGroupType audioGroup)
    {
        AudioSource[] audioSources = gameObject.GetComponentsInChildren<AudioSource>();
        
        AudioMixerGroup targetMixerGroup = audioGroup switch
        {
            AudioMixerGroupType.Persistent => AudioManager.instance.PersistentSoundGroup,
            AudioMixerGroupType.Sound => AudioManager.instance.SoundMasterGroup,
            AudioMixerGroupType.Music => AudioManager.instance.MusicMasterGroup,
            AudioMixerGroupType.Microphone => AudioManager.instance.MicrophoneSoundGroup,
            AudioMixerGroupType.MicrophoneSpectate => AudioManager.instance.MicrophoneSpectateGroup,
            AudioMixerGroupType.TTS => AudioManager.instance.TTSSoundGroup,
            AudioMixerGroupType.TTSSpectate => AudioManager.instance.TTSSpectateGroup,
            _ => throw new ArgumentOutOfRangeException(nameof(audioGroup), audioGroup, null)
        };

        foreach (AudioSource audioSource in audioSources)
        {
            audioSource.outputAudioMixerGroup = targetMixerGroup.audioMixer.outputAudioMixerGroup;
        }
    }
}