using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseButton : MonoBehaviour
{
    public Button pause;
    public GameManager manager;
    public UIManager uiManager;
    public Image ren;
    public GameObject Menu;
    public GameObject Settings;
    public Player player;

    void Start()
    {
        pause.onClick.AddListener(TaskOnClick);
        manager = GameObject.Find("GameManager").GetComponent<GameManager>();
        uiManager = GameObject.Find("GameManager").GetComponent<UIManager>();
        player = GameObject.Find("Player").GetComponent<Player>();
        ren = GetComponent<Image>();
    }

    void TaskOnClick()
    {
        if (manager.currentState == GameManager.GameState.PAUSED)
        {
            Menu.SetActive(false);
            Settings.SetActive(false);
            manager.SetState(GameManager.GameState.WAVE);
            ren.sprite = GameModel.instance.UpgradeImages[12];
            player.SelectorRing.activated = false;
        }
        else
        {
            Menu.SetActive(true);
            Settings.SetActive(true);
            manager.SetState(GameManager.GameState.PAUSED);
            ren.sprite = GameModel.instance.UpgradeImages[13];
        }
    }
}
