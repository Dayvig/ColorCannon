using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PromodeScript : MonoBehaviour
{

    public Button increaseArrow;
    public Image incArrowImage;
    public Image decArrowImage;
    public Button decreaseArrow;
    public ArrowScript decArrowScript;
    public ArrowScript incArrowScript;
    public TextMeshProUGUI modeText;
    public TextMeshProUGUI reminderText;

    public void initialize()
    {
        if (GameManager.instance.maxProModeLevelUnlocked == 0)
        {
            modeText.color = Color.gray;
            reminderText.color = Color.gray;
            reminderText.text = "Locked - Beat wave 15 to Unlock.";

            increaseArrow.gameObject.SetActive(false);
            decreaseArrow.gameObject.SetActive(false);
        }
        else if (GameManager.instance.promodeLevel == 0)
        {
            modeText.color = Color.black;
            reminderText.color = Color.black;

            increaseArrow.gameObject.SetActive(true);
            decreaseArrow.gameObject.SetActive(false);
            reminderText.text = "Base Difficulty";
            modeText.text = "Pro Mode - " + GameManager.instance.promodeLevel;
        }
        else
        {
            modeText.color = Color.black;
            reminderText.color = Color.black;
            modeText.text = "Pro Mode - " + GameManager.instance.promodeLevel;

            increaseArrow.gameObject.SetActive(true);
            decreaseArrow.gameObject.SetActive(true);

            if (GameManager.instance.promodeLevel == 1)
            {
                reminderText.text = "Makes waves harder with a random modifier.";
            }
            else if (GameManager.instance.promodeLevel == 2)
            {
                reminderText.text = "Makes waves harder with " + GameManager.instance.promodeLevel + " random modifiers.";
            }
            else if (GameManager.instance.promodeLevel < 6)
            {
                reminderText.text = "Makes waves harder with " + GameManager.instance.promodeLevel + " random modifiers. Gain a random upgrade at the start.";
            }
            else
            {
                reminderText.text = "Makes waves harder with " + GameManager.instance.promodeLevel + " random modifiers. Gain " + (int)GameManager.instance.promodeLevel / 3 + " random upgrades at the start.";
            }
        }

        setArrowColor();
    }

    void setArrowColor()
    {
        if (GameManager.instance.maxProModeLevelUnlocked > GameManager.instance.promodeLevel)
        {
            incArrowImage.color = Color.white;
            incArrowScript.active = true;
        }
        else
        {
            incArrowImage.color = Color.gray;
            incArrowScript.active = false;
        }

        decArrowImage.color = Color.white;
        decArrowScript.active = true;
    }
}
