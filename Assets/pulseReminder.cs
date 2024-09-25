using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pulseReminder : MonoBehaviour
{
    public float reminderPulseTimer = 0.0f;
    public float pointInterval = 0.1f;
    public float pointTimer = 0.0f;
    public float pulseInterval = 0.8f;
    public bool active = false;
    public bool reverse = false;
    public SpriteRenderer ren;
    public GameObject pointer;
    public void Initialize()
    {
        this.gameObject.SetActive(true);
        pointer.gameObject.SetActive(true);
        reminderPulseTimer = 0.0f;
        pointTimer = -(pointInterval * 4);
        active = true;
        GameManager.instance.rainbowRushRemindersShown++;
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
        pointer.gameObject.SetActive(false);
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
            pointTimer += Time.deltaTime;
            if (pointTimer < pointInterval)
            {
                pointer.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, 30f, pointTimer / pointInterval));
            }
            else if (pointTimer < pointInterval*2)
            {
                pointer.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(30f, 0f, (pointTimer-pointInterval) / pointInterval));
            }
            else if (pointTimer < pointInterval*3)
            {
                pointer.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, 30f, (pointTimer - (pointInterval * 2)) / pointInterval));
            }
            else if (pointTimer < pointInterval*4)
            {
                pointer.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(30f, 0f, (pointTimer - (pointInterval * 3)) / pointInterval));
            }
            else if (pointTimer >= pointInterval*30)
            {
                pointTimer = 0.0f;
            }
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
