using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeButton : MonoBehaviour
{
    public Button upgrade;
    public GameManager manager;
    public GameManager.Upgrade upp;
    public GameModel model;
    void Start()
    {
        upgrade.onClick.AddListener(TaskOnClick);
        manager = GameObject.Find("GameManager").GetComponent<GameManager>();
        model = GameObject.Find("GameManager").GetComponent<GameModel>();
    }

    public void initialize(GameManager.Upgrade thisUpgrade)
    {
        upp = thisUpgrade;
    }

    void TaskOnClick()
    {
        manager.selectedUpgrade = upp;
        manager.selectedUpgradeType = upp.type;
        for (int i = 0; i < this.transform.parent.childCount; i++)
        {
            transform.parent.GetChild(i).GetComponent<Image>().color =
                model.UItoColor(GameModel.UIColor.UPGRADENOTSELECTED);
        }
        GetComponent<Image>().color = model.UItoColor(GameModel.UIColor.UPGRADESELECTED);
    }
}