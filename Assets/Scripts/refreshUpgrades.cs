using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class refreshUpgrades : MonoBehaviour
{
    public Button refresh;
    public GameManager manager;
    public UIManager uiManager;
    public bool infinite;

    void Start()
    {
        refresh.onClick.AddListener(TaskOnClick);
        manager = GameObject.Find("GameManager").GetComponent<GameManager>();
        uiManager = GameObject.Find("GameManager").GetComponent<UIManager>();
    }

    void TaskOnClick()
    {
        UIManager.instance.WipeUpgrades();
        manager.currentOfferedUpgrades.Clear();
        if (WaveSpawningSystem.instance.Level % 2 == 0)
        {
            manager.GenerateSpecialUpgrades();
        }
        else
        {
            manager.GenerateUpgrades();
        }
        UIManager.instance.SetUpgradesVisible(true);
        if (!infinite)
        {
            this.gameObject.SetActive(false);
            UIManager.instance.refreshActive = false;
        }
        SaveLoadManager.instance.SaveGame();
    }
}
