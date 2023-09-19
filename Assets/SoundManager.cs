using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    
    public static SoundManager instance { get; private set; }

    public SoundManager()
    {
        instance = this;
    }

    public void PlaySound(AudioSource source, AudioClip clip)
    {
        source.clip = clip;
        source.PlayOneShot(clip);
    }

    public void PlaySound(AudioSource source, AudioClip clip, float delaySeconds)
    {
        StartCoroutine(PlaySoundWithDelay(source, clip, delaySeconds));
    }

    public void PlaySound(AudioSource source, AudioClip clip, int priority)
    {
        source.clip = clip;
        source.priority = priority;
        source.PlayOneShot(clip);
    }

    IEnumerator PlaySoundWithDelay(AudioSource source, AudioClip clip, float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        PlaySound(source, clip);
    }

}
