using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotebookArrowScript : MonoBehaviour
{
    public Button arrow;
    public bool increase;
    public bool active;
    public GameObject incArrow;
    public GameObject decArrow;

    void Start()
    {
        arrow.onClick.AddListener(TaskOnClick);
    }

    void TaskOnClick()
    {
        if (active)
        {
           if (increase && GameManager.instance.arena < GameModel.instance.arenaImages.Count - 1)
            {
                GameManager.instance.arena++;
                if (SaveLoadManager.instance.getUnlockedArenas().Contains(GameManager.instance.arena))
                {
                    GameManager.instance.lastValidArena = GameManager.instance.arena;
                }
                UIManager.instance.setArenaImage();
            }
            else if (GameManager.instance.arena > 0)
            {
                GameManager.instance.arena--;
                if (SaveLoadManager.instance.getUnlockedArenas().Contains(GameManager.instance.arena))
                {
                    GameManager.instance.lastValidArena = GameManager.instance.arena;
                }
                UIManager.instance.setArenaImage();
            }
        }
        else
        {
            //play bad noise
        }
        setupArrows();
    }

    void setupArrows()
    {
        if (GameManager.instance.arena <= 0)
        {
            incArrow.gameObject.SetActive(true);
            decArrow.gameObject.SetActive(false);
        }
        else if (GameManager.instance.arena >= GameModel.instance.arenaImages.Count-1)
        {
            incArrow.gameObject.SetActive(false);
            decArrow.gameObject.SetActive(true);
        }
        else
        {
            incArrow.gameObject.SetActive(true);
            decArrow.gameObject.SetActive(true);
        }
        UIManager.instance.unlockButton.setupUnlockButton();
    }
}