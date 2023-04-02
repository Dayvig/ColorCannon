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
        uiManager.WipeUpgrades();
        manager.GenerateUpgrades();
    }
}
