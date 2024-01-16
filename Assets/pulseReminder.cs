using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pulseReminder : MonoBehaviour
{
    public float reminderPulseTimer = 0.0f;
    public float pulseInterval = 0.8f;
    public bool active = false;
    public bool reverse = false;
    public SpriteRenderer ren;
    public void Initialize()
    {
        this.gameObject.SetActive(true);
        reminderPulseTimer = 0.0f;
        active = true;
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
        active = false;
    }

    private void Update()
    {
        if (GameManager.instance.player.rainbowRush && active)
        {
            Hide();
        }
        if (active)
        {
            if (reverse)
            {
                reminderPulseTimer -= Time.deltaTime;
                if (reminderPulseTimer < 0.0f)
                {
                    reverse = !reverse;
                }
            }
            else
            {
                reminderPulseTimer += Time.deltaTime;
                if (reminderPulseTimer >= pulseInterval)
                {
                    reverse = !reverse;
                }
            }
            ren.color = new Color(ren.color.r, ren.color.g, ren.color.b, Mathf.Lerp(0f, 0.8f, reminderPulseTimer / pulseInterval));
        }
    }
}
