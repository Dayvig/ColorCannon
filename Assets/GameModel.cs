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
    public List<Sprite> WaveModImages = new List<Sprite>();
    public List<string> WaveModTexts = new List<string>();
    public List<string> ChunkTexts = new List<string>();

    [Range(1.05f, 10f)] public float shotSpeedMultiplier;
    [Range(1.05f, 10f)] public float rapidFireMultiplier;
    [Range(1.05f, 10f)] public float shotSizeMultiplier;
    [Range(1, 10)] public int numShotsUpgrade;
    [Range(1, 10)] public int piercingUpgrade;
    [Range(20, 180)] 
    public float spreadAngleMax;
    public int baseGlobalWaveNumber;
    public float baseGlobalWaveSpacing;
    public float baseTutorialSpacing;
    public float baseGlobalWaveSpeed;
    public int baseNumChunks;
    public int baseNumUniqueChunks;

    public float darkenedColorDivisor = 1.5f;

    public Color redVisualColor = Color.red;
    public Color greenVisualColor = new Color(0f, 0.65f, 0.15f);
    public Color blueVisualColor = Color.blue;
    public Color yellowVisualColor = Color.yellow;
    public Color orangeVisualColor = new Color(1f, 0.65f, 0.15f);
    public Color purpleVisualColor = new Color(0.5f, 0f, 1f);
    public Color whiteVisualColor = Color.white;

    public int CHARACTERMAX = 68;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    public Color ColorToColor(GameColor col)
    {
        switch (col)
        {
            case GameColor.RED:
                return redVisualColor;
            case GameColor.BLUE:
                return blueVisualColor;
            case GameColor.YELLOW:
                return yellowVisualColor;
            case GameColor.ORANGE:
                return orangeVisualColor;
            case GameColor.GREEN:
                return greenVisualColor;
            case GameColor.PURPLE:
                return purpleVisualColor;
            case GameColor.WHITE:
                return whiteVisualColor;
            default:
                return Color.gray;
        }
    }

    public Color OppositeColor(GameColor col)
    {
        switch (col)
        {
            case GameColor.RED:
                return greenVisualColor;
            case GameColor.BLUE:
                return orangeVisualColor;
            case GameColor.YELLOW:
                return purpleVisualColor;
            case GameColor.ORANGE:
                return blueVisualColor;
            case GameColor.GREEN:
                return redVisualColor;
            case GameColor.PURPLE:
                return yellowVisualColor;
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

    public Sprite WaveModImageFromType(UIManager.WaveModifier waveModifier)
    {
        switch (waveModifier)
        {
            case UIManager.WaveModifier.DIFFICULT:
                return WaveModImages[0];
            case UIManager.WaveModifier.FASTER:
                return WaveModImages[1];
            case UIManager.WaveModifier.NUMEROUS:
                return WaveModImages[2];
            case UIManager.WaveModifier.BIGGER_WAVE:
                return WaveModImages[3];
            default:
                return WaveModImages[0];
        }

    }

    public string GetWaveModTextFromType(UIManager.WaveModifier waveModifier)
    {
        switch (waveModifier)
        {
            case UIManager.WaveModifier.DIFFICULT:
                return WaveModTexts[0];
            case UIManager.WaveModifier.FASTER:
                return WaveModTexts[1];
            case UIManager.WaveModifier.NUMEROUS:
                return WaveModTexts[2];
            case UIManager.WaveModifier.BIGGER_WAVE:
                return WaveModTexts[3];
            default:
                return "Huh";
        }
    }

    public string GetChunkTextFromType(WaveSpawningSystem.Chunk thisChunk)
    {
        string returnText = "";
        switch (thisChunk.name)
        {
            case "Basic":
                returnText = ChunkTexts[0];
                    break;
            case "Fast":
                returnText = ChunkTexts[1];
                break;
            case "Ninja":
                returnText = ChunkTexts[2];
                break;
            case "Swarm":
                returnText = ChunkTexts[3];
                break;
            case "Disguiser":
                returnText = ChunkTexts[4];
                break;
            case "Swirl":
                returnText = ChunkTexts[5];
                break;
            case "ZigZag":
                returnText = ChunkTexts[6];
                break;
            default:
                return "Something went wrong";
        }
        returnText += "\n"+"Colors: ";
        for (int i = 0; i < thisChunk.colors.Length; i++)
        {
            if (thisChunk.isDarkened)
            {
                returnText += "Dark ";
            }
            returnText += thisChunk.colors[i];
            if (i != thisChunk.colors.Length - 1)
                returnText += ", ";
        }
        return returnText;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
