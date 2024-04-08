using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.UI;

public class UnlockTextScript : MonoBehaviour
{
    public TextMeshProUGUI thisText;
    public float textFadeTimer = 0.0f;
    public float textFadeDuration = 4.0f;
    public Color textColor;

    // Update is called once per frame
    void Update()
    {
        if (textFadeTimer > 0.0f)
        {
            textFadeTimer -= Time.deltaTime;
            thisText.color = new Color(textColor.r, textColor.g, textColor.b, (textFadeTimer / textFadeDuration));
        }
        else
        {
            thisText.color = new Color(textColor.r, textColor.g, textColor.b, 0f);
        }
    }

    public void flash()
    {
        textFadeTimer = textFadeDuration;
    }
}
