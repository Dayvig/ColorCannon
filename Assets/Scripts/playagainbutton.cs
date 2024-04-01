using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class playagainbutton : MonoBehaviour
{
    public Button again;
    public GameManager manager;
    public UIManager uiManager;
    
    void Awake()
    {
        again.onClick.AddListener(TaskOnClick);
        manager = GameObject.Find("GameManager").GetComponent<GameManager>();
        uiManager = GameObject.Find("GameManager").GetComponent<UIManager>();
    }

    void TaskOnClick()
    {
        manager.PlayAgain();
    }
}