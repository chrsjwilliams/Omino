using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager {

    private AudioSource mainTrack;

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
        mainTrack.loop = true;
        mainTrack.Play();
    }
}
