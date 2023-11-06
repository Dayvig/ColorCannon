using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class SoundManager : MonoBehaviour
{

    [Range(0f, 1f)]
    public float masterVolume;

    public static SoundManager instance { get; private set; }

    public SoundManager()
    {
        instance = this;
    }

    public void PlaySound(AudioSource source, AudioClip clip)
    {
        source.clip = clip;
        source.PlayOneShot(clip, masterVolume);
    }

    public void PlaySound(AudioSource source, AudioClip clip, float minPitchOffset, float maxPitchOffset)
    {
        float pitchShift = Random.Range(minPitchOffset, maxPitchOffset);
        source.clip = clip;
        source.pitch *= pitchShift;
        source.PlayOneShot(clip, masterVolume);
        source.pitch /= pitchShift;
    }

    public void PlaySound(AudioSource source, AudioClip clip, float delaySeconds)
    {
        StartCoroutine(PlaySoundWithDelay(source, clip, delaySeconds));
    }

    public void PlaySound(AudioSource source, AudioClip clip, int priority)
    {
        source.clip = clip;
        source.priority = priority;
        source.PlayOneShot(clip, masterVolume);
    }

    IEnumerator PlaySoundWithDelay(AudioSource source, AudioClip clip, float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        PlaySound(source, clip);
    }

}
