using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class settingsButton : MonoBehaviour
{
    public Button settings;
    public GameManager manager;
    public UIManager uiManager;

    void Start()
    {
        settings.onClick.AddListener(TaskOnClick);
        manager = GameObject.Find("GameManager").GetComponent<GameManager>();
        uiManager = GameObject.Find("GameManager").GetComponent<UIManager>();
    }

    void TaskOnClick()
    {
        GameManager.instance.SetState(GameManager.GameState.SETTINGS);
    }
}