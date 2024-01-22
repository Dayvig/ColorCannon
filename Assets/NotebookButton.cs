using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotebookButton : MonoBehaviour
{
    public Button notebook;

    void Start()
    {
        notebook.onClick.AddListener(TaskOnClick);
    }

    void TaskOnClick()
    {
        GameManager.instance.SetState(GameManager.GameState.NOTEBOOK);
    }
}
