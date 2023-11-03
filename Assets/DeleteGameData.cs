using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeleteGameData : MonoBehaviour
{
    public Button refresh;

    void Start()
    {
        refresh.onClick.AddListener(TaskOnClick);
    }

    void TaskOnClick()
    {
        SaveLoadManager.instance.WipeAllData();
    }
}