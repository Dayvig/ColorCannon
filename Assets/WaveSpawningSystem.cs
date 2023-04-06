using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public List<EnemyBehavior> inactiveEnemies = new List<EnemyBehavior>();
    public List<GameModel.GameColor> currentColors = new List<GameModel.GameColor>();
    public List<int> chunkDifficulties = new List<int>();
    public int currentWaveIndex = 0;

    private float enemyTimer = 0.0f;
    private float xBounds = 3;
    private float yBounds = 5;
    
    public GameModel modelGame;
    public GameObject player;
    public GameManager gameManager;
    public static UIManager uiManager;

    private int recusionProtection = 0;

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

            GameObject enemyObject = null;
            EnemyBehavior enemyScript;
            foreach (EnemyBehavior w in inactiveEnemies)
            {
                if (w.enemyType == currentWave[currentWaveIndex].enemyType)
                {
                    enemyObject = w.gameObject;
                    inactiveEnemies.Remove(w);
                    break;
                }
            }
            if (enemyObject == null)
            {
                enemyObject = Instantiate(currentWave[currentWaveIndex].body, startPos, Quaternion.identity);
            }

            enemyObject.transform.position = startPos;
            enemyScript = enemyObject.GetComponent<EnemyBehavior>();
            GameModel.GameColor enemyColor = currentWave[currentWaveIndex].color;
            bool darkEnemy = currentWave[currentWaveIndex].darkened;
            enemyScript.initialize(player.transform.position, enemyColor, darkEnemy, enemyScript.enemyType);
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
    }

    public void initialize()
    {
        addStartingChunks();
        globalWaveNumber = baseGlobalWaveNumber;
        globalWaveSpacing = baseGlobalWaveSpacing;
        numChunks = baseNumChunks;
        for (int i = 0; i < numChunks; i++)
        {
            chunkDifficulties.Add((i+1));
            Debug.Log(chunkDifficulties[i]);
        }

        gameManager.addStartingUpgrades();
        generateWave();
        gameManager.GenerateUpgrades();
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
        
        //Basic Red Dark wave
        availableChunks.Add(new BasicChunk(new[]{GameModel.GameColor.RED}, true));
        
        //Basic Yellow Dark wave
        availableChunks.Add(new BasicChunk(new[]{GameModel.GameColor.YELLOW}, true));
        
        //Basic Blue Dark wave
        availableChunks.Add(new BasicChunk(new[]{GameModel.GameColor.BLUE}, true));
        
        //Basic Red and Yellow wave
        availableChunks.Add(new BasicChunk(new[]{GameModel.GameColor.YELLOW, GameModel.GameColor.RED}));
        
        //Basic Red and Blue wave
        availableChunks.Add(new BasicChunk(new[]{GameModel.GameColor.RED, GameModel.GameColor.BLUE}));
        
        //Basic Yellow and Blue wave
        availableChunks.Add(new BasicChunk(new[]{GameModel.GameColor.YELLOW, GameModel.GameColor.BLUE}));
        

        //Basic Orange wave
        availableChunks.Add(new BasicChunk(new[]{GameModel.GameColor.ORANGE}));
        
        //Basic Purple wave
        availableChunks.Add(new BasicChunk(new[]{GameModel.GameColor.PURPLE}));
        
        //Basic Green wave
        availableChunks.Add(new BasicChunk(new[]{GameModel.GameColor.GREEN}));

        //Basic Fast Orange and purple wave
        availableChunks.Add(new FastChunk(new[]{GameModel.GameColor.ORANGE, GameModel.GameColor.PURPLE}));
        
    }
    
    public Chunk returnRandomChunk()
    {
        return availableChunks[Random.Range(0, availableChunks.Count)];
    }
    
    public Chunk returnRandomChunk(int lessthan)
    {
        Chunk nextChunk = availableChunks[Random.Range(0, availableChunks.Count)];
        if (nextChunk.difficulty <= lessthan)
        {
            return nextChunk;
        }
        recusionProtection++;
        if (recusionProtection <= 100)
        {
            return returnRandomChunk(lessthan);
        }
        Debug.Log("Tried to access a chunk that didn't exist");
        return new BasicChunk(new[] {GameModel.GameColor.RED, GameModel.GameColor.BLUE}, false);
    }
    
    public Chunk returnRandomChunk(int floor, int ceiling)
    {
        Chunk nextChunk = availableChunks[Random.Range(0, availableChunks.Count)];
        if (nextChunk.difficulty >= floor && nextChunk.difficulty <= ceiling)
        {
            return nextChunk;
        }
        recusionProtection++;
        if (recusionProtection <= 100)
        {
            return returnRandomChunk(floor, ceiling);
        }
        Debug.Log("Tried to access a chunk that didn't exist");

        return new BasicChunk(new[] {GameModel.GameColor.RED, GameModel.GameColor.BLUE}, false);
    }

    public void generateWave()
    {
        uiManager.WipePreviewImages();
        for (int i = 0; i < chunkDifficulties.Count; i++)
        {
            recusionProtection = 0;
            Chunk nextChunk = returnRandomChunk(chunkDifficulties[i]);
            uiManager.SetupChunkPreview(nextChunk);
            nextChunk.Generate();
        }
    }



    public void SetupNextWave()
    {
        uiManager.WipePreviewImages();
        clearWave();
        IncreaseDifficulty();
        generateWave();
        gameManager.GenerateUpgrades();
    }

    public void IncreaseDifficulty()
    {
        Level++;
        if (Level % 6 == 0)
        {
            double sum = 0;
            for (int k = 0; k < chunkDifficulties.Count; k++)
            {
                sum += chunkDifficulties[k];
            }
            numChunks++;
            chunkDifficulties.Add((int)Math.Ceiling((sum/chunkDifficulties.Count)));
        }
        int rand = Random.Range(0, chunkDifficulties.Count);
        chunkDifficulties[rand] = (int)Math.Ceiling(chunkDifficulties[rand] * 1.5f);
        for (int i = 0; i < chunkDifficulties.Count; i++)
        {
            Debug.Log(chunkDifficulties[i]);
        }
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
        public int difficulty;
        public int baseDifficulty;
        public bool isDarkened;

        public Chunk(GameModel.GameColor[] spawnColors, bool dark)
        {
            colors = spawnColors;
            spawningSystem = GameObject.Find("GameManager").GetComponent<WaveSpawningSystem>();
            isMultiColor = (colors.Contains(GameModel.GameColor.ORANGE) ||
                            colors.Contains(GameModel.GameColor.GREEN) ||
                            colors.Contains(GameModel.GameColor.PURPLE) ||
                            colors.Contains(GameModel.GameColor.WHITE));
            isDarkened = dark;
        }
        public Chunk(GameModel.GameColor[] spawnColors)
        {
            colors = spawnColors;
            spawningSystem = GameObject.Find("GameManager").GetComponent<WaveSpawningSystem>();
            isMultiColor = (colors.Contains(GameModel.GameColor.ORANGE) ||
                            colors.Contains(GameModel.GameColor.GREEN) ||
                            colors.Contains(GameModel.GameColor.PURPLE) ||
                            colors.Contains(GameModel.GameColor.WHITE));
            isDarkened = false;
        }
        public abstract void Generate();
        public virtual void SetDifficulty(GameModel.GameColor[] spawnColors)
        {
            difficulty = baseDifficulty + spawnColors.Length;
            if (isMultiColor)
            {
                difficulty *= 2;
                if (difficulty <= 6)
                {
                    difficulty = 6;
                }
            }

            if (isDarkened)
            {
                difficulty *= 2;
                if (difficulty <= 4)
                {
                    difficulty = 4;
                }
            }
        }
    }

    public class BasicChunk : Chunk
    {
        public override void Generate()
        {
            Debug.Log("Basic Chunk Added: Difficulty - "+difficulty + " / "+ String.Join("",
                new List<GameModel.GameColor>(colors)
                    .ConvertAll(i => i.ToString())
                    .ToArray())+ " | "+isDarkened);
            if (colors.Length == 1)
            {
                for (int i = 0; i < globalWaveNumber; i++)
                {
                    currentWave.Add(new WaveObject(spawningSystem.Enemies[0], spawningSystem.EnemyScripts[0], colors[0], globalWaveSpacing, 
                        TOPBOTTOM, isDarkened, WaveObject.Type.BASIC));
                }
            }
            else
            {
                int rand = Random.Range(0, colors.Length);
                {
                    for (int i = 0; i < globalWaveNumber; i++)
                    {
                        currentWave.Add(new WaveObject(spawningSystem.Enemies[0], spawningSystem.EnemyScripts[0], colors[rand], globalWaveSpacing, 
                            TOPBOTTOM, isDarkened, WaveObject.Type.BASIC));
                    }
                }
            }
        }
        public BasicChunk(GameModel.GameColor[] spawnColors, bool dark) : base(spawnColors, dark)
        {
            baseDifficulty = 0;
            image = uiManager.EnemySprites[0];
            SetDifficulty(spawnColors);
        }
        public BasicChunk(GameModel.GameColor[] spawnColors) : base(spawnColors)
        {
            baseDifficulty = 0;
            image = uiManager.EnemySprites[0];
            SetDifficulty(spawnColors);
        }
    }
    
    public class FastChunk : Chunk
    {
        public override void Generate()
        {
            Debug.Log("Fast Chunk Added: Difficulty - "+difficulty);
            if (colors.Length == 1)
            {
                for (int i = 0; i < globalWaveNumber; i++)
                {
                    currentWave.Add(new WaveObject(spawningSystem.Enemies[1], spawningSystem.EnemyScripts[1], colors[0], globalWaveSpacing, 
                        TOPBOTTOM, isDarkened, WaveObject.Type.FAST));
                }
            }
            else
            {
                int rand = Random.Range(0, colors.Length);
                {
                    for (int i = 0; i < globalWaveNumber; i++)
                    {
                        currentWave.Add(new WaveObject(spawningSystem.Enemies[1], spawningSystem.EnemyScripts[1], colors[rand], globalWaveSpacing, 
                            TOPBOTTOM, isDarkened, WaveObject.Type.FAST));
                    }
                }
            }
        }

        public FastChunk(GameModel.GameColor[] spawnColors, bool dark) : base(spawnColors, dark)
        {
            baseDifficulty = 3;
            image = uiManager.EnemySprites[1];
            SetDifficulty(spawnColors);
        }
        public FastChunk(GameModel.GameColor[] spawnColors) : base(spawnColors)
        {
            baseDifficulty = 3;
            image = uiManager.EnemySprites[1];
            SetDifficulty(spawnColors);
        }
    }
    
    public class NinjaChunk : Chunk
    {
        public override void Generate()
        {
            Debug.Log("Ninja Chunk Added: Difficulty - "+difficulty);
            if (colors.Length == 1)
            {
                for (int i = 0; i < globalWaveNumber; i++)
                {
                    currentWave.Add(new WaveObject(spawningSystem.Enemies[2], spawningSystem.EnemyScripts[2], colors[0], globalWaveSpacing, 
                        LEFTRIGHT, isDarkened, WaveObject.Type.NINJA));
                }
            }
            else
            {
                int rand = Random.Range(0, colors.Length);
                {
                    for (int i = 0; i < globalWaveNumber; i++)
                    {
                        currentWave.Add(new WaveObject(spawningSystem.Enemies[2], spawningSystem.EnemyScripts[2], colors[rand], globalWaveSpacing, 
                            LEFTRIGHT, isDarkened, WaveObject.Type.NINJA));
                    }
                }
            }
        }

        public NinjaChunk(GameModel.GameColor[] spawnColors, bool dark) : base(spawnColors, dark)
        {
            baseDifficulty = 2;
            image = uiManager.EnemySprites[2];
            SetDifficulty(spawnColors);
        }
        public NinjaChunk(GameModel.GameColor[] spawnColors) : base(spawnColors)
        {
            baseDifficulty = 2;
            image = uiManager.EnemySprites[2];
            SetDifficulty(spawnColors);
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
        public bool darkened;
        public Type enemyType;

        public enum Type
        {
            BASIC,
            FAST,
            NINJA
        }
        
        public WaveObject(GameObject enemyObject, EnemyBehavior enemyScript, GameModel.GameColor col, float delay, int[] loc, bool dark, WaveObject.Type type)
        {
            body = enemyObject;
            script = enemyScript;
            color = col;
            delayUntilNext = delay;
            locationsToSpawn = loc;
            darkened = dark;
            enemyType = type;
        }
    }
}
