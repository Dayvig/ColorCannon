using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RainbowInkScript : MonoBehaviour
{
    float fadeOutTimer = 0.0f;
    public float fadeOutInterval;
    public TextMeshProUGUI rainbowInkText;
    public Image ren;
    public bool visible;
    public Player p;
    public void Appear()
    {
        ren.color = new Color(1f, 1f, 1f, 0.7f);
        rainbowInkText.color = new Color(0f, 0f, 0f, 0.7f);

        if (!visible)
        {
            visible = true;
        }
        fadeOutTimer = 0.0f;
    }

    public void Update()
    {
        rainbowInkText.text = ""+GameManager.instance.rainbowInk;
        if (visible)
        {
            fadeOutTimer += Time.deltaTime;
            ren.color = new Color(1f, 1f, 1f, Mathf.Lerp(0.7f, 0f, fadeOutTimer / fadeOutInterval));
            rainbowInkText.color = new Color(0f, 0f, 0f, Mathf.Lerp(0.7f, 0f, fadeOutTimer / fadeOutInterval));
            if (fadeOutTimer > fadeOutInterval)
            {
                visible = false;
            }
        }
    }

}
