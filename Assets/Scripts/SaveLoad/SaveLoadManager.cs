using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static WaveSpawningSystem;

public class SaveLoadManager : MonoBehaviour
{

    [Header("File Storage Config")]
    [SerializeField] private string fileName;
    [SerializeField] private string settingsFileName;
    [SerializeField] private string unlocksFileName;

    public static SaveLoadManager instance { get; private set; }
    private GameData gameData;
    private SettingsData settingsData;
    private UnlockData unlockData;

    public List<IDataPersistence> saveLoadObjects = new List<IDataPersistence>();
    private FileDataHandler dataHandler;
    public bool isWebGL;
    public bool isAndroidBuild;
    public void initialize()
    {
        this.dataHandler = new FileDataHandler(Application.persistentDataPath, fileName, Application.persistentDataPath, settingsFileName, Application.persistentDataPath, unlocksFileName);
        this.saveLoadObjects = FindAllSaveLoadObjects();

        LoadGame();
        LoadSettings();
        LoadUnlocks();
    }

    public void Awake()
    {
        if (instance != null)
        {
            
            Debug.Log("Multiple SaveLoadManagers Detected.");
        }
        instance = this;
    }

    public void NewGame()
    {
        gameData = new GameData();
        settingsData = new SettingsData();
        unlockData = new UnlockData();
        dataHandler.Save(gameData);
        dataHandler.SaveSettings(settingsData);
        dataHandler.SaveUnlocks(unlockData);
    }


    public void WipeMidRunDataOnly()
    {
        gameData.currentLevel = 0;
        gameData.refreshActive = true;
        gameData.undiscoveredEasyMechanics.Clear();
        gameData.undiscoveredMedMechanics.Clear();
        gameData.chunkDifficulties = new int[] { 1, 1, 2 };
        gameData.currentNewMechanics.Clear();
        gameData.currentMechanics.Clear();
        gameData.currentUpgradesOffered.Clear();
        gameData.playerUpgrades.Clear();
        gameData.waveUpgrades.Clear();
        gameData.waveSpacing = GameModel.instance.baseGlobalWaveSpacing;
        gameData.waveNumber = GameModel.instance.baseGlobalWaveNumber;
        gameData.waveSpeed = GameModel.instance.baseGlobalWaveSpeed;
        gameData.rainbowMult = GameModel.instance.baseGlobalRainbowMult;
        gameData.chunks.Clear();
        gameData.numChunks = GameModel.instance.baseNumChunks;
        gameData.uniqueChunks = GameModel.instance.baseNumUniqueChunks;
        gameData.playerLives = 3;
        gameData.rainbowMeter = 0f;
        UIManager.instance.DestroyAll();
        dataHandler.Save(gameData);
    }

    public void WipeAllData()
    {
        NewGame();
    }

    public void LoadGame()
    {
        if (!isWebGL)
        {
            this.gameData = dataHandler.Load();
        }
        else
        {
            NewGame();
        }

        if (this.gameData == null)
        {
            NewGame();
        }

        foreach (IDataPersistence saveLoadObj in saveLoadObjects)
        { 
            saveLoadObj.LoadData(gameData);
        }
    }

    public void LoadSettings()
    {
        this.settingsData = dataHandler.LoadSettings();
        if (settingsData == null)
        {
            settingsData = new SettingsData();
        }
        SoundManager.instance.masterVolume = settingsData.masterVolume;
        SoundManager.instance.musicVolume= settingsData.musicVolume;
        SoundManager.instance.sfxVolume = settingsData.sfxVolume;
        SoundManager.instance.isMuted = settingsData.muted;
        UIManager.instance.mute.SetState();
        GameManager.instance.splatterVal = settingsData.splatters;
        GameManager.instance.doubleTapCycle = settingsData.doubletapcycle;
        if (settingsData.tutorialOn)
        {
            WaveSpawningSystem.instance.tutorialStage = 0;
        }
        else
        {
            WaveSpawningSystem.instance.tutorialStage = -2;
        }
    }

    public void LoadUnlocks()
    {
        this.unlockData = dataHandler.LoadUnlocks();
        if (unlockData == null)
        {
            unlockData = new UnlockData();
        }
        GameManager.instance.rainbowInk = unlockData.rainbowInk;
        GameManager.instance.unlockedArenas = unlockData.arenas;
        GameManager.instance.arena = unlockData.currentArena;
        GameManager.instance.lastValidArena = unlockData.currentArena;
        GameManager.instance.maxProModeLevelUnlocked = unlockData.maxProMode;
    }

    public int getRainbowInk()
    {
        return unlockData.rainbowInk;
    }
    public void spendRainbowInk(int spent)
    {
        unlockData.rainbowInk -= spent;
    }
    
    public void unlockArena(int unlocked)
    {
        unlockData.arenas.Add(unlocked);
    }

    public List<int> getUnlockedArenas()
    {
        return unlockData.arenas;
    }

    public void SaveGame()
    {
        SaveUnlocks();
        foreach (IDataPersistence saveLoadObj in saveLoadObjects)
        {
            saveLoadObj.SaveData(ref gameData);
        }
        dataHandler.Save(gameData);
    }

    private List<IDataPersistence> FindAllSaveLoadObjects()
    {
        IEnumerable<IDataPersistence> saveLoadObjects = Object.FindObjectsOfType<MonoBehaviour>().OfType<IDataPersistence>();

        return new List<IDataPersistence>(saveLoadObjects);
    }

    public void SaveSettings()
    {
        settingsData.masterVolume = SoundManager.instance.masterVolume;
        settingsData.musicVolume = SoundManager.instance.musicVolume;
        settingsData.sfxVolume = SoundManager.instance.sfxVolume;
        settingsData.splatters = GameManager.instance.splatterVal;
        settingsData.doubletapcycle = GameManager.instance.doubleTapCycle;
        settingsData.tutorialOn = (WaveSpawningSystem.instance.tutorialStage == 0);
        settingsData.muted = SoundManager.instance.isMuted;
        dataHandler.SaveSettings(settingsData);
    }

    public void SaveUnlocks()
    {
        unlockData.rainbowInk = GameManager.instance.rainbowInk;
        unlockData.arenas = GameManager.instance.unlockedArenas;
        unlockData.currentArena = GameManager.instance.arena;
        unlockData.maxProMode = GameManager.instance.maxProModeLevelUnlocked;
        dataHandler.SaveUnlocks(unlockData);
    }
}
