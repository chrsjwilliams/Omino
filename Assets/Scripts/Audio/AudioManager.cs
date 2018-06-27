﻿using System;
using System.Collections;
using System.Collections.Generic;
using Beat;
using UnityEngine;
using EasingEquations;

public class AudioManager : MonoBehaviour {

    private AudioSource mainTrack;
    private TaskManager levelMusicManager;
    private List<AudioSource> effectChannels;
    private GameObject effectsHolder;
    private GameObject levelMusicHolder;
    private int effectChannelSize = 100;
    private int effectChannelIndex = 0;
    private List<AudioSource> levelMusicSources;
    private List<float> levelMusicVolumes, previousVolumes;
    private readonly float BASEMUSICVOLUME = 0.6f;
    private Dictionary<string, AudioClip> reversedClips;
    
    public void Awake()
    {
        DontDestroyOnLoad(gameObject);
        
        effectsHolder = new GameObject("Effect Tracks");
 
        effectsHolder.transform.parent = transform;
        _PopulateLevelMusic();
        _PopulateReversedEffects();

        effectChannels = new List<AudioSource>();

        for (int i = 0; i < effectChannelSize; i++)
        {
            GameObject channel = new GameObject("Effect Channel");
            channel.transform.parent = effectsHolder.transform;
            
            effectChannels.Add(channel.AddComponent<AudioSource>());
            effectChannels[i].loop = false;
        }
    }

    public void Update()
    {
        if (levelMusicManager != null)
        {
            levelMusicManager.Update();
        }
    }
    
    public void RegisterSoundEffect(AudioClip clip, float volume, Clock.BeatValue timing = Clock.BeatValue.Eighth)
    {
        Services.AudioManager.ConnectQuantizedClip(clip, Services.Clock.ReturnAtNext(timing) - AudioSettings.dspTime);
        
        Services.Clock.SyncFunction(_ParameterizeAction(PlaySoundEffect, clip, volume).Invoke, timing);
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
        levelMusicSources = new List<AudioSource>();
        
        levelMusicHolder = new GameObject("Level Music Tracks");
        levelMusicHolder.transform.parent = transform;

        int i = 1;
        
        foreach (AudioClip clip in Services.Clips.LevelTracks) {
            GameObject newTrack = new GameObject("Track " + i++);
            newTrack.transform.parent = levelMusicHolder.transform;
            AudioSource levelMusicTrack = newTrack.AddComponent<AudioSource>();
            levelMusicTrack.clip = clip;
            levelMusicTrack.loop = true;
            levelMusicTrack.volume = BASEMUSICVOLUME;
            
            levelMusicSources.Add (levelMusicTrack);
        }
    }

    private void _PopulateReversedEffects()
    {
        UnityEngine.Object[] reversed_effects = Resources.LoadAll("Audio/ReversedAudioSamples/", typeof(AudioClip));

        reversedClips = new Dictionary<string, AudioClip>();
        
        foreach (UnityEngine.Object effect in reversed_effects)
        {
            reversedClips.Add(((AudioClip)effect).name.Split('_')[1], (AudioClip)effect);
        }
    }

    public void RegisterStartLevelMusic()
    {
        Services.Clock.SyncFunction(_StartLevelMusic, Clock.BeatValue.Measure);
    }

    private void _StartLevelMusic()
    {
        levelMusicVolumes = new List<float>();
        
        for (int i = 1; i < levelMusicSources.Count; i++)
        {
            levelMusicSources[i].volume = 0;
        }

        levelMusicSources[0].volume = BASEMUSICVOLUME;
        
        foreach (AudioSource source in levelMusicSources)
        {
            source.Play();
            levelMusicVolumes.Add(source.volume);
        }
        
        levelMusicManager = new TaskManager();
        
        Task initialWait = new Wait(Services.Clock.MeasureLength() * 4);
        Task changeVolumes = new ActionTask(DynamicLevelMusicVolumes);

        initialWait.Then(changeVolumes);
        
        levelMusicManager.Do(initialWait);

        MuteMusicChannels();
    }

    private void DynamicLevelMusicVolumes()
    {
        previousVolumes = new List<float>();
        
        previousVolumes.Add(levelMusicSources[0].volume);

        if (levelMusicSources.Count >= 2)
        {
            previousVolumes.Add(levelMusicSources[1].volume);
            levelMusicVolumes[1] = Mathf.Clamp((float)Services.GameData.totalFilledMapTiles / (float)Services.GameData.totalMapTiles, 0.0f, BASEMUSICVOLUME);
        }

        if (levelMusicSources.Count >= 4)
        {
            previousVolumes.Add(levelMusicSources[2].volume);
            levelMusicVolumes[2] = Mathf.Clamp(Services.GameData.productionRates[0], 0.0f, BASEMUSICVOLUME);
            
            previousVolumes.Add(levelMusicSources[3].volume);
            levelMusicVolumes[3] = Mathf.Clamp(Services.GameData.productionRates[1], 0.0f, BASEMUSICVOLUME);
        }

        if (levelMusicSources.Count >= 6)
        {
            previousVolumes.Add(levelMusicSources[4].volume);
            levelMusicVolumes[4] = Mathf.Clamp(2.0f / Services.GameData.distancesToOpponentBase[0], 0.0f, BASEMUSICVOLUME);
            
            previousVolumes.Add(levelMusicSources[5].volume);
            levelMusicVolumes[5] = Mathf.Clamp(2.0f / Services.GameData.distancesToOpponentBase[1], 0.0f, BASEMUSICVOLUME);
        }

        if (levelMusicSources.Count >= 7)
        {
            for (int i = 6; i < levelMusicSources.Count; i++)
            {
                previousVolumes.Add(levelMusicSources[i].volume);
                levelMusicVolumes[i] = Mathf.Clamp(Services.GameData.secondsSinceMatchStarted / (5.0f * 60f), 0.0f,
                    BASEMUSICVOLUME);
            }
        }

        for (int i = 0; i < levelMusicSources.Count; i++)
        {
            AudioSource to_change = levelMusicSources[i];
            float starting_value = previousVolumes[i];
            float new_value = levelMusicVolumes[i];
            
            StartCoroutine(Coroutines.DoOverEasedTime(Services.Clock.HalfLength() + Services.Clock.QuarterLength(), Easing.Linear,
                t =>
                {
                    float new_volume = Mathf.Lerp(starting_value, new_value, t);
                    to_change.volume = new_volume;
                }));
        }
        
        Task measureWait = new Wait(Services.Clock.MeasureLength());
        Task changeVolumes = new ActionTask(DynamicLevelMusicVolumes);
        
        measureWait.Then(changeVolumes);
        levelMusicManager.Do(measureWait);
    }

    private void ConnectQuantizedClip(AudioClip clip, double amount_to_play)
    {
        // find reversed clip
        
        AudioClip reversed_clip = reversedClips[clip.name];
        
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
        
        if (Services.GameManager.SoundEffectsEnabled)
            to_play.clip = reversed_clip;
        else
            to_play.clip = Services.Clips.Silence;
        
        to_play.time = to_play.clip.length - (float)amount_to_play;
        to_play.volume = 0.2f;
        to_play.Play();
    }

    public void PlaySoundEffect(AudioClip clip, float volume = 1.0f)
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

        if (Services.GameManager.SoundEffectsEnabled)
            to_play.clip = clip;
        else
            to_play.clip = Services.Clips.Silence;
        
        to_play.volume = volume;
        to_play.Play();
    }

    public void FadeOutLevelMusic()
    {
        previousVolumes = new List<float>();

        foreach (AudioSource source in levelMusicSources)
        {
            previousVolumes.Add(source.volume);
        }
        
        for (int i = 0; i < levelMusicSources.Count; i++)
        {
            AudioSource to_change = levelMusicSources[i];
            float starting_value = previousVolumes[i];
            float new_value = 0.0f;
            
            StartCoroutine(Coroutines.DoOverEasedTime(Services.Clock.MeasureLength(), Easing.Linear,
                t =>
                {
                    float new_volume = Mathf.Lerp(starting_value, new_value, t);
                    to_change.volume = new_volume;
                }));
        }

        levelMusicManager = new TaskManager();
        
        Task wait = new Wait(Services.Clock.MeasureLength() * 2);
        wait.Then(new ActionTask(() => { levelMusicHolder = null; }));
        
        levelMusicManager.Do(wait);
    }

    public void FadeOutLevelMusicMainMenuCall()
    {
        FadeOutLevelMusic();
        Destroy(gameObject, Services.Clock.MeasureLength() * 4);
    }

    public void ResetLevelMusic()
    {
        foreach (AudioSource source in levelMusicSources)
        {
            source.volume = 0;
        }
        
        levelMusicSources[0].volume = BASEMUSICVOLUME;
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
        mainTrack.loop = true;
        mainTrack.Play();
    }

    public void ToggleSoundEffects()
    {
        Services.GameManager.SoundEffectsEnabled = !Services.GameManager.SoundEffectsEnabled;
    }
    
    public void ToggleMusic()
    {
        Services.GameManager.MusicEnabled = !Services.GameManager.MusicEnabled;
        
        MuteMusicChannels();
    }

    public void MuteMusicChannels()
    {        
        foreach (AudioSource source in levelMusicSources)
        {
            source.mute = !Services.GameManager.MusicEnabled;
        }
    }
}
