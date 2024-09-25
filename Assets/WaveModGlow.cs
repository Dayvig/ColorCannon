using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaveModGlow : MonoBehaviour
{
    public Image glow;
    public float glowTimer = 0.0f;
    bool reverse = false;
    public RectTransform parRect;
    public RectTransform thisRect;

    void Awake()
    {
        parRect = GetComponent<RectTransform>();
        thisRect = glow.gameObject.GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        glowTimer += reverse ? (Time.deltaTime / 2) * -1 : (Time.deltaTime / 2);
        if (reverse ? glowTimer <= 0f : glowTimer >= 1f)
        {
            reverse = !reverse;
        }
        glow.color = new Color(1f, 1f, glowTimer, 0.8f);
        thisRect.sizeDelta = parRect.sizeDelta * 1.1f;

    }
}
