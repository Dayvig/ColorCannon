using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayGameButton : MonoBehaviour
{
    public Button nextWave;
    public GameManager manager;
    public TextMeshProUGUI playText;
    public TextMeshProUGUI waveText;


    void Start()
    {
        nextWave.onClick.AddListener(TaskOnClick);
        manager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    public void initialize()
    {
        if (WaveSpawningSystem.instance.Level == 0)
        {
            this.gameObject.SetActive(false);
            playText.text = "New Game";
            waveText.gameObject.SetActive(false);
        }
        else
        {
            this.gameObject.SetActive(true);
            playText.text = "Continue";
            waveText.text = "( Wave "+WaveSpawningSystem.instance.Level+", Pro Level "+ GameManager.instance.promodeLevel+" )";
            waveText.gameObject.SetActive(true);

        }
    }

    void TaskOnClick()
    {
        SaveLoadManager.instance.LoadGame();
        WaveSpawningSystem.instance.tutorialStage = -2;
        GameManager.instance.SetState(GameManager.GameState.POSTWAVE);
        PostProcessingManager.instance.SetBlur(false);
    }
}
