using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class WaveSpawningSystem : MonoBehaviour
{
    public List<GameObject> Enemies = new List<GameObject>();
    public List<EnemyBehavior> EnemyScripts = new List<EnemyBehavior>();

    public int Level = 1;
    public int baseGlobalWaveNumber;
    public float baseGlobalWaveSpacing;
    public int baseNumChunks;
    private int globalWaveNumber;
    private float globalWaveSpacing;
    private int numChunks;
    List<int> availableChunks = new List<int>();
    public List<WaveObject> currentWave = new List<WaveObject>();
    public List<GameModel.GameColor> currentColors = new List<GameModel.GameColor>();
    private int currentWaveIndex = 0;

    public enum SpawnLocations
    {
        TOP,
        BOTTOM,
        LEFT,
        RIGHT,
    }
    
    public int[] ALL =
        {0, 1, 2, 3};

    
    void Start()
    {
        addStartingChunks();
        globalWaveNumber = baseGlobalWaveNumber;
        globalWaveSpacing = baseGlobalWaveSpacing;
        numChunks = baseNumChunks;
        
        generateWave();
        initializeColorsForTestingPurposes();
    }

    void addStartingChunks()
    {
        //Basic Red wave
        availableChunks.Add(0);
        
        //Basic Yellow wave
        availableChunks.Add(1);
        
        //Basic Blue wave
        availableChunks.Add(2);
        
        //Basic Red and Yellow wave
        availableChunks.Add(3);
        
        //Basic Red and Blue wave
        availableChunks.Add(4);
        
        //Basic Yellow and Blue wave
        availableChunks.Add(5);
    }
    
    public int returnRandomChunk()
    {
        return availableChunks[Random.Range(0, availableChunks.Count)];
    }

    public void generateWave()
    {
        for (int i = 0; i < numChunks; i++)
        {
            addChunk(returnRandomChunk());
        }
    }

    public void addChunk(int chunk)
    {
        switch (chunk)
        {
            case 0:
                generateBasicRedChunk();
                break;
            case 1:
                generateBasicBlueChunk();
                break;
            case 2:
                generateBasicYellowChunk();
                break;
            case 3:
                generateBasicRedAndYellowChunk();
                break;
            case 4:
                generateBasicRedAndBlueChunk();
                break;
            case 5:
                generateBasicYellowAndBlueChunk();
                break;
            default:
                Debug.Log("Failed to generate proper chunk");
                generateBasicRedChunk();
                break;
        }
    }

    void initializeColorsForTestingPurposes()
    {
        for (int k = 0; k < currentWave.Count; k++)
        {
            currentColors.Add(currentWave[k].color);
        }
    }
    
    //============================================================ Wave Chunk Behaviors ============================================

    void generateBasicRedChunk()
    {
        Debug.Log("Basic Red Chunk Added");
        for (int i = 0; i < globalWaveNumber; i++)
        {
            currentWave.Add(new WaveObject(Enemies[0], EnemyScripts[0], GameModel.GameColor.RED, globalWaveSpacing, ALL));
        }
    }
    
    void generateBasicBlueChunk()
    {
        Debug.Log("Basic Blue Chunk Added");
        for (int i = 0; i < globalWaveNumber; i++)
        {
            currentWave.Add(new WaveObject(Enemies[0], EnemyScripts[0], GameModel.GameColor.BLUE, globalWaveSpacing, ALL));
        }
    }
    
    void generateBasicYellowChunk()
    {
        Debug.Log("Basic Yellow Chunk Added");
        for (int i = 0; i < globalWaveNumber; i++)
        {
            currentWave.Add(new WaveObject(Enemies[0], EnemyScripts[0], GameModel.GameColor.YELLOW, globalWaveSpacing, ALL));
        }
    }

    void generateBasicRedAndBlueChunk()
    {
        Debug.Log("Basic Red/Blue Chunk Added");
        for (int i = 0; i < globalWaveNumber; i++)
        {
            int rand = Random.Range(0, 2);
            if (rand == 1)
            {
                currentWave.Add(new WaveObject(Enemies[0], EnemyScripts[0], GameModel.GameColor.RED, globalWaveSpacing, ALL));
            }
            else
            {
                currentWave.Add(new WaveObject(Enemies[0], EnemyScripts[0], GameModel.GameColor.BLUE, globalWaveSpacing, ALL));
            }
        }
    }
    
    void generateBasicRedAndYellowChunk()
    {
        Debug.Log("Basic Red/Yellow Chunk Added");
        for (int i = 0; i < globalWaveNumber; i++)
        {
            int rand = Random.Range(0, 2);
            if (rand == 1)
            {
                currentWave.Add(new WaveObject(Enemies[0], EnemyScripts[0], GameModel.GameColor.RED, globalWaveSpacing, ALL));
            }
            else
            {
                currentWave.Add(new WaveObject(Enemies[0], EnemyScripts[0], GameModel.GameColor.YELLOW, globalWaveSpacing, ALL));
            }
        }
    }
    
    void generateBasicYellowAndBlueChunk()
    {
        Debug.Log("Basic Blue/Yellow Chunk Added");
        for (int i = 0; i < globalWaveNumber; i++)
        {
            int rand = Random.Range(0, 2);
            if (rand == 1)
            {
                currentWave.Add(new WaveObject(Enemies[0], EnemyScripts[0], GameModel.GameColor.YELLOW, globalWaveSpacing, ALL));
            }
            else
            {
                currentWave.Add(new WaveObject(Enemies[0], EnemyScripts[0], GameModel.GameColor.BLUE, globalWaveSpacing, ALL));
            }
        }
    }
    
    // ========================================================= End of Chunks =================================================================
    
    public class WaveObject
    {
        private GameObject body;
        private EnemyBehavior script;
        public GameModel.GameColor color;
        private float delayUntilNext;
        private int[] locationsToSpawn;
        
        public WaveObject(GameObject enemyObject, EnemyBehavior enemyScript, GameModel.GameColor col, float delay, int[] loc)
        {
            body = enemyObject;
            script = enemyScript;
            color = col;
            delayUntilNext = delay;
            locationsToSpawn = loc;
        }
    }
}
