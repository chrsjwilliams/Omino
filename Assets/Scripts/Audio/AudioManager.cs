using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager {

    private AudioSource mainTrack;
    private float last_volume = 1.0f;

    public void Init()
    {
        CreateTempAudio(Services.Clips.MainTrackAudio, 0.0f);
    }
    
    public void CreateTempAudio(AudioClip clip, float volume)
    {
        GameObject obj = new GameObject();
        obj.name = "Audio Clip: " + clip.name;
        AudioSource audioSource = obj.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.Play();
        GameObject.Destroy(obj, clip.length);
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
            Services.Clips = Resources.Load<ClipLibrary>("Audio/PreIncubatorClipLibrary");
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
