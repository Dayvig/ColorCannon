using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class returnButton : MonoBehaviour
{
    public Button returnB;
    public GameManager manager;
    public UIManager uiManager;

    void Start()
    {
        returnB.onClick.AddListener(TaskOnClick);
        manager = GameObject.Find("GameManager").GetComponent<GameManager>();
        uiManager = GameObject.Find("GameManager").GetComponent<UIManager>();
    }

    void TaskOnClick()
    {
        GameManager.instance.SetState(GameManager.instance.returnState);
        SaveLoadManager.instance.SaveGame();
    }
}