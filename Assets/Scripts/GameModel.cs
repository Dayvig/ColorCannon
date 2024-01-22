using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public class GameModel : MonoBehaviour
{

    public enum GameColor
    {
        RED, YELLOW, BLUE, ORANGE, PURPLE, GREEN, WHITE, NONE
    }

    public GameColor gameColors;

    public enum UIColor
    {
        UPGRADESELECTED,
        UPGRADENOTSELECTED
    }

    public List<Sprite> UpgradeImages = new List<Sprite>();
    public List<Sprite> WaveModImages = new List<Sprite>();
    public List<Sprite> bulletImages = new List<Sprite>();
    public List<Sprite> arenaImages = new List<Sprite>();
    public List<int> arenaCosts = new List<int>();
    public List<Sprite> giblets = new List<Sprite>();
    public List<string> WaveModTexts = new List<string>();
    public List<string> ChunkTexts = new List<string>();
    public List<string> UpgradeTexts = new List<string>();

    [Range(1.05f, 10f)] public float shotSpeedMultiplier;
    [Range(1.05f, 10f)] public float rapidFireMultiplier;
    [Range(1.05f, 10f)] public float shotSizeMultiplier;
    [Range(1, 10)] public int numShotsUpgrade;
    [Range(1, 10)] public int piercingUpgrade;
    [Range(10, 180)] 
    public float baseSpreadAngle;
    [Range(0.05f, 1f)] public float shieldPulseInc;

    public int baseGlobalWaveNumber;
    public float baseGlobalWaveSpacing;
    public float baseTutorialSpacing;
    public float baseGlobalWaveSpeed;
    public float baseGlobalRainbowMult;
    public int baseNumChunks;
    public int baseNumUniqueChunks;
    public int shieldUpgradeFreq = 9;
    public int basicUpgradeFreq = 3;
    [Range (5, 50)]
    public int combinerBaseChance = 10;
    [Range(1f, 10f)]
    public float shieldPulseRadius;
    [Range(1f, 10f)]
    public float rocketInterval;
    [Range(0.1f, 1f)]
    public float rocketBarrageUpg;
    [Range(0.1f, 2f)]
    public float meterMultInc;

    public float darkenedColorDivisor = 1.5f;

    [Range(1f, 1.2f)]
    public float baseWaveSpacingUpgrade = 1.03f;

    [Range(1.1f, 1.5f)]
    public float baseNumerousSpacingUpgrade = 1.25f;

    [Range(0, 10)]
    public int baseNumerousNumberUpgrade = 3;

    [Range(1.1f, 1.5f)]
    public float baseWaveSpeedUpgrade = 1.2f;

    [Range(1.1f, 3f)]
    public float baseWaveDifficultyUpgrade = 1.5f;

    [Range(0.05f, 1f)]
    public float baseWaveMonochrome = 0.2f;

    [Range(0, 10)]
    public int baseStandardDifficultyIncrease = 1;

    [Range(0, 5)]
    public int baseWaveNumberUpgrade = 1;


    public Color redVisualColor = Color.red;
    public Color greenVisualColor = new Color(0f, 0.65f, 0.15f);
    public Color blueVisualColor = Color.blue;
    public Color yellowVisualColor = Color.yellow;
    public Color orangeVisualColor = new Color(1f, 0.65f, 0.15f);
    public Color purpleVisualColor = new Color(0.5f, 0f, 1f);
    public Color whiteVisualColor = Color.white;

    public int CHARACTERMAX = 68;

    public List<AudioClip> bulletSounds = new List<AudioClip>();
    public List<AudioClip> enemySounds = new List<AudioClip>();
    public List<AudioClip> uiSounds = new List<AudioClip>();
    public List<AudioClip> music = new List<AudioClip>();

    public GameObject DeathSplatter;
    public GameObject DeathGiblet;

    public int xBounds = 3;
    public int yBounds = 5;

    public static GameModel instance { get; private set; }
    // Start is called before the first frame update
    void Start()
    {
        instance = this;  
    }

    public bool isInBounds(Vector3 location)
    {
        return location.x <= xBounds && location.x >= -xBounds && location.y >= -yBounds && location.y <= yBounds;
    }

    public bool isInBoundsPercent(Vector3 location, float percentage)
    {
        Debug.Log(-(xBounds * percentage));
        Debug.Log(-(yBounds * percentage));
        return location.x <= (xBounds * percentage) && location.x > (-xBounds * percentage) && location.y > (-yBounds * percentage) && location.y <= (yBounds * percentage);
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
                return whiteVisualColor;
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

    public GameColor ReturnRandomMixedColor(GameColor col)
    {
        int rand = UnityEngine.Random.Range(0, 2);
        switch (col)
        {
            case GameColor.RED:
                return rand == 0 ? GameColor.PURPLE : GameColor.ORANGE;
            case GameColor.BLUE:
                return rand == 0 ? GameColor.PURPLE : GameColor.GREEN;
            case GameColor.YELLOW:
                return rand == 0 ? GameColor.GREEN : GameColor.ORANGE;
            case GameColor.ORANGE:
                return GameColor.WHITE;
            case GameColor.GREEN:
                return GameColor.WHITE;
            case GameColor.PURPLE:
                return GameColor.WHITE;
            default:
                return col;
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
            case GameManager.UpgradeType.COMBINER:
                return UpgradeImages[14];
            case GameManager.UpgradeType.ROCKETS:
                return UpgradeImages[15];
            case GameManager.UpgradeType.PULSERADIUS:
                return UpgradeImages[16];
            case GameManager.UpgradeType.DEATHBLAST:
                return UpgradeImages[17];
            case GameManager.UpgradeType.RAINBOWMULT:
                return UpgradeImages[19];
            case GameManager.UpgradeType.BARRAGE:
                return UpgradeImages[20];
            case GameManager.UpgradeType.PULSE:
                return UpgradeImages[21];
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
            case UIManager.WaveModifier.MONOCHROME:
                return WaveModImages[4];
            default:
                return WaveModImages[0];
        }

    }

    public string GetWaveModTextFromType(UIManager.WaveModifier waveModifier, int count)
    {
        switch (waveModifier)
        {
            case UIManager.WaveModifier.DIFFICULT:
                return WaveModTexts[0];
            case UIManager.WaveModifier.FASTER:
                return WaveModTexts[1] + (count * 20) + WaveModTexts[2];
            case UIManager.WaveModifier.NUMEROUS:
                if (count == 1)
                {
                    return WaveModTexts[3] + (count * 12) + WaveModTexts[4] + count + WaveModTexts[5];
                }
                else
                {
                    return WaveModTexts[3] + (count * 12) + WaveModTexts[4] + count + WaveModTexts[6];
                }
            case UIManager.WaveModifier.BIGGER_WAVE:
                return WaveModTexts[3];
            case UIManager.WaveModifier.MONOCHROME:
                String returnText = WaveModTexts[7] + baseWaveMonochrome * count * 100 + WaveModTexts[8];
                return returnText;
            default:
                return "Huh";
        }
    }

    public int GetPlayerUpgradePreviewColorRowFromColor(GameModel.GameColor color)
    {
        switch (color)
        {
            case GameColor.RED:
                return 0;
            case GameColor.BLUE:
                return 1;
            case GameColor.YELLOW:
                return 2;
            case GameColor.NONE:
                return 3;
        }
        Debug.Log("Heck");
        return 0;
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
            case "Zigzag":
                returnText = ChunkTexts[6];
                break;
            case "Rage":
                returnText = ChunkTexts[7];
                break;
            case "Painter":
                returnText = ChunkTexts[8];
                break;
            case "Explosive":
                returnText = ChunkTexts[9];
                break;
            case "Splitter":
                returnText = ChunkTexts[10];
                break;
            case "Swooper":
                returnText = ChunkTexts[11];
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

    public string GetUpgradeTextFromType(GameManager.Upgrade upgrade)
    {
        string returnText = "";
        switch (upgrade.type)
        {
            case GameManager.UpgradeType.SHOTSPEED:
                returnText = UpgradeTexts[0];
                returnText += WeaponUpgradeText(upgrade);
                break;
            case GameManager.UpgradeType.SHOTS:
                returnText = UpgradeTexts[1];
                returnText += WeaponUpgradeText(upgrade);
                break;
            case GameManager.UpgradeType.ATTACKSPEED:
                returnText = UpgradeTexts[2];
                returnText += WeaponUpgradeText(upgrade);
                break;
            case GameManager.UpgradeType.SHIELDS:
                returnText = UpgradeTexts[3];
                break;
            case GameManager.UpgradeType.SHOTSIZE:
                returnText = UpgradeTexts[4];
                returnText += WeaponUpgradeText(upgrade);
                break;
            case GameManager.UpgradeType.PIERCING:
                returnText = UpgradeTexts[5];
                returnText += WeaponUpgradeText(upgrade);
                break;
            case GameManager.UpgradeType.COMBINER:
                returnText = UpgradeTexts[10] + combinerBaseChance * upgrade.factor + UpgradeTexts[11];
                break;
            case GameManager.UpgradeType.PULSERADIUS:
                returnText = UpgradeTexts[12];
                break;
            case GameManager.UpgradeType.DEATHBLAST:
                returnText = UpgradeTexts[13];
                break;
            case GameManager.UpgradeType.ROCKETS:
                returnText = UpgradeTexts[14] + upgrade.factor * 2 +" "+ upgrade.color.ToString().ToLower() + UpgradeTexts[15] + rocketInterval + "s";
                break;
            case GameManager.UpgradeType.RAINBOWMULT:
                returnText = UpgradeTexts[16] + upgrade.factor * gameModel.meterMultInc * 100 + UpgradeTexts[17] + upgrade.factor;
                returnText += (upgrade.factor == 1 ? UpgradeTexts[21] : UpgradeTexts[22]);
                break;
            case GameManager.UpgradeType.PULSE:
                returnText = UpgradeTexts[19] + (5 - upgrade.factor) + UpgradeTexts[20];
                break;
            case GameManager.UpgradeType.BARRAGE:
                returnText = UpgradeTexts[18];
                break;
            default:
                return "Something went wrong";
        }

        return returnText;
    }

    private String WeaponUpgradeText(GameManager.Upgrade upgrade)
    {
        String returnText = "";
        if (upgrade.isPlayerUpgrade)
        {
            return returnText;
        }
        if (upgrade.color == GameColor.WHITE)
        {
            returnText += UpgradeTexts[6];
        }
        else
        {
            returnText += UpgradeTexts[7] + upgrade.factor + UpgradeTexts[8] + upgrade.color.ToString().ToLower() + UpgradeTexts[9];
        }
        return returnText;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
