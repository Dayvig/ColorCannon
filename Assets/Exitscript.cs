using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Exitscript : MonoBehaviour
{
    public Button nextWave;
    public GameManager manager;

    void Start()
    {
        nextWave.onClick.AddListener(TaskOnClick);
        manager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    void TaskOnClick()
    {
        Application.Quit();
    }
}
