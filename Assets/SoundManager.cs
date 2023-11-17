using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class SoundManager : MonoBehaviour
{

    [Range(0f, 1f)]
    public float masterVolume;
    [Range(0f, 1f)]
    public float musicVolume;

    public AudioSource mainMusic;
    public bool mainMusicPlaying = false;

    public static SoundManager instance { get; private set; }

    public SoundManager()
    {
        instance = this;
    }

    public void PlaySFX(AudioSource source, AudioClip clip)
    {
        source.clip = clip;
        source.PlayOneShot(clip, masterVolume);
    }

    public void PlaySFX(AudioSource source, AudioClip clip, float minPitchOffset, float maxPitchOffset)
    {
        float pitchShift = Random.Range(minPitchOffset, maxPitchOffset);
        source.clip = clip;
        source.pitch *= pitchShift;
        source.PlayOneShot(clip, masterVolume);
        source.pitch /= pitchShift;
    }

    public void PlaySFX(AudioSource source, AudioClip clip, float delaySeconds)
    {
        StartCoroutine(PlaySFXWithDelay(source, clip, delaySeconds));
    }

    public void PlaySFX(AudioSource source, AudioClip clip, int priority)
    {
        source.clip = clip;
        source.priority = priority;
        source.PlayOneShot(clip, masterVolume);
    }

    public void PlayMusicAndLoop(AudioSource source, AudioClip clip)
    {
        source.clip = clip;
        source.loop = true;
        source.volume = masterVolume * musicVolume;
        source.Play();
    }

    IEnumerator PlaySFXWithDelay(AudioSource source, AudioClip clip, float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        PlaySFX(source, clip);
    }

}
