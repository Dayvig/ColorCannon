using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameModel : MonoBehaviour
{

    public enum GameColor
    {
        RED, YELLOW, BLUE, ORANGE, PURPLE, GREEN, WHITE, NONE
    }

    public enum UIColor
    {
        UPGRADESELECTED,
        UPGRADENOTSELECTED
    }

    public List<Sprite> UpgradeImages = new List<Sprite>();

    [Range(1.05f, 10f)] public float shotSpeedMultiplier;
    [Range(1.05f, 10f)] public float rapidFireMultiplier;
    [Range(1.05f, 10f)] public float shotSizeMultiplier;
    [Range(1, 10)] public int numShotsUpgrade;
    [Range(1, 10)] public int piercingUpgrade;
    [Range(20, 180)] 
    public float spreadAngleMax;


    public float darkenedColorDivisor = 1.5f;


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
            case GameColor.ORANGE:
                return new Color(1f, 0.65f, 0.15f);
            case GameColor.GREEN:
                return new Color(0f, 0.65f, 0.15f);
            case GameColor.PURPLE:
                return new Color(0.5f, 0f, 1f);
            case GameColor.WHITE:
                return Color.white;
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
            case GameManager.UpgradeType.PIERCING:
                return UpgradeImages[4];
            case GameManager.UpgradeType.SHIELDS:
                return UpgradeImages[5];
            default:
                return UpgradeImages[3];
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
