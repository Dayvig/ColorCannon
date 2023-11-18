using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuButton : MonoBehaviour
{
    public Button pause;
    public GameManager manager;
    public UIManager uiManager;

    void Start()
    {
        pause.onClick.AddListener(TaskOnClick);
        manager = GameObject.Find("GameManager").GetComponent<GameManager>();
        uiManager = GameObject.Find("GameManager").GetComponent<UIManager>();
    }

    void TaskOnClick()
    {
        manager.SetState(GameManager.GameState.MAINMENU);
    }
}
