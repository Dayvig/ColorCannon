using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnlockButton : MonoBehaviour
{
    public Button unlock;
    public TextMeshProUGUI rainbowCost;
    public AudioSource thisSource;
    void Start()
    {
        unlock.onClick.AddListener(TaskOnClick);
    }

    void TaskOnClick()
    {
        if (SaveLoadManager.instance.getRainbowInk() >= GameModel.instance.arenaCosts[GameManager.instance.arena])
        {
            //purchase fanfare
            SoundManager.instance.PlaySFX(thisSource, GameModel.instance.bulletSounds[5]);

            SaveLoadManager.instance.spendRainbowInk(GameModel.instance.arenaCosts[GameManager.instance.arena]);
            SaveLoadManager.instance.unlockArena(GameManager.instance.arena);
            GameManager.instance.lastValidArena = GameManager.instance.arena;
            setupUnlockButton();
        }
        else
        {
            SoundManager.instance.PlaySFX(thisSource, GameModel.instance.uiSounds[5], -0.5f, 0.5f);
        }
    }

    public void setupUnlockButton()
    {
        this.gameObject.SetActive(!SaveLoadManager.instance.getUnlockedArenas().Contains(GameManager.instance.arena));
        rainbowCost.text = ""+GameModel.instance.arenaCosts[GameManager.instance.arena];
        rainbowCost.color = (GameModel.instance.arenaCosts[GameManager.instance.arena] >= GameManager.instance.rainbowInk) ? Color.red : Color.yellow;
    }
}
