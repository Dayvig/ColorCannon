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
    public float baseTutorialSpacing;
    public float baseGlobalWaveSpeed;
    public int baseNumChunks;
    private static int globalWaveNumber;
    private static float globalWaveSpacing;
    private static float globalWaveSpeed;
    public static float tutorialSpacing;
    private int numChunks;
    List<Chunk> availableChunks = new List<Chunk>();
    public static List<WaveObject> currentWave = new List<WaveObject>();
    public List<EnemyBehavior> inactiveEnemies = new List<EnemyBehavior>();
    public List<GameModel.GameColor> currentColors = new List<GameModel.GameColor>();
    public List<int> chunkDifficulties = new List<int>();
    public int currentWaveIndex = 0;
    public List<Mechanic> currentMechanics = new List<Mechanic>();
    public List<Mechanic> newMechanics = new List<Mechanic>();

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

    public enum Mechanic
    {
        BASIC,
        DARK,
        FAST,
        NINJA,
        TWOCOLOR,
        THREECOLOR,
    }

    public List<Mechanic> basicMechanics = new List<Mechanic>{Mechanic.FAST, Mechanic.NINJA, Mechanic.TWOCOLOR, Mechanic.DARK};
    public List<Mechanic> medMechanics = new List<Mechanic>{Mechanic.THREECOLOR};

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
        tutorialSpacing = baseTutorialSpacing;
    }

    public void initialize()
    {
        addStartingChunks();
        newMechanics.Add(Mechanic.BASIC);
        
        globalWaveNumber = baseGlobalWaveNumber;
        globalWaveSpacing = baseGlobalWaveSpacing;
        numChunks = baseNumChunks;
        for (int i = 0; i < numChunks; i++)
        {
            chunkDifficulties.Add(1 + i/2);
            Debug.Log(chunkDifficulties[i]);
        }

        gameManager.addStartingUpgrades();
        generateFirstWave();
        
        enemyTimer = currentWave[currentWaveIndex].delayUntilNext;
    }

    void addStartingChunks()
    {
        addBasicChunks(1, currentMechanics.Contains(Mechanic.DARK));
    }

    void addBasicChunks(int colors, bool hasDark)
    {
        switch (colors)
        {
            case 1:
                GameModel.GameColor[] standardColors = {GameModel.GameColor.YELLOW, GameModel.GameColor.BLUE, GameModel.GameColor.RED};
                for (int i = 0; i < standardColors.Length; i++)
                {
                    availableChunks.Add(new BasicChunk(new[]{standardColors[i]}, hasDark));
                }
                availableChunks.Add(new BasicChunk(new[]{standardColors[0], standardColors[1]}, hasDark));
                availableChunks.Add(new BasicChunk(new[]{standardColors[0], standardColors[2]}, hasDark));
                availableChunks.Add(new BasicChunk(new[]{standardColors[1], standardColors[2]}, hasDark));
                availableChunks.Add(new BasicChunk(new[]{standardColors[0], standardColors[1], standardColors[2]}, hasDark));
                break;
            case 2:
                GameModel.GameColor[] mixedColors = {GameModel.GameColor.ORANGE, GameModel.GameColor.GREEN, GameModel.GameColor.PURPLE};
                for (int i = 0; i < mixedColors.Length; i++)
                {
                    availableChunks.Add(new BasicChunk(new[]{mixedColors[i]}));
                }
                availableChunks.Add(new BasicChunk(new[]{mixedColors[0], mixedColors[1]}, hasDark));
                availableChunks.Add(new BasicChunk(new[]{mixedColors[0], mixedColors[2]}, hasDark));
                availableChunks.Add(new BasicChunk(new[]{mixedColors[1], mixedColors[2]}, hasDark));
                availableChunks.Add(new BasicChunk(new[]{mixedColors[0], mixedColors[1], mixedColors[2]}, hasDark));
                break;
            case 3:
                availableChunks.Add(new BasicChunk(new[] {GameModel.GameColor.WHITE}, hasDark));
                break;
        }
        if (hasDark)
        {
            addBasicChunks(colors, false);
        }
    }
    
    void addFastChunks(int colors, bool hasDark)
    {
        switch (colors)
        {
            case 1:
                GameModel.GameColor[] standardColors = {GameModel.GameColor.YELLOW, GameModel.GameColor.BLUE, GameModel.GameColor.RED};
                for (int i = 0; i < standardColors.Length; i++)
                {
                    availableChunks.Add(new FastChunk(new[]{standardColors[i]}, hasDark));
                }
                availableChunks.Add(new FastChunk(new[]{standardColors[0], standardColors[1]}, hasDark));
                availableChunks.Add(new FastChunk(new[]{standardColors[0], standardColors[2]}, hasDark));
                availableChunks.Add(new FastChunk(new[]{standardColors[1], standardColors[2]}, hasDark));
                availableChunks.Add(new FastChunk(new[]{standardColors[0], standardColors[1], standardColors[2]}, hasDark));
                break;
            case 2:
                GameModel.GameColor[] mixedColors = {GameModel.GameColor.ORANGE, GameModel.GameColor.GREEN, GameModel.GameColor.PURPLE};
                for (int i = 0; i < mixedColors.Length; i++)
                {
                    availableChunks.Add(new FastChunk(new[]{mixedColors[i]}, hasDark));
                }
                availableChunks.Add(new FastChunk(new[]{mixedColors[0], mixedColors[1]}, hasDark));
                availableChunks.Add(new FastChunk(new[]{mixedColors[0], mixedColors[2]}, hasDark));
                availableChunks.Add(new FastChunk(new[]{mixedColors[1], mixedColors[2]}, hasDark));
                availableChunks.Add(new FastChunk(new[]{mixedColors[0], mixedColors[1], mixedColors[2]}, hasDark));
                break;
            case 3:
                availableChunks.Add(new FastChunk(new[] {GameModel.GameColor.WHITE}));
                break;
        }
        if (hasDark)
        {
            addFastChunks(colors, false);
        }
    }
    
    void addNinjaChunks(int colors, bool hasDark)
    {
        switch (colors)
        {
            case 1:
                GameModel.GameColor[] standardColors = {GameModel.GameColor.YELLOW, GameModel.GameColor.BLUE, GameModel.GameColor.RED};
                for (int i = 0; i < standardColors.Length; i++)
                {
                    availableChunks.Add(new NinjaChunk(new[]{standardColors[i]}, hasDark));
                }
                availableChunks.Add(new NinjaChunk(new[]{standardColors[0], standardColors[1]}, hasDark));
                availableChunks.Add(new NinjaChunk(new[]{standardColors[0], standardColors[2]}, hasDark));
                availableChunks.Add(new NinjaChunk(new[]{standardColors[1], standardColors[2]}, hasDark));
                availableChunks.Add(new NinjaChunk(new[]{standardColors[0], standardColors[1], standardColors[2]}, hasDark));
                break;
            case 2:
                GameModel.GameColor[] mixedColors = {GameModel.GameColor.ORANGE, GameModel.GameColor.GREEN, GameModel.GameColor.PURPLE};
                for (int i = 0; i < mixedColors.Length; i++)
                {
                    availableChunks.Add(new NinjaChunk(new[]{mixedColors[i]}));
                }
                availableChunks.Add(new NinjaChunk(new[]{mixedColors[0], mixedColors[1]}, hasDark));
                availableChunks.Add(new NinjaChunk(new[]{mixedColors[0], mixedColors[2]}, hasDark));
                availableChunks.Add(new NinjaChunk(new[]{mixedColors[1], mixedColors[2]}, hasDark));
                availableChunks.Add(new NinjaChunk(new[]{mixedColors[0], mixedColors[1], mixedColors[2]}, hasDark));
                break;
            case 3:
                availableChunks.Add(new NinjaChunk(new[] {GameModel.GameColor.WHITE}, hasDark));
                break;
        }
        if (hasDark)
        {
            addNinjaChunks(colors, false);
        }
    }

    void addTwoColorChunks(bool hasDark)
    {
        addBasicChunks(2, hasDark);
        if (currentMechanics.Contains(Mechanic.FAST))
        {
            addFastChunks(2, hasDark);
        }
        if (currentMechanics.Contains(Mechanic.NINJA))
        {
            addNinjaChunks(2, hasDark);
        }
        if (currentMechanics.Contains(Mechanic.NINJA))
        {
            addNinjaChunks(2, hasDark);
        }
    }
    
    void addThreeColorChunks(bool hasDark)
    {
        addBasicChunks(3, hasDark);
        if (currentMechanics.Contains(Mechanic.FAST))
        {
            addFastChunks(3, hasDark);
        }
        if (currentMechanics.Contains(Mechanic.NINJA))
        {
            addNinjaChunks(3, hasDark);
        }
        if (currentMechanics.Contains(Mechanic.NINJA))
        {
            addNinjaChunks(3, hasDark);
        }
    }
    
    void addDarkChunks()
    {
        List<Chunk> toAdd = new List<Chunk>();
        foreach (Chunk c in availableChunks)
        {
            if (c.isDarkened == false)
            {
                Chunk nu = c.MakeCopy();
                nu.isDarkened = true;
                toAdd.Add(nu);
            }
        }
        foreach (Chunk c2 in toAdd)
        {
            availableChunks.Add(c2);
        }
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
            bool isTutorial = false;
            foreach (Mechanic m in newMechanics)
            {
                if (ChunkIsMechanic(m, nextChunk))
                {
                    isTutorial = true;
                    currentMechanics.Add(m);
                }
            }
            nextChunk.Generate(isTutorial);
            foreach (Mechanic m2 in currentMechanics)
            {
                if (newMechanics.Contains(m2))
                {
                    newMechanics.Remove(m2);
                }
            }
        }
    }

    public void generateFirstWave()
    {
        uiManager.WipePreviewImages();
        Chunk nextChunk = new BasicChunk(new[] {GameModel.GameColor.RED, GameModel.GameColor.BLUE, GameModel.GameColor.YELLOW});
        uiManager.SetupChunkPreview(nextChunk);

        currentWave.Add(new WaveObject(Enemies[0], EnemyScripts[0], GameModel.GameColor.RED,  
            tutorialSpacing,
            TOPBOTTOM, false, WaveObject.Type.BASIC));

        currentWave.Add(new WaveObject(Enemies[0], EnemyScripts[0], GameModel.GameColor.BLUE,  
            tutorialSpacing,
            TOPBOTTOM, false, WaveObject.Type.BASIC));

        currentWave.Add(new WaveObject(Enemies[0], EnemyScripts[0], GameModel.GameColor.YELLOW,  
            tutorialSpacing,
            TOPBOTTOM, false, WaveObject.Type.BASIC));

        nextChunk.Generate(false);
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
        //every sixth wave, adds an extra chunk
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
        //every third wave, introduces a new mechanic.
        if (Level % 3 == 0)
        {
            if (Level > 15)
            {
                addRandomMechanic(2);
            }
            else
            {
                addRandomMechanic(1);
            }
        }

        if (Level % 2 == 0)
        {
            //Every other wave makes waves harder with a modifier.
            int randomWaveMod = Random.Range(0, 3);
            switch (randomWaveMod)
            {
                case 0:
                    globalWaveNumber = (int)(globalWaveNumber * 1.2);
                    break;
                case 1:
                    globalWaveSpacing /= 1.2f;
                    break;
                case 2:
                    for (int k = 0; k < chunkDifficulties.Count; k++)
                    {
                        chunkDifficulties[k]++;
                    }
                    break;
            }
        }
        int rand = Random.Range(0, chunkDifficulties.Count);
        chunkDifficulties[rand] = (int)Math.Ceiling(chunkDifficulties[rand] * 1.5f);
        for (int i = 0; i < chunkDifficulties.Count; i++)
        {
            Debug.Log(chunkDifficulties[i]);
        }
    }

    void addRandomMechanic(int level)
    {
        int rand;
        switch (level)
        {
            case 1:
                if (basicMechanics.Count == 0)
                {
                    return;
                }
                rand = Random.Range(0, basicMechanics.Count);
                newMechanics.Add(basicMechanics[rand]);
                implementMechanic(basicMechanics[rand]);
                basicMechanics.Remove(basicMechanics[rand]);
                break;
            case 2:
                if (medMechanics.Count == 0)
                {
                    return;
                }
                rand = Random.Range(0, medMechanics.Count);
                newMechanics.Add(medMechanics[rand]);
                implementMechanic(medMechanics[rand]);
                medMechanics.Remove(medMechanics[rand]);
                break;
        }
    }

    void implementMechanic(Mechanic newMechanic)
    {
        switch (newMechanic)
        {
            case Mechanic.BASIC:
                addBasicChunks(1, currentMechanics.Contains(Mechanic.DARK));
                break;
            case Mechanic.FAST:
                addFastChunks(1, currentMechanics.Contains(Mechanic.DARK));
                break;
            case Mechanic.NINJA:
                addNinjaChunks(1, currentMechanics.Contains(Mechanic.DARK));
                break;
            case Mechanic.TWOCOLOR:
                addTwoColorChunks(currentMechanics.Contains(Mechanic.DARK));
                break;
            case Mechanic.DARK:
                addDarkChunks();
                break;
            case Mechanic.THREECOLOR:
                addThreeColorChunks(currentMechanics.Contains(Mechanic.DARK));
                break;
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
            if (colors.Contains(GameModel.GameColor.WHITE))
            {
                isDarkened = false;
            }
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
        public abstract void Generate(bool tutorial);
        public abstract Chunk MakeCopy();
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
        public override void Generate(bool tutorial)
        {
            Debug.Log("Basic Chunk Added: Difficulty - "+difficulty + " / "+ String.Join("",
                new List<GameModel.GameColor>(colors)
                    .ConvertAll(i => i.ToString())
                    .ToArray())+ " | "+isDarkened);
            if (colors.Length == 1)
            {
                for (int i = 0; i < globalWaveNumber; i++)
                {
                    currentWave.Add(new WaveObject(spawningSystem.Enemies[0], spawningSystem.EnemyScripts[0], colors[0],  
                        tutorial && i < 3 ? tutorialSpacing : globalWaveSpacing,
                        TOPBOTTOM, isDarkened, WaveObject.Type.BASIC));
                }
            }
            else
            {
                int rand = Random.Range(0, colors.Length);
                {
                    for (int i = 0; i < globalWaveNumber; i++)
                    {
                        currentWave.Add(new WaveObject(spawningSystem.Enemies[0], spawningSystem.EnemyScripts[0], colors[rand],  
                            tutorial && i < 3 ? tutorialSpacing : globalWaveSpacing, 
                            TOPBOTTOM, isDarkened, WaveObject.Type.BASIC));
                        rand = Random.Range(0, colors.Length);
                    }
                }
            }
        }

        public override Chunk MakeCopy()
        {
            return new BasicChunk(this.colors, this.isDarkened);
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
        public override void Generate(bool tutorial)
        {
            Debug.Log("Fast Chunk Added: Difficulty - "+difficulty);
            if (colors.Length == 1)
            {
                for (int i = 0; i < globalWaveNumber; i++)
                {
                    currentWave.Add(new WaveObject(spawningSystem.Enemies[1], spawningSystem.EnemyScripts[1], colors[0],  
                        tutorial && i < 3 ? tutorialSpacing : globalWaveSpacing,
                        TOPBOTTOM, isDarkened, WaveObject.Type.FAST));
                    
                }
            }
            else
            {
                int rand = Random.Range(0, colors.Length);
                {
                    for (int i = 0; i < globalWaveNumber; i++)
                    {
                        currentWave.Add(new WaveObject(spawningSystem.Enemies[1], spawningSystem.EnemyScripts[1], colors[rand],  
                            tutorial && i < 3 ? tutorialSpacing : globalWaveSpacing, 
                            TOPBOTTOM, isDarkened, WaveObject.Type.FAST));
                        rand = Random.Range(0, colors.Length);
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
        
        public override Chunk MakeCopy()
        {
            return new FastChunk(this.colors, this.isDarkened);
        }
    }
    
    public class NinjaChunk : Chunk
    {
        public override void Generate(bool tutorial)
        {
            Debug.Log("Ninja Chunk Added: Difficulty - "+difficulty);
            if (colors.Length == 1)
            {
                for (int i = 0; i < globalWaveNumber; i++)
                {
                    currentWave.Add(new WaveObject(spawningSystem.Enemies[2], spawningSystem.EnemyScripts[2], colors[0], 
                        tutorial && i < 3 ? tutorialSpacing : globalWaveSpacing, 
                        LEFTRIGHT, isDarkened, WaveObject.Type.NINJA));
                }
            }
            else
            {
                int rand = Random.Range(0, colors.Length);
                {
                    for (int i = 0; i < globalWaveNumber; i++)
                    {
                        currentWave.Add(new WaveObject(spawningSystem.Enemies[2], spawningSystem.EnemyScripts[2], colors[rand],  
                            tutorial && i < 3 ? tutorialSpacing : globalWaveSpacing, 
                            LEFTRIGHT, isDarkened, WaveObject.Type.NINJA));
                        rand = Random.Range(0, colors.Length);
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
        public override Chunk MakeCopy()
        {
            return new NinjaChunk(colors, isDarkened);
        }
    }

    public bool ChunkIsMechanic(Mechanic m, Chunk c)
    {
        switch (m)
        {
            case Mechanic.BASIC:
                return c is BasicChunk;
            case Mechanic.FAST:
                return c is FastChunk;
            case Mechanic.NINJA:
                return c is NinjaChunk;
            case Mechanic.DARK:
                return c.isDarkened;
            case Mechanic.TWOCOLOR:
                return c.isMultiColor;
            case Mechanic.THREECOLOR:
                return c.isMultiColor && c.colors.Length == 3;
        }

        return false;
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
