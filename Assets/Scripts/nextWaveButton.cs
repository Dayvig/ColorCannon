using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class nextWaveButton : MonoBehaviour
{
    public Button nextWave;
    public GameManager manager;
    
    void Start()
    {
        nextWave.onClick.AddListener(TaskOnClick);
        manager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    void TaskOnClick()
    {
        if (WaveSpawningSystem.instance.Level > 15)
        {
            GameManager.instance.SetState(GameManager.GameState.WIN);
            Debug.Log("Win");
        }
        else
        {
            GameManager.instance.SetState(GameManager.GameState.WAVE);
        }
    }   
}
