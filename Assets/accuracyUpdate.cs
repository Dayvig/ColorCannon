using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class accuracyUpdate : MonoBehaviour
{


    public TextMeshProUGUI thisText;
    public float accuracy;

    // Update is called once per frame
    void Update()
    {
        if (GameManager.instance.shotsFired != 0)
        {
            accuracy = (float)GameManager.instance.shotsHit / (float)GameManager.instance.shotsFired * 100f;
            thisText.text = "Player Accuracy: " + Mathf.Round(accuracy) + "%";
        }
    }
}
