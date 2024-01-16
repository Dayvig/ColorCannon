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
    [Range(0f, 1f)]
    public float sfxVolume;

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
        source.volume = masterVolume * sfxVolume;
        source.PlayOneShot(clip, masterVolume*sfxVolume);
    }

    public void PlaySFX(AudioSource source, AudioClip clip, float minPitchOffset, float maxPitchOffset)
    {
        float pitchShift = Random.Range(minPitchOffset, maxPitchOffset);
        source.clip = clip;
        source.pitch *= pitchShift;
        source.PlayOneShot(clip, masterVolume*sfxVolume);
        source.pitch /= pitchShift;
    }

    public void PlaySFX(AudioSource source, AudioClip clip, float delaySeconds)
    {
        source.volume = masterVolume * sfxVolume;
        StartCoroutine(PlaySFXWithDelay(source, clip, delaySeconds));
    }

    public void PlaySFX(AudioSource source, AudioClip clip, int priority)
    {
        source.clip = clip;
        source.priority = priority;
        source.volume = masterVolume * sfxVolume;
        source.PlayOneShot(clip, masterVolume * sfxVolume);
    }

    public void PlayMusicAndLoop(AudioSource source, AudioClip clip)
    {
        source.clip = clip;
        source.loop = true;
        source.volume = masterVolume * musicVolume;
        source.Play();
    }

    public void PlayRandomMainTheme()
    {
        SoundManager.instance.mainMusic.Stop();
        PlayMusicAndLoop(mainMusic, GameModel.instance.music[Random.Range(0, 4) + 1]);
    }

    IEnumerator PlaySFXWithDelay(AudioSource source, AudioClip clip, float delaySeconds)
    {
        source.volume = masterVolume * sfxVolume;
        yield return new WaitForSeconds(delaySeconds);
        PlaySFX(source, clip);
    }

    public void CalculateMusicVolume()
    {
        mainMusic.volume = masterVolume * musicVolume;
    }

}
