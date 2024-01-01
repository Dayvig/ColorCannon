using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class enableTutorial : MonoBehaviour
{
    public Toggle tutorialToggle;

    void Start()
    {
        tutorialToggle.onValueChanged.AddListener(delegate {
            ToggleValueChanged(tutorialToggle);
        });
    }

    public void init()
    {
        tutorialToggle.isOn = (WaveSpawningSystem.instance.tutorialStage == 0);
    }
    void ToggleValueChanged(Toggle change)
    {
        SoundManager.instance.PlaySFX(GameManager.instance.gameAudio, GameModel.instance.uiSounds[0]);
        WaveSpawningSystem.instance.tutorialStage = tutorialToggle.isOn ? 0 : -2;
        SaveLoadManager.instance.SaveSettings();
    }
}
