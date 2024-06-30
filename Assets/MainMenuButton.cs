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
    public bool playMusic;

    void Awake()
    {
        menuButton.onClick.AddListener(TaskOnClick);
        manager = GameObject.Find("GameManager").GetComponent<GameManager>();
        uiManager = GameObject.Find("GameManager").GetComponent<UIManager>();
    }

    void TaskOnClick()
    {
        manager.SetState(GameManager.GameState.MAINMENU);
        UIManager.instance.activateMainMenuAnimations(true);
        GameManager.instance.arena = GameManager.instance.lastValidArena;
        UIManager.instance.setArenaImage();
        pauseButton.HideUI();
        SaveLoadManager.instance.SaveUnlocks();
        SaveLoadManager.instance.LoadUnlocks();
        UIManager.instance.activateNotebookUI(false);
        uiManager.titleTextScript.Reset();
        uiManager.playButton.initialize();
        if (playMusic)
        {
            SoundManager.instance.mainMusic.Stop();
            SoundManager.instance.PlayMusicAndLoop(SoundManager.instance.mainMusic, GameModel.instance.music[0]);
        }
    }
}
