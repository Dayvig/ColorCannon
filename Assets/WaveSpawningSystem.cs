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
    public GameObject player;



    public enum SpawnLocations
    {
        TOP,
        BOTTOM,
        LEFT,
        RIGHT,
    }
    
    public int[] ALL =
        {0, 1, 2, 3};

    public int[] TOPBOTTOM = {0, 1};
    public int[] LEFTRIGHT = {2, 3};

    //================================================  Enemy Spawning ===============================================
    
    void EnemyUpdate()
    {
        enemyTimer -= Time.deltaTime;
        if (enemyTimer <= 0.0f)
        {
            WaveObject enemy = currentWave[currentWaveIndex];
            int EdgeToSpawnFrom = enemy.locationsToSpawn[Random.Range(0, enemy.locationsToSpawn.Length)];
            Vector3 startPos;
            switch (EdgeToSpawnFrom)
            {
                case 0:
                    startPos = new Vector3(Random.Range(xBounds, -xBounds), yBounds, 0);
                    break;
                case 1:
                    startPos = new Vector3(Random.Range(xBounds, -xBounds), -yBounds, 0);
                    break;
                case 2:
                    startPos = new Vector3(-xBounds, Random.Range(yBounds, -yBounds), 0);
                    break;
                case 3:
                    startPos = new Vector3(xBounds, Random.Range(yBounds, -yBounds), 0);
                    break;
                default:
                    startPos = new Vector3(Random.Range(xBounds, -xBounds), yBounds, 0);
                    break;
            }
            GameObject newEnemy = Instantiate(currentWave[currentWaveIndex].body, startPos, Quaternion.identity);
            EnemyBehavior enemyScript = newEnemy.GetComponent<EnemyBehavior>();
            GameModel.GameColor enemyColor = currentWave[currentWaveIndex].color;
            enemyScript.initialize(player.transform.position, enemyColor);
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
        
        //Basic Red Fast wave
        availableChunks.Add(6);
        
        //Basic Yellow Fast wave
        availableChunks.Add(7);
        
        //Basic Blue Fast wave
        availableChunks.Add(8);
        
        //Basic Red Ninja Wave
        availableChunks.Add(9);
        
        //Basic Blue Ninja Wave
        availableChunks.Add(10);
        
        //Basic Blue Ninja Wave
        availableChunks.Add(11);

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
                generateBasicChunk(GameModel.GameColor.RED);
                break;
            case 1:
                generateBasicChunk(GameModel.GameColor.BLUE);
                break;
            case 2:
                generateBasicChunk(GameModel.GameColor.YELLOW);
                break;
            case 3:
                generateBasicTwoColorChunk(GameModel.GameColor.RED, GameModel.GameColor.YELLOW);
                break;
            case 4:
                generateBasicTwoColorChunk(GameModel.GameColor.RED, GameModel.GameColor.BLUE);
                break;
            case 5:
                generateBasicTwoColorChunk(GameModel.GameColor.BLUE, GameModel.GameColor.YELLOW);
                break;
            case 6:
                generateBasicFastChunk(GameModel.GameColor.RED);
                break;
            case 7:
                generateBasicFastChunk(GameModel.GameColor.YELLOW);
                break;
            case 8:
                generateBasicFastChunk(GameModel.GameColor.BLUE);
                break;
            case 9:
                generateBasicNinjaChunk(GameModel.GameColor.RED);
                break;
            case 10:
                generateBasicNinjaChunk(GameModel.GameColor.BLUE);
                break;
            case 11:
                generateBasicNinjaChunk(GameModel.GameColor.YELLOW);
                break;
            default:
                Debug.Log("Failed to generate proper chunk");
                generateBasicChunk(GameModel.GameColor.RED);
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

    void generateBasicChunk(GameModel.GameColor color)
    {
        Debug.Log("Basic Chunk Added");
        for (int i = 0; i < globalWaveNumber; i++)
        {
            currentWave.Add(new WaveObject(Enemies[0], EnemyScripts[0], color, globalWaveSpacing, TOPBOTTOM));
        }
    }
    
    void generateBasicFastChunk(GameModel.GameColor color)
    {
        Debug.Log("Basic Fast Chunk Added");
        for (int i = 0; i < globalWaveNumber; i++)
        {
            currentWave.Add(new WaveObject(Enemies[1], EnemyScripts[1], color, globalWaveSpacing, TOPBOTTOM));
        }
    }
    
    void generateBasicNinjaChunk(GameModel.GameColor color)
    {
        Debug.Log("Basic Fast Chunk Added");
        for (int i = 0; i < globalWaveNumber; i++)
        {
            currentWave.Add(new WaveObject(Enemies[2], EnemyScripts[2], color, globalWaveSpacing, LEFTRIGHT));
        }
    }

    void generateBasicTwoColorChunk(GameModel.GameColor color1, GameModel.GameColor color2)
    {
        Debug.Log("Basic Red/Blue Chunk Added");
        for (int i = 0; i < globalWaveNumber; i++)
        {
            int rand = Random.Range(0, 2);
            if (rand == 1)
            {
                currentWave.Add(new WaveObject(Enemies[0], EnemyScripts[0], color1, globalWaveSpacing, TOPBOTTOM));
            }
            else
            {
                currentWave.Add(new WaveObject(Enemies[0], EnemyScripts[0], color2, globalWaveSpacing, TOPBOTTOM));
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
