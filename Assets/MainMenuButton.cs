using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuButton : MonoBehaviour
{
    public Button menuButton;
    public GameManager manager;
    public UIManager uiManager;
    public PauseButton pauseButton;

    void Start()
    {
        menuButton.onClick.AddListener(TaskOnClick);
        manager = GameObject.Find("GameManager").GetComponent<GameManager>();
        uiManager = GameObject.Find("GameManager").GetComponent<UIManager>();
    }

    void TaskOnClick()
    {
        manager.SetState(GameManager.GameState.MAINMENU);
        pauseButton.HideUI();
    }
}
