using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager {

    private AudioSource mainTrack;
    private float last_volume;

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
        if(mainTrack == null)
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
        Services.GameManager.soundEffectsEnabled = !Services.GameManager.soundEffectsEnabled;
        
        if (Services.GameManager.soundEffectsEnabled)
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
        Services.GameManager.musicEnabled = !Services.GameManager.musicEnabled;
        
        if (Services.GameManager.musicEnabled)
        {
            mainTrack.volume = last_volume;
        }
        else
        {
            mainTrack.volume = 0.0f;
        }
    }
}
