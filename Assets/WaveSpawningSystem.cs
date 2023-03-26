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

    private float enemyTimer = 0.0f;
    private float xBounds = 3;
    private float yBounds = 5;
    
    public GameModel modelGame;



    public enum SpawnLocations
    {
        TOP,
        BOTTOM,
        LEFT,
        RIGHT,
    }
    
    public int[] ALL =
        {0, 1, 2, 3};

    //================================================  Enemy Spawning ===============================================
    
    void EnemyUpdate()
    {
        enemyTimer -= Time.deltaTime;
        if (enemyTimer <= 0.0f)
        {
            int EdgeToSpawnFrom = currentWave[currentWaveIndex].locationsToSpawn[Random.Range(0, currentWave[currentWaveIndex].locationsToSpawn.Length)];
            Vector3 startPos;
            switch (EdgeToSpawnFrom)
            {
                case 0:
                    startPos = new Vector3(Random.Range(xBounds, -xBounds), yBounds, 0);
                    break;
                case 1:
                    startPos = new Vector3(xBounds, Random.Range(yBounds, -yBounds), 0);
                    break;
                case 2:
                    startPos = new Vector3(Random.Range(xBounds, -xBounds), -yBounds, 0);
                    break;
                case 3:
                    startPos = new Vector3(-xBounds, Random.Range(yBounds, -yBounds), 0);
                    break;
                default:
                    startPos = new Vector3(Random.Range(xBounds, -xBounds), yBounds, 0);
                    break;
            }
            GameObject newEnemy = Instantiate(currentWave[currentWaveIndex].body, startPos, Quaternion.identity);
            EnemyBehavior enemyScript = newEnemy.GetComponent<EnemyBehavior>();
            GameModel.GameColor enemyColor = currentWave[currentWaveIndex].color;
            enemyScript.initialize(transform.position, enemyColor);
            enemyScript.SetColor(modelGame.ColorToColor(enemyColor));
            enemyTimer = currentWave[currentWaveIndex].delayUntilNext;
            currentWaveIndex++;
        }
    }

    void Update()
    {
        EnemyUpdate();
    }
    
    
    
    void Start()
    {
        modelGame = GameObject.Find("GameManager").GetComponent<GameModel>();
        
        addStartingChunks();
        globalWaveNumber = baseGlobalWaveNumber;
        globalWaveSpacing = baseGlobalWaveSpacing;
        numChunks = baseNumChunks;
        
        generateWave();
        initializeColorsForTestingPurposes();
        
        enemyTimer = currentWave[currentWaveIndex].delayUntilNext;
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
        public GameObject body;
        public EnemyBehavior script;
        public GameModel.GameColor color;
        public float delayUntilNext;
        public int[] locationsToSpawn;
        
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
