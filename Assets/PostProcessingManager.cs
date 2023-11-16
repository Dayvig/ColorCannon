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
    public static PostProcessingManager instance { get; set; }

    public void Awake()
    {
        if (instance != null)
        {
            Debug.Log("Multiple PPManagers Detected.");
        }
        instance = this;

        globalVolume.profile.TryGet(out depth);

    }

    public void SetBlur(bool blurOn)
    {
        depth.active = blurOn;
    }

}
