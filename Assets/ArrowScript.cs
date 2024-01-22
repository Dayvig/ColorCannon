using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArrowScript : MonoBehaviour
{
    public Button arrow;
    public bool increase;
    public GameManager manager;
    public UIManager uiManager;
    public bool active = false;
    public PromodeScript promode;
    public AudioSource thisSource;

    void Start()
    {
        arrow.onClick.AddListener(TaskOnClick);
        manager = GameObject.Find("GameManager").GetComponent<GameManager>();
        uiManager = GameObject.Find("GameManager").GetComponent<UIManager>();
    }

    void TaskOnClick()
    {
        if (active)
        {
            if (increase)
            {
                GameManager.instance.promodeLevel++;
            }
            else
            {
                GameManager.instance.promodeLevel--;
            }
            promode.initialize();
        }
        else
        {
            SoundManager.instance.PlaySFX(thisSource, GameModel.instance.uiSounds[5], -0.5f, 0.5f);
        }
    }
}