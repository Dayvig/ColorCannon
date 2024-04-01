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
    public bool isMuted;

    public static SoundManager instance { get; private set; }

    void Awake()
    {
        instance = this;
    }

    public void PlaySFX(AudioSource source, AudioClip clip)
    {
        if (!isMuted)
        {
            //source.pitch = 1;
            source.clip = clip;
            source.volume = masterVolume * sfxVolume;
            source.PlayOneShot(clip, masterVolume * sfxVolume);
        }
    }

    public void PlaySFX(AudioSource source, AudioClip clip, bool noChangeBasePitch)
    {
        if (!isMuted)
        {
            source.clip = clip;
            source.volume = masterVolume * sfxVolume;
            source.PlayOneShot(clip, masterVolume * sfxVolume);
        }
    }

    public void MuteAudio()
    {
        isMuted = true;
        SoundManager.instance.mainMusic.mute = isMuted;
    }

    public void UnMuteAudio()
    {
        isMuted = false;
        SoundManager.instance.mainMusic.mute = isMuted;
    }

    public void PlaySFX(AudioSource source, AudioClip clip, float minPitchOffset, float maxPitchOffset)
    {
        if (!isMuted)
        {
            float pitchShift = Random.Range(1+minPitchOffset, 1+maxPitchOffset);
            source.clip = clip;
            source.pitch = pitchShift;
            source.PlayOneShot(clip, masterVolume * sfxVolume);
        }
    }

    public void PlayBulletSFX(AudioSource source, AudioClip clip)
    {
        if (!isMuted)
        {
            source.clip = clip;
            source.pitch = 0.65f;
            source.PlayOneShot(clip, masterVolume * sfxVolume);
        }
    }

    public void PlaySFX(AudioSource source, AudioClip clip, float delaySeconds)
    {
        if (!isMuted)
        {
            source.volume = masterVolume * sfxVolume;
            StartCoroutine(PlaySFXWithDelay(source, clip, delaySeconds));
        }
    }

    public void PlaySFX(AudioSource source, AudioClip clip, int priority)
    {
        if (!isMuted)
        {
            source.clip = clip;
            source.priority = priority;
            source.volume = masterVolume * sfxVolume;
            source.PlayOneShot(clip, masterVolume * sfxVolume);
        }
    }

    public void PlayMusicAndLoop(AudioSource source, AudioClip clip)
    {
            source.clip = clip;
            source.loop = true;
            source.volume = masterVolume * musicVolume;
            source.Play();
            SoundManager.instance.mainMusic.mute = isMuted;
    }

    public void PlayRandomMainTheme()
    {
        SoundManager.instance.mainMusic.Stop();
        PlayMusicAndLoop(mainMusic, GameModel.instance.music[Random.Range(0, 4) + 1]);
        SoundManager.instance.mainMusic.mute = isMuted;
    }

    IEnumerator PlaySFXWithDelay(AudioSource source, AudioClip clip, float delaySeconds)
    {
        if (!isMuted)
        {
            source.volume = masterVolume * sfxVolume;
            yield return new WaitForSeconds(delaySeconds);
            PlaySFX(source, clip, true);
        }
    }

    public void CalculateMusicVolume()
    {
        mainMusic.volume = masterVolume * musicVolume;
    }

}
