using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsData
{
    public float masterVolume;
    public float musicVolume;
    public float sfxVolume;
    public bool doubletapcycle;
    public float splatters;
    public bool tutorialOn;
    public bool muted;

    public SettingsData()
    {
        splatters = 1f;
        doubletapcycle = true;
        masterVolume = 0.5f;
        musicVolume = 0.5f;
        sfxVolume = 0.5f;
        tutorialOn = true;
        muted = false;
    }

}
