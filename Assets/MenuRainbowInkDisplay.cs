using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MenuRainbowInkDisplay : MonoBehaviour
{
    public TextMeshProUGUI rainbowInkText;

    public void Update()
    {
        rainbowInkText.text = "" + SaveLoadManager.instance.getRainbowInk();
    }


}
