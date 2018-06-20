using System;
using System.Collections;
using System.Collections.Generic;
using Beat;
using UnityEngine;

public class AudioManager {

    private AudioSource mainTrack;
    private float last_volume = 1.0f;
    private List<AudioSource> effectChannels;
    private GameObject effectsHolder;
    private int effectChannelSize = 100;
    private int effectChannelIndex = 0;
    private List<AudioSource> level_music_sources;

    public void Init()
    {
        GameObject main = GameObject.Find("Main");
        effectsHolder = new GameObject("Effect Tracks");
        effectsHolder.transform.parent = main.transform;
        _PopulateLevelMusic();

        effectChannels = new List<AudioSource>();

        for (int i = 0; i < effectChannelSize; i++)
        {
            GameObject channel = new GameObject("Effect Channel");
            channel.transform.parent = effectsHolder.transform;
            
            effectChannels.Add(channel.AddComponent<AudioSource>());
            effectChannels[i].loop = false;
        }
    }
    
    public void RegisterSoundEffect(AudioClip clip, float volume)
    {
        Services.Clock.SyncFunction(_ParameterizeAction(PlaySoundEffect, clip, volume).Invoke, Clock.BeatValue.Eighth);
    }
    
    private System.Action _ParameterizeAction(System.Action<AudioClip, float> function, AudioClip clip, float volume)
    {
        System.Action to_return = () =>
        {
            function(clip, volume);
        };
        
        return to_return;
    }

    private void _PopulateLevelMusic()
    {
        level_music_sources = new List<AudioSource>();
        
        GameObject levelMusicHolder = new GameObject("Level Music Tracks");

        int i = 1;
        
        foreach (AudioClip clip in Services.Clips.LevelTracks) {
            GameObject newTrack = new GameObject("Track " + i++);
            newTrack.transform.parent = levelMusicHolder.transform;
            AudioSource levelMusicTrack = newTrack.AddComponent<AudioSource>();
            levelMusicTrack.clip = clip;
            levelMusicTrack.loop = true;
            levelMusicTrack.volume = 0.6f;
            
            level_music_sources.Add (levelMusicTrack);
        }
    }

    public void RegisterStartLevelMusic()
    {
        Services.Clock.SyncFunction(_StartLevelMusic, Clock.BeatValue.Measure);
    }

    private void _StartLevelMusic()
    {
        foreach (AudioSource source in level_music_sources)
        {
            source.Play();
        }
    }

    public void PlaySoundEffect(AudioClip clip, float volume)
    {
        AudioSource to_play = effectChannels[effectChannelIndex];
        effectChannelIndex = (effectChannelIndex + 1) % effectChannelSize;

        if (to_play.isPlaying)
        {
            GameObject channel = new GameObject("Effect Channel");
            channel.transform.parent = effectsHolder.transform;
            effectChannels.Insert(effectChannelIndex, channel.AddComponent<AudioSource>());
            effectChannels[effectChannelIndex].loop = false;
            
            to_play = effectChannels[effectChannelIndex];
            effectChannelIndex = (effectChannelIndex + 1) % effectChannelSize;
        }

        to_play.clip = clip;
        to_play.volume = volume;
        to_play.Play();
    }

    public void SetMainTrack(AudioClip clip, float volume)
    {
        if (mainTrack == null)
        {
            GameObject obj = new GameObject();
            obj.name = "Main Track: " + clip.name;
            mainTrack = obj.AddComponent<AudioSource>();
        }
        
        mainTrack.clip = clip;
        mainTrack.volume = volume;
        last_volume = volume;
        mainTrack.loop = true;
        mainTrack.Play();
    }

    public void ToggleSoundEffects()
    {
        Services.GameManager.SoundEffectsEnabled = !Services.GameManager.SoundEffectsEnabled;
        SetSoundEffectsOnOrOff();
    }

    public void SetSoundEffectsOnOrOff()
    {
        if (Services.GameManager.SoundEffectsEnabled)
        {
            Services.Clips = Resources.Load<ClipLibrary>("Audio/QuantizedClipLibrary");
        }
        else
        {
            Services.Clips = Resources.Load<ClipLibrary>("Audio/SilentClipLibrary");
        }
    }
    
    public void ToggleMusic()
    {
        Services.GameManager.MusicEnabled = !Services.GameManager.MusicEnabled;
        SetMusicOnOrOff();
    }

    public void SetMusicOnOrOff()
    {
        if (mainTrack == null)
        {
            GameObject obj = new GameObject();
            obj.name = "Main Track";
            mainTrack = obj.AddComponent<AudioSource>();
        }
        
        if (Services.GameManager.MusicEnabled)
        {
            mainTrack.volume = last_volume;
        }
        else
        {
            mainTrack.volume = 0.0f;
        }
    }
}
