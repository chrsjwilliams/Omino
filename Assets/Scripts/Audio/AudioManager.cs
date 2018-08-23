using System;
using System.Collections;
using System.Collections.Generic;
using BeatManagement;
using UnityEngine;
using EasingEquations;

public class AudioManager : MonoBehaviour {

    private AudioSource mainTrack;
    private List<AudioSource> effectChannels;
    private GameObject effectsHolder;
    private GameObject levelMusicHolder;
    private int effectChannelSize = 100;
    private int effectChannelIndex = 0;
    private List<AudioSource> levelMusicSources;
    private List<float> levelMusicVolumes, previousVolumes;
    private readonly float BASEMUSICVOLUME = 0.6f;
    private Dictionary<string, AudioClip> reversedClips;
    private Dictionary<string, AudioClip> reverbClips;
    private Dictionary<string, AudioClip> materialClips;
    private bool silent = false;
    
    public void Awake()
    {
        DontDestroyOnLoad(gameObject);
        
        effectsHolder = new GameObject("Effect Tracks");
 
        effectsHolder.transform.parent = transform;
        _PopulateLevelMusic();
        _PopulateReversedEffects();
        _PopulateReverbEffects();
        _PopulateMaterialEffects();

        effectChannels = new List<AudioSource>();

        for (int i = 0; i < effectChannelSize; i++)
        {
            GameObject channel = new GameObject("Effect Channel");
            channel.transform.parent = effectsHolder.transform;
            
            effectChannels.Add(channel.AddComponent<AudioSource>());
            effectChannels[i].loop = false;
        }
    }
    
    public void RegisterSoundEffect(AudioClip clip, float volume = 1.0f, Clock.BeatValue timing = Clock.BeatValue.Eighth)
    {
        // Services.AudioManager.ConnectQuantizedClipReverse(clip, Services.Clock.ReturnAtNext(timing) - AudioSettings.dspTime);
        if (!silent)
        {
            Services.AudioManager.PlaySoundEffectMaterial(clip, 0.5f);

            Services.Clock.SyncFunction(_ParameterizeAction(PlaySoundEffect, clip, volume).Invoke, timing);
        }
    }

    public void RegisterSoundEffectReverb(AudioClip clip, float volume = 1.0f,
        Clock.BeatValue timing = Clock.BeatValue.Eighth)
    {
        if (!silent)
        {
            Services.AudioManager.PlaySoundEffectReverb(clip, 0.5f);
            Services.Clock.SyncFunction(_ParameterizeAction(PlaySoundEffect, clip, volume).Invoke, timing);
        }
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
    
    private void _PopulateReverbEffects()
    {
        UnityEngine.Object[] reversed_effects = Resources.LoadAll("Audio/ReverbAudioSamples/", typeof(AudioClip));

        reversedClips = new Dictionary<string, AudioClip>();
        
        foreach (UnityEngine.Object effect in reversed_effects)
        {
            reversedClips.Add(((AudioClip)effect).name.Split('_')[1], (AudioClip)effect);
        }
    }

    private void _PopulateMaterialEffects()
    {
        UnityEngine.Object[] material_effects = Resources.LoadAll("Audio/MaterialAudioSamples/", typeof(AudioClip));

        materialClips = new Dictionary<string, AudioClip>();
        
        foreach (UnityEngine.Object effect in material_effects)
        {
            materialClips.Add(((AudioClip)effect).name, (AudioClip)effect);
        }
    }

    public void RegisterStartLevelMusic()
    {
        Destroy(levelMusicHolder);
        _PopulateLevelMusic();
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
        
        Services.Clock.eventManager.Register<Measure>(DynamicLevelMusicVolumes);
        MuteMusicChannels();
    }

    private void DynamicLevelMusicVolumes(BeatEvent e)
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
    }

    private void ConnectQuantizedClipReverse(AudioClip clip, double amount_to_play)
    {
        // find reversed clip
        if (clip.length > amount_to_play)
        {
            AudioClip reversedClip = Services.Clips.Silence;
            
            if ((reversedClips.ContainsKey(clip.name)) && (Services.GameManager.SoundEffectsEnabled))
                 reversedClip = reversedClips[clip.name];
            
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

            to_play.clip = reversedClip;
    
            to_play.time = to_play.clip.length - (float) amount_to_play;
            to_play.volume = 0.2f;
            to_play.Play();
        }
    }

    public void PlaySoundEffectReverb(AudioClip clip, float volume = 1.0f)
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

        to_play.clip = Services.Clips.Silence;
        
        if ((Services.GameManager.SoundEffectsEnabled) && (reversedClips.ContainsKey(clip.name)))
            to_play.clip = reversedClips[clip.name];
            
        
        to_play.volume = volume;
        to_play.Play();
    }

    public void PlaySoundEffectMaterial(AudioClip clip, float volume = 1.0f)
    {
        AudioClip to_play = Services.Clips.Silence;
        
        if ((Services.GameManager.SoundEffectsEnabled) && (materialClips.ContainsKey(clip.name)))
            to_play = materialClips[clip.name];
            
        Services.Clock.SyncFunction(_ParameterizeAction(PlaySoundEffect, to_play, volume).Invoke, Clock.BeatValue.ThirtySecond);
    }

    public void PlaySoundEffect(AudioClip clip, float volume = 1.0f)
    {
        if (!silent)
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
    }

    public void FadeOutLevelMusic()
    {
        Services.Clock.eventManager.Unregister<Measure>(DynamicLevelMusicVolumes);
        previousVolumes = new List<float>();
        var to_destroy = levelMusicHolder;

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
        
        Delay(() =>
            {
                Destroy(to_destroy);
        }, Services.Clock.MeasureLength() * 2);
        
    }

    public void Delay(System.Action callback, float delayTime)
    {
        StartCoroutine(YieldForSync(callback, delayTime));
    }

    IEnumerator YieldForSync(System.Action callback, float delayTime)
    {
        float timeElapsed = 0.0f;
        bool waiting = true;
        while (waiting)
        {
            timeElapsed += Time.deltaTime;
            
            if (timeElapsed > delayTime)
                waiting = false;
            else
                yield return false;
        }
        callback();
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
        
        Services.Clock.ClearEvents();
        StopAllCoroutines();
    }

    public void SetMainTrack(AudioClip clip, float volume)
    {
        if (mainTrack == null)
        {
            GameObject obj = new GameObject();
            //obj.name = "Main Track: " + clip.name;
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

    public void SetPitch(float pitch)
    {
        foreach (AudioSource source in levelMusicSources)
        {
            source.pitch = pitch;
        }
    }

    public TaskTree SlowMo(float duration)
    {
        silent = true;
        ActionTask slow_down = new ActionTask(() =>
        {
            StartCoroutine(Coroutines.DoOverEasedTime(duration/2, Easing.Linear,
                t =>
                {
                    Services.AudioManager.SetPitch(Mathf.Lerp(1, 0.1f, t));
                }));
        });
        
        Wait wait = new Wait(duration/2);
        
        ActionTask speed_up = new ActionTask(() =>
        {
            StartCoroutine(Coroutines.DoOverEasedTime(duration/2, Easing.Linear,
                t =>
                {
                    Services.AudioManager.SetPitch(Mathf.Lerp(0.1f, 1, t));
                }));
        });
        
        Wait wait2 = new Wait(duration/2);
        
        ActionTask reset = new ActionTask(() => { Services.AudioManager.SetPitch(1f); silent = false; });

        TaskTree to_return = new TaskTree(slow_down, new TaskTree(wait, new TaskTree(speed_up, new TaskTree(wait2, new TaskTree(reset)))));

        return to_return;
    }
}
