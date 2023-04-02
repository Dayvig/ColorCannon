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
    private static int globalWaveNumber;
    private static float globalWaveSpacing;
    private int numChunks;
    List<Chunk> availableChunks = new List<Chunk>();
    public static List<WaveObject> currentWave = new List<WaveObject>();
    public List<GameModel.GameColor> currentColors = new List<GameModel.GameColor>();
    public int currentWaveIndex = 0;

    private float enemyTimer = 0.0f;
    private float xBounds = 3;
    private float yBounds = 5;
    
    public GameModel modelGame;
    public GameObject player;
    public GameManager gameManager;
    public static UIManager uiManager;

    public enum SpawnLocations
    {
        TOP,
        BOTTOM,
        LEFT,
        RIGHT,
    }
    
    public int[] ALL =
        {0, 1, 2, 3};

    public static int[] TOPBOTTOM = {0, 1};
    public static int[] LEFTRIGHT = {2, 3};
    

    //================================================  Enemy Spawning ===============================================
    
    public void EnemyUpdate()
    {
        enemyTimer -= Time.deltaTime;
        if (enemyTimer <= 0.0f && currentWaveIndex < currentWave.Count-1)
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
            gameManager.activeEnemies.Add(enemyScript);
            enemyTimer = currentWave[currentWaveIndex].delayUntilNext;
            if (currentWaveIndex < currentWave.Count-1){
                currentWaveIndex++;
            }
        }
    }

    void Start()
    {
        modelGame = GameObject.Find("GameManager").GetComponent<GameModel>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        uiManager = GameObject.Find("GameManager").GetComponent<UIManager>();

        addStartingChunks();
        globalWaveNumber = baseGlobalWaveNumber;
        globalWaveSpacing = baseGlobalWaveSpacing;
        numChunks = baseNumChunks;

        gameManager.addStartingUpgrades();
        generateWave();
        generateUpgrades();
        initializeColorsForTestingPurposes();
        
        enemyTimer = currentWave[currentWaveIndex].delayUntilNext;
    }

    void addStartingChunks()
    {
        //Basic Red wave
        availableChunks.Add(new BasicChunk(new[]{GameModel.GameColor.RED}));
        
        //Basic Yellow wave
        availableChunks.Add(new BasicChunk(new[]{GameModel.GameColor.YELLOW}));
        
        //Basic Blue wave
        availableChunks.Add(new BasicChunk(new[]{GameModel.GameColor.BLUE}));
        
        //Basic Red and Yellow wave
        availableChunks.Add(new BasicChunk(new[]{GameModel.GameColor.YELLOW, GameModel.GameColor.RED}));
        
        //Basic Red and Blue wave
        availableChunks.Add(new BasicChunk(new[]{GameModel.GameColor.RED, GameModel.GameColor.BLUE}));
        
        //Basic Yellow and Blue wave
        availableChunks.Add(new BasicChunk(new[]{GameModel.GameColor.YELLOW, GameModel.GameColor.BLUE}));
        
        //Basic Red Fast wave
        availableChunks.Add(new FastChunk(new[]{GameModel.GameColor.RED}));
        
        //Basic Yellow Fast wave
        availableChunks.Add(new FastChunk(new[]{GameModel.GameColor.YELLOW}));
        
        //Basic Blue Fast wave
        availableChunks.Add(new FastChunk(new[]{GameModel.GameColor.BLUE}));
        
        //Basic Red Ninja Wave
        availableChunks.Add(new NinjaChunk(new[]{GameModel.GameColor.RED}));
        
        //Basic Blue Ninja Wave
        availableChunks.Add(new NinjaChunk(new[]{GameModel.GameColor.BLUE}));
        
        //Basic Blue Ninja Wave
        availableChunks.Add(new NinjaChunk(new[]{GameModel.GameColor.YELLOW}));

    }
    
    public Chunk returnRandomChunk()
    {
        return availableChunks[Random.Range(0, availableChunks.Count)];
    }

    public void generateWave()
    {
        uiManager.WipePreviewImages();
        for (int i = 0; i < numChunks; i++)
        {
            /*
            Chunk nextChunk = returnRandomChunk();
            uiManager.SetupChunkPreview(nextChunk);
            nextChunk.Generate();
            */
        }
        Chunk nextChunk = new BasicChunk(new[]{GameModel.GameColor.ORANGE});
        uiManager.SetupChunkPreview(nextChunk);
        nextChunk.Generate();
        
        nextChunk = new BasicChunk(new[]{GameModel.GameColor.PURPLE});
        uiManager.SetupChunkPreview(nextChunk);
        nextChunk.Generate();
        
        nextChunk = new BasicChunk(new[]{GameModel.GameColor.GREEN});
        uiManager.SetupChunkPreview(nextChunk);
        nextChunk.Generate();

        nextChunk = new BasicChunk(new[]{GameModel.GameColor.WHITE});
        uiManager.SetupChunkPreview(nextChunk);
        nextChunk.Generate();
    }

    public void generateUpgrades()
    {
        for (int i = 0; i < 3; i++)
        {
            GameManager.Upgrade nextUpgrade = gameManager.getRandomUpgrade();
            uiManager.SetupUpgradePreview(nextUpgrade);
        }
    }

    public void SetupNextWave()
    {
        uiManager.WipePreviewImages();
        clearWave();
        generateWave();
        generateUpgrades();
        Level++;
    }

    private void clearWave()
    {
        currentWave.Clear();
        currentWaveIndex = 0;
    }

    void initializeColorsForTestingPurposes()
    {
        for (int k = 0; k < currentWave.Count; k++)
        {
            currentColors.Add(currentWave[k].color);
        }
    }
    
    //============================================================ Wave Chunk Behaviors ============================================

    public abstract class Chunk
    {
        public WaveSpawningSystem spawningSystem;
        public GameModel.GameColor[] colors = {GameModel.GameColor.RED};
        public Sprite image;
        public bool isMultiColor;

        public Chunk(GameModel.GameColor[] spawnColors)
        {
            colors = spawnColors;
            spawningSystem = GameObject.Find("GameManager").GetComponent<WaveSpawningSystem>();
        }
        public abstract void Generate();
    }

    public class BasicChunk : Chunk
    { 
        public override void Generate()
        {
            Debug.Log("Basic Chunk Added");
            if (colors.Length == 1)
            {
                for (int i = 0; i < globalWaveNumber; i++)
                {
                    currentWave.Add(new WaveObject(spawningSystem.Enemies[0], spawningSystem.EnemyScripts[0], colors[0], globalWaveSpacing, TOPBOTTOM));
                }
            }
            else
            {
                int rand = Random.Range(0, colors.Length);
                {
                    for (int i = 0; i < globalWaveNumber; i++)
                    {
                        currentWave.Add(new WaveObject(spawningSystem.Enemies[0], spawningSystem.EnemyScripts[0], colors[rand], globalWaveSpacing, TOPBOTTOM));
                    }
                }
            }
        }

        public BasicChunk(GameModel.GameColor[] spawnColors) : base(spawnColors)
        {
            image = uiManager.EnemySprites[0];
        }
    }
    
    public class FastChunk : Chunk
    {
        public override void Generate()
        {
            Debug.Log("Fast Chunk Added");
            if (colors.Length == 1)
            {
                for (int i = 0; i < globalWaveNumber; i++)
                {
                    currentWave.Add(new WaveObject(spawningSystem.Enemies[1], spawningSystem.EnemyScripts[1], colors[0], globalWaveSpacing, TOPBOTTOM));
                }
            }
            else
            {
                int rand = Random.Range(0, colors.Length);
                {
                    for (int i = 0; i < globalWaveNumber; i++)
                    {
                        currentWave.Add(new WaveObject(spawningSystem.Enemies[1], spawningSystem.EnemyScripts[1], colors[rand], globalWaveSpacing, TOPBOTTOM));
                    }
                }
            }
        }

        public FastChunk(GameModel.GameColor[] spawnColors) : base(spawnColors)
        {
            image = uiManager.EnemySprites[1];
        }
    }
    
    public class NinjaChunk : Chunk
    {
        public override void Generate()
        {
            Debug.Log("Ninja Chunk Added");
            if (colors.Length == 1)
            {
                for (int i = 0; i < globalWaveNumber; i++)
                {
                    currentWave.Add(new WaveObject(spawningSystem.Enemies[2], spawningSystem.EnemyScripts[2], colors[0], globalWaveSpacing, LEFTRIGHT));
                }
            }
            else
            {
                int rand = Random.Range(0, colors.Length);
                {
                    for (int i = 0; i < globalWaveNumber; i++)
                    {
                        currentWave.Add(new WaveObject(spawningSystem.Enemies[2], spawningSystem.EnemyScripts[2], colors[rand], globalWaveSpacing, TOPBOTTOM));
                    }
                }
            }
        }

        public NinjaChunk(GameModel.GameColor[] spawnColors) : base(spawnColors)
        {
            image = uiManager.EnemySprites[2];
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
