using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameModel : MonoBehaviour
{

    public enum GameColor
    {
        RED, YELLOW, BLUE
    }

    public enum UIColor
    {
        UPGRADESELECTED,
        UPGRADENOTSELECTED
    }

    public List<Sprite> UpgradeImages = new List<Sprite>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public Color ColorToColor(GameColor col)
    {
        switch (col)
        {
            case GameColor.RED:
                return Color.red;
            case GameColor.BLUE:
                return Color.blue;
            case GameColor.YELLOW:
                return Color.yellow;
            default:
                return Color.gray;
        }
    }

    public Color UItoColor(UIColor u)
    {
        switch (u)
        {
            case UIColor.UPGRADESELECTED:
                return Color.yellow;
            case UIColor.UPGRADENOTSELECTED:
                return Color.white;
            default:
                return Color.gray;
        }
    }

    public Sprite UpgradeImageFromType(GameManager.UpgradeType type)
    {
        switch (type)
        {
            case GameManager.UpgradeType.SHOTS:
                return UpgradeImages[0];
            case GameManager.UpgradeType.SHOTSIZE:
                return UpgradeImages[1];
            case GameManager.UpgradeType.SHOTSPEED:
                return UpgradeImages[2];
            case GameManager.UpgradeType.ATTACKSPEED:
                return UpgradeImages[3];
            default:
                return UpgradeImages[3];
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
