using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class refreshUpgrades : MonoBehaviour
{
    public Button refresh;
    public GameManager manager;
    public UIManager uiManager;
    
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
        manager.GenerateUpgrades();
        this.gameObject.SetActive(false);
        UIManager.instance.SetUpgradesVisible(true);
        UIManager.instance.refreshActive = false;
        SaveLoadManager.instance.SaveGame();
    }
}
