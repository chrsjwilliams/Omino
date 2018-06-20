using System;
using System.Collections;
using System.Collections.Generic;
using Beat;
using UnityEngine;
using EasingEquations;

public class AudioManager : MonoBehaviour {

    private AudioSource mainTrack;
    private TaskManager levelMusicManager;
    private float last_volume = 1.0f;
    private List<AudioSource> effectChannels;
    private GameObject effectsHolder;
    private int effectChannelSize = 100;
    private int effectChannelIndex = 0;
    private List<AudioSource> level_music_sources;
    private List<float> level_music_volumes, previous_volumes;
    private readonly float BASEMUSICVOLUME = 0.6f;
    
    public void Awake()
    {
        effectsHolder = new GameObject("Effect Tracks");
 
        effectsHolder.transform.parent = transform;
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

    public void Update()
    {
        if (levelMusicManager != null)
        {
            levelMusicManager.Update();
        }
    }
    
    public void RegisterSoundEffect(AudioClip clip, float volume)
    {
        Services.Clock.SyncFunction(_ParameterizeAction(PlaySoundEffect, clip, volume).Invoke, Clock.BeatValue.Eighth);
    }
    
    public void RegisterSoundEffect(AudioClip clip, float volume, Clock.BeatValue timing)
    {
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
        level_music_sources = new List<AudioSource>();
        
        GameObject levelMusicHolder = new GameObject("Level Music Tracks");
        levelMusicHolder.transform.parent = transform;

        int i = 1;
        
        foreach (AudioClip clip in Services.Clips.LevelTracks) {
            GameObject newTrack = new GameObject("Track " + i++);
            newTrack.transform.parent = levelMusicHolder.transform;
            AudioSource levelMusicTrack = newTrack.AddComponent<AudioSource>();
            levelMusicTrack.clip = clip;
            levelMusicTrack.loop = true;
            levelMusicTrack.volume = BASEMUSICVOLUME;
            
            level_music_sources.Add (levelMusicTrack);
        }
    }

    public void RegisterStartLevelMusic()
    {
        Services.Clock.SyncFunction(_StartLevelMusic, Clock.BeatValue.Measure);
    }

    private void _StartLevelMusic()
    {
        level_music_volumes = new List<float>();
        
        for (int i = 1; i < level_music_sources.Count; i++)
        {
            level_music_sources[i].volume = 0;
        }
        
        foreach (AudioSource source in level_music_sources)
        {
            source.Play();
            level_music_volumes.Add(source.volume);
        }
        
        levelMusicManager = new TaskManager();
        
        Task initialWait = new Wait(Services.Clock.MeasureLength() * 4);
        Task changeVolumes = new ActionTask(ChangeVolumes);

        initialWait.Then(changeVolumes);
        
        levelMusicManager.Do(initialWait);
    }

    private void ChangeVolumes()
    {
        previous_volumes = new List<float>();
        
        previous_volumes.Add(level_music_sources[0].volume);

        if (level_music_sources.Count >= 2)
        {
            previous_volumes.Add(level_music_sources[1].volume);
            level_music_volumes[1] = Mathf.Clamp((float)Services.GameData.totalFilledMapTiles / (float)Services.GameData.totalMapTiles, 0.0f, BASEMUSICVOLUME);
        }

        if (level_music_sources.Count >= 4)
        {
            previous_volumes.Add(level_music_sources[2].volume);
            level_music_volumes[2] = Mathf.Clamp(Services.GameData.productionRates[0], 0.0f, BASEMUSICVOLUME);
            
            previous_volumes.Add(level_music_sources[3].volume);
            level_music_volumes[3] = Mathf.Clamp(Services.GameData.productionRates[1], 0.0f, BASEMUSICVOLUME);
        }

        if (level_music_sources.Count >= 6)
        {
            previous_volumes.Add(level_music_sources[4].volume);
            level_music_volumes[4] = Mathf.Clamp(2.0f / Services.GameData.distancesToOpponentBase[0], 0.0f, BASEMUSICVOLUME);
            
            previous_volumes.Add(level_music_sources[5].volume);
            level_music_volumes[5] = Mathf.Clamp(2.0f / Services.GameData.distancesToOpponentBase[1], 0.0f, BASEMUSICVOLUME);
        }

        if (level_music_sources.Count >= 7)
        {
            for (int i = 6; i < level_music_sources.Count; i++)
            {
                previous_volumes.Add(level_music_sources[i].volume);
                level_music_volumes[i] = Mathf.Clamp(Services.GameData.secondsSinceMatchStarted / 5.0f * 60f, 0.0f,
                    BASEMUSICVOLUME);
            }
        }

        for (int i = 0; i < level_music_sources.Count; i++)
        {
            AudioSource to_change = level_music_sources[i];
            float starting_value = previous_volumes[i];
            float new_value = level_music_volumes[i];
            Debug.Log("Track " + i + ": " + starting_value + "->" + new_value);
            
            StartCoroutine(Coroutines.DoOverEasedTime(Services.Clock.HalfLength() + Services.Clock.QuarterLength(), Easing.Linear,
                t =>
                {
                    float new_volume = Mathf.Lerp(starting_value, new_value, t);
                    to_change.volume = new_volume;
                }));
        }
        
        Task measureWait = new Wait(Services.Clock.MeasureLength());
        Task changeVolumes = new ActionTask(ChangeVolumes);
        
        measureWait.Then(changeVolumes);
        levelMusicManager.Do(measureWait);
    }

    public void StopLevelMusic()
    {
        levelMusicManager = null;
        foreach (AudioSource source in level_music_sources)
        {
            source.Stop();
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
