using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TitleTextScript : MonoBehaviour
{

    TextMeshProUGUI text;
    public float fadeInTimer = 0.0f;
    public float fadeInInterval = 2f;
    public bool fade = false;
    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        fade = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (fade)
        {
            fadeInTimer += Time.deltaTime;
            text.color = new Color(text.color.r, text.color.g, text.color.b, fadeInTimer / fadeInInterval);
            if (fadeInTimer > fadeInInterval)
            {
                fade = false;
            }
        }
    }
}
