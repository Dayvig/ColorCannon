using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MenuRainbowInkDisplay : MonoBehaviour
{
    public TextMeshProUGUI rainbowInkText;
    private float rainbowInkColorTimer = 0.0f;
    private float rainbowInkColorInterval = 1.2f;
    public Color transitionColor;

    public void Update()
    {
        rainbowInkText.text = "" + SaveLoadManager.instance.getRainbowInk();
        if (rainbowInkColorTimer >= 0.0f)
        {
            rainbowInkColorTimer -= Time.deltaTime;
            rainbowInkText.color = Color.Lerp(transitionColor, Color.black, rainbowInkColorInterval-rainbowInkColorTimer / rainbowInkColorInterval);
        }
    }

    public void SetTextColorTransition(Color c)
    {
        transitionColor = c;
        rainbowInkColorTimer = rainbowInkColorInterval;
    }

    

}
