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

    public static SaveLoadManager instance { get; private set; }
    private GameData gameData;
    private SettingsData settingsData;
    public List<IDataPersistence> saveLoadObjects = new List<IDataPersistence>();
    private FileDataHandler dataHandler;
    public bool isWebGL;
    public void initialize()
    {
        this.dataHandler = new FileDataHandler(Application.persistentDataPath, fileName, Application.persistentDataPath, settingsFileName);
        this.saveLoadObjects = FindAllSaveLoadObjects();    
        LoadGame();
        LoadSettings();
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
        dataHandler.Save(gameData);
    }


    public void WipeMidRunDataOnly()
    {
        Debug.Log("Starting new Run");
        gameData.currentLevel = 0;
        Debug.Log("Level" + gameData.currentLevel);
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
        Debug.Log("Current Undiscovered med Mechanics" + WaveSpawningSystem.instance.medMechanics.Count);
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
        Debug.Log("Current Undiscovered med Mechanics" + WaveSpawningSystem.instance.medMechanics.Count);
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

    public void SaveGame()
    {
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
        dataHandler.SaveSettings(settingsData);
    }
}
