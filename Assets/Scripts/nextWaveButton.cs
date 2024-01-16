using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class nextWaveButton : MonoBehaviour
{
    public Button nextWave;
    public GameManager manager;
    public UpgradeReminderScript reminder;
    private int pressed = 0;
    
    void Start()
    {
        nextWave.onClick.AddListener(TaskOnClick);
        manager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    void TaskOnClick()
    {
        if (WaveSpawningSystem.instance.Level > 15)
        {
            GameManager.instance.Win();
            GameManager.instance.SetState(GameManager.GameState.WIN);
            Debug.Log("Win");
        }
        else
        {
            if (manager.selectedUpgrade.type.Equals(GameManager.UpgradeType.NONE) && pressed == 0 && WaveSpawningSystem.instance.Level != 1)
            {
                reminder.Flash();
                pressed++;
            }
            else
            {
                GameManager.instance.SetState(GameManager.GameState.WAVE);
                pressed = 0;
            }
        }
    }   
}
