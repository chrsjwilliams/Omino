using System.Collections;
using System.Collections.Generic;
using BeatManagement;
using UnityEngine;

public class AudioManager : MonoBehaviour {

    private AudioSource mainTrack;
    private List<AudioSource> effectChannels;
    private GameObject effectsHolder;
    private GameObject levelMusicHolder;
    private readonly int effectChannelSize = 200;
    private int _effectChannelIndex = 0;
    private List<AudioSource> _levelMusicSources;
    private List<float> _levelMusicVolumes, _previousVolumes;
    private const float BaseMusicVolume = 0.65f;
    private Dictionary<string, AudioClip> reversedClips;
    private Dictionary<string, AudioClip> reverbClips;
    private Dictionary<string, AudioClip> materialClips;
    private bool silent = false;
    private TaskManager _tm = new TaskManager();

    private Coroutine mainTrackFade;
    private Coroutine[] levelTracksFade;
    
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

        for (var i = 0; i < effectChannelSize; i++)
        {
            GameObject channel = new GameObject("Effect Channel");
            channel.transform.parent = effectsHolder.transform;
            
            effectChannels.Add(channel.AddComponent<AudioSource>());
            effectChannels[i].loop = false;
        }
    }

    private void Update()
    {
        _tm.Update();
        _SyncLevelMusicSources();
    }

    private void _SyncLevelMusicSources()
    {
        if (_levelMusicSources == null) return;

        for (var i = 1; i < _levelMusicSources.Count; i++)
        {
            if (_levelMusicSources[i] == null) continue;

            _levelMusicSources[i].timeSamples = _levelMusicSources[0].timeSamples;
        }
    }
    
    public void RegisterSoundEffect(AudioClip clip, float volume = 1.0f, Clock.BeatValue timing = Clock.BeatValue.Eighth)
    {
        if (silent) return;
        
        Services.AudioManager.PlaySoundEffectMaterial(clip, 0.5f);

        Services.Clock.SyncFunction(_ParameterizeAction(PlaySoundEffect, clip, volume).Invoke, timing);
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
        _levelMusicSources = new List<AudioSource>();
        
        levelMusicHolder = new GameObject("Level Music Tracks");
        levelMusicHolder.transform.parent = transform;

        var i = 1;
        
        foreach (var clip in Services.Clips.LevelTracks) {
            var newTrack = new GameObject("Track " + i++);
            newTrack.transform.parent = levelMusicHolder.transform;
            var levelMusicTrack = newTrack.AddComponent<AudioSource>();
            levelMusicTrack.clip = clip;
            levelMusicTrack.loop = true;
            levelMusicTrack.volume = BaseMusicVolume;
            
            _levelMusicSources.Add (levelMusicTrack);
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
        _ReadyMusic();

        var wait = new Wait(0.15f);
        var start = new ActionTask(() => Services.Clock.SyncFunction(_StartLevelMusic));

        wait.Then(start);

        _tm.Do(wait);
    }

    private void _ReadyMusic()
    {
        _levelMusicVolumes = new List<float>();
        
        for (var i = 1; i < _levelMusicSources.Count; i++)
        {
            _levelMusicSources[i].Stop();
            _levelMusicSources[i].volume = 0;
        }

        _levelMusicSources[0].volume = BaseMusicVolume;
        
        foreach (var source in _levelMusicSources)
        {
            _levelMusicVolumes.Add(source.volume);
        }
        
        Services.Clock.eventManager.Register<Measure>(DynamicLevelMusicVolumes);
        MuteMusicChannels();
    }
    
    private void _StartLevelMusic()
    {
        foreach (var source in _levelMusicSources)
        {
            source.Stop();
            source.Play();
        }
    }

    private void DynamicLevelMusicVolumes(BeatEvent e)
    {
        _previousVolumes = new List<float>();
        
        _previousVolumes.Add(_levelMusicSources[0].volume);

        if (_levelMusicSources.Count >= 2)
        {
            _previousVolumes.Add(_levelMusicSources[1].volume);
            _levelMusicVolumes[1] = Mathf.Lerp(0.0f, BaseMusicVolume, (float) Services.GameData.totalFilledMapTiles / Services.GameData.totalMapTiles);
        }

        if (_levelMusicSources.Count >= 4)
        {
            _previousVolumes.Add(_levelMusicSources[2].volume);
            _levelMusicVolumes[2] = Mathf.Lerp(0, BaseMusicVolume, Services.GameData.productionRates[0] / Services.GameData.maxProductionRates);
            
            _previousVolumes.Add(_levelMusicSources[3].volume);
            _levelMusicVolumes[3] = Mathf.Lerp(0, BaseMusicVolume, Services.GameData.productionRates[1] / Services.GameData.maxProductionRates);
        }

        if (_levelMusicSources.Count >= 6)
        {
            _previousVolumes.Add(_levelMusicSources[4].volume);
            _levelMusicVolumes[4] = Mathf.Clamp(2.0f / Services.GameData.distancesToOpponentBase[0], 0.0f, BaseMusicVolume);
            
            _previousVolumes.Add(_levelMusicSources[5].volume);
            _levelMusicVolumes[5] = Mathf.Clamp(2.0f / Services.GameData.distancesToOpponentBase[1], 0.0f, BaseMusicVolume);
        }

        if (_levelMusicSources.Count >= 7)
        {
            for (int i = 6; i < _levelMusicSources.Count; i++)
            {
                _previousVolumes.Add(_levelMusicSources[i].volume);
                _levelMusicVolumes[i] = Mathf.Clamp(Services.GameData.secondsSinceMatchStarted / (5.0f * 60f), 0.0f,
                    BaseMusicVolume);
            }
        }

        if (levelTracksFade != null)
            foreach (var fade in levelTracksFade)
               StopCoroutine(fade);

        levelTracksFade = new Coroutine[_levelMusicSources.Count];

        for (var i = 0; i < _levelMusicSources.Count; i++)
        {
            var toChange = _levelMusicSources[i];
            var startingValue = _previousVolumes[i];
            var newValue = _levelMusicVolumes[i];
            
            levelTracksFade[i] = StartCoroutine(Coroutines.DoOverEasedTime(Services.Clock.HalfLength() + Services.Clock.QuarterLength(), Easing.Linear,
                t =>
                {
                    var newVolume = Mathf.Lerp(startingValue, newValue, t);
                    toChange.volume = newVolume;
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
            
            AudioSource to_play = effectChannels[_effectChannelIndex];
            _effectChannelIndex = (_effectChannelIndex + 1) % effectChannelSize;
    
            if (to_play.isPlaying)
            {
                GameObject channel = new GameObject("Effect Channel");
                channel.transform.parent = effectsHolder.transform;
                effectChannels.Insert(_effectChannelIndex, channel.AddComponent<AudioSource>());
                effectChannels[_effectChannelIndex].loop = false;
    
                to_play = effectChannels[_effectChannelIndex];
                _effectChannelIndex = (_effectChannelIndex + 1) % effectChannelSize;
            }

            to_play.clip = reversedClip;
    
            to_play.time = to_play.clip.length - (float) amount_to_play;
            to_play.volume = 0.2f;
            to_play.Play();
        }
    }

    public void PlaySoundEffectReverb(AudioClip clip, float volume = 1.0f)
    {
        AudioSource to_play = effectChannels[_effectChannelIndex];
        _effectChannelIndex = (_effectChannelIndex + 1) % effectChannelSize;

        if (to_play.isPlaying)
        {
            GameObject channel = new GameObject("Effect Channel");
            channel.transform.parent = effectsHolder.transform;
            effectChannels.Insert(_effectChannelIndex, channel.AddComponent<AudioSource>());
            effectChannels[_effectChannelIndex].loop = false;
            
            to_play = effectChannels[_effectChannelIndex];
            _effectChannelIndex = (_effectChannelIndex + 1) % effectChannelSize;
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
        if (silent) return;
        
        var toPlay = effectChannels[_effectChannelIndex];
        _effectChannelIndex = (_effectChannelIndex + 1) % effectChannelSize;

        if (toPlay.isPlaying)
        {
            var channel = new GameObject("Effect Channel");
            channel.transform.parent = effectsHolder.transform;
            effectChannels.Insert(_effectChannelIndex, channel.AddComponent<AudioSource>());
            effectChannels[_effectChannelIndex].loop = false;

            toPlay = effectChannels[_effectChannelIndex];
            _effectChannelIndex = (_effectChannelIndex + 1) % effectChannelSize;
        }

        toPlay.clip = Services.GameManager.SoundEffectsEnabled ? clip : Services.Clips.Silence;

        toPlay.volume = volume;
        toPlay.Play();
    }

    public void FadeOutLevelMusic()
    {
        Services.Clock.eventManager.Unregister<Measure>(DynamicLevelMusicVolumes);
        
        if (levelTracksFade != null)
            foreach (var fade in levelTracksFade)
                StopCoroutine(fade);
        
        _previousVolumes = new List<float>();
        var toDestroy = levelMusicHolder;

        foreach (var source in _levelMusicSources)
        {
            _previousVolumes.Add(source.volume);
        }
        
        for (var i = 0; i < _levelMusicSources.Count; i++)
        {
            var toChange = _levelMusicSources[i];
            var startingValue = _previousVolumes[i];
            var newValue = 0.0f;
            StartCoroutine(Coroutines.DoOverEasedTime(Services.Clock.HalfLength(), Easing.Linear,
                t =>
                {
                    var newVolume = Mathf.Lerp(startingValue, newValue, t);
                    if (toChange != null)
                    {
                        toChange.volume = newVolume;
                    }
                }));
        }
        
        Delay(() =>
        {
            Destroy(toDestroy);
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
        foreach (AudioSource source in _levelMusicSources)
        {
            source.volume = 0;
        }
        
        _levelMusicSources[0].volume = BaseMusicVolume;
        
        Services.Clock.ClearEvents();
        StopAllCoroutines();
    }

    public void FadeMainTrack(float duration, bool fadeOut = true)
    {
        if (mainTrack == null)
            SetMainTrack(Services.Clips.MenuSong, 0.0f);
        
        if (mainTrackFade != null)
            StopCoroutine(mainTrackFade);
        
        var starting = fadeOut ? mainTrack.volume : 0;
        var ending = fadeOut ? 0 : BaseMusicVolume;
        mainTrackFade = StartCoroutine(Coroutines.DoOverEasedTime(duration/2, Easing.Linear,
            t =>
            {
                mainTrack.volume = Mathf.Lerp(starting, ending, t);
            }));
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
        mainTrack.mute = !Services.GameManager.MusicEnabled;
        
        foreach (var source in _levelMusicSources)
        {
            source.mute = !Services.GameManager.MusicEnabled;
        }
    }

    public void SetPitch(float pitch)
    {
        foreach (AudioSource source in _levelMusicSources)
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
