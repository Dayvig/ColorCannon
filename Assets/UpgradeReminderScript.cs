using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeReminderScript : MonoBehaviour
{
    public bool active = false;
    private float flashTimer = 0.0f;
    private float flashInterval = 0.5f;
    public Image thisImage;
    public AudioSource buttonSource;
    public void Flash()
    {
        active = true;
        thisImage.color = new Color(thisImage.color.r, thisImage.color.g, thisImage.color.b, 1f);
        this.gameObject.SetActive(true);
        SoundManager.instance.PlaySFX(buttonSource, GameModel.instance.enemySounds[0]);
        flashTimer = 0.0f;
    }

    void Update()
    {
        if (active)
        {
            flashTimer += Time.deltaTime;
            thisImage.color = new Color(thisImage.color.r, thisImage.color.g, thisImage.color.b, Mathf.Lerp(1f, 0f, flashTimer / flashInterval));
            if (flashTimer > flashInterval)
            {
                this.gameObject.SetActive(false);
                this.active = false;
            }
        }
    }
}
