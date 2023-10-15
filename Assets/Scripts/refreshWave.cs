using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class refreshWave : MonoBehaviour
{
    public Button refresh;
    public GameManager manager;
    public UIManager uiManager;
    public WaveSpawningSystem spawningSystem;
    
    void Start()
    {
        refresh.onClick.AddListener(TaskOnClick);
        manager = GameObject.Find("GameManager").GetComponent<GameManager>();
        uiManager = GameObject.Find("GameManager").GetComponent<UIManager>();
        spawningSystem = GameObject.Find("GameManager").GetComponent<WaveSpawningSystem>();
    }

    void TaskOnClick()
    {
        uiManager.HideWave();
        WaveSpawningSystem.currentChunks.Clear();
        spawningSystem.SetupNextWave();
    }
}