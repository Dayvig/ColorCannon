using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static WaveSpawningSystem;

public class SaveLoadManager : MonoBehaviour
{

    [Header("File Storage Config")]
    [SerializeField] private string fileName;
    
    public static SaveLoadManager instance { get; private set; }
    private GameData gameData;
    public List<IDataPersistence> saveLoadObjects = new List<IDataPersistence>();
    private FileDataHandler dataHandler;
    public void initialize()
    {
        this.dataHandler = new FileDataHandler(Application.persistentDataPath, fileName);
        this.saveLoadObjects = FindAllSaveLoadObjects();
        SaveLoadManager.instance.LoadGame();
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
        gameData.currentLevel = 1;
        gameData.refreshActive = true;
        gameData.undiscoveredEasyMechanics = new List<Mechanic> { Mechanic.FAST, Mechanic.NINJA, Mechanic.DARK, Mechanic.SWARM, Mechanic.ZIGZAG };
        gameData.undiscoveredMedMechanics = new List<Mechanic> { Mechanic.DISGUISED, Mechanic.SWIRL };
        gameData.chunkDifficulties = new int[] { 1, 1, 2 };
        gameData.currentNewMechanics.Clear();
        gameData.currentMechanics.Clear();
        gameData.currentUpgradesOffered.Clear();
        gameData.playerUpgrades.Clear();
        gameData.waveUpgrades.Clear();
        gameData.waveSpacing = GameModel.instance.baseGlobalWaveSpacing;
        gameData.waveNumber = GameModel.instance.baseGlobalWaveNumber;
        gameData.waveSpeed = GameModel.instance.baseGlobalWaveSpeed;
        gameData.chunks.Clear();
    }

    public void WipeAllData()
    {
        WipeMidRunDataOnly();
        gameData.encounteredEnemies.Clear();
    }

    public void LoadGame()
    {

        this.gameData = dataHandler.Load();

        if (this.gameData == null)
        {
            NewGame();
        }

        foreach (IDataPersistence saveLoadObj in saveLoadObjects)
        {
            saveLoadObj.LoadData(gameData);
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
}
