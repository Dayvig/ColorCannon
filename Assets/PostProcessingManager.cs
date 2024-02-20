using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessingManager : MonoBehaviour
{

    public Volume globalVolume;
    private DepthOfField depth;
    private Vignette rainbowVignette;
    private ChromaticAberration rainbowChrom;
    private LiftGammaGain rainbowLGG;

    private float rainbowFlashTimer = 0.0f;
    public float rainbowFlashInterval = 0.05f;

    public static PostProcessingManager instance { get; set; }

    public void Awake()
    {
        if (instance != null)
        {
            Debug.Log("Multiple PPManagers Detected.");
        }
        instance = this;

        globalVolume.profile.TryGet(out depth);
        globalVolume.profile.TryGet(out rainbowVignette);
        globalVolume.profile.TryGet(out rainbowChrom);
        globalVolume.profile.TryGet(out rainbowLGG);

    }

    private void Update()
    {
        if (rainbowLGG.active)
        {
            rainbowFlashTimer += Time.deltaTime;
            if (rainbowFlashTimer >= rainbowFlashInterval)
            {
                rainbowLGG.gamma.Override(new Vector4(UnityEngine.Random.Range(0.2f, 1f), UnityEngine.Random.Range(0.2f, 1f), UnityEngine.Random.Range(0.2f, 1f), 0.2f));
                rainbowFlashTimer -= rainbowFlashInterval;
            }
        }
    }

    public void SetBlur(bool blurOn)
    {
        depth.active = blurOn;
    }

    public void SetRainbowRush(bool rainbowActive)
    {
        rainbowChrom.active = rainbowActive;
        rainbowVignette.active = rainbowActive;
        rainbowLGG.active = rainbowActive;
    }

}
