using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class WaveSpawningSystem : MonoBehaviour, IDataPersistence
{
    public List<GameObject> Enemies = new List<GameObject>();
    public List<EnemyBehavior> EnemyScripts = new List<EnemyBehavior>();

    public int Level;
    private static int globalWaveNumber;
    private static float globalWaveSpacing;
    private static float globalWaveSpeed;
    public static float tutorialSpacing;
    public int numUniqueChunks;
    public int numChunks;
    public List<Chunk> availableChunks = new List<Chunk>();
    public List<Chunk> availableSpecialChunks = new List<Chunk>();
    public static List<WaveObject> currentWave = new List<WaveObject>();
    public List<EnemyBehavior> inactiveEnemies = new List<EnemyBehavior>();
    public List<GameModel.GameColor> currentColors = new List<GameModel.GameColor>();
    public List<int> chunkDifficulties = new List<int>();
    public int currentWaveIndex = 0;
    public List<Mechanic> currentMechanics = new List<Mechanic>();
    public List<Mechanic> newMechanics = new List<Mechanic>();

    [SerializeField]
    private float enemyTimer = 0.0f;
    private float xBounds = 3;
    private float yBounds = 5;
    
    public GameModel modelGame;
    public GameObject player;
    public GameManager gameManager;

    private int recusionProtection = 0;
    public int enemiesToSpawn;

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
        ORANGE,
        GREEN,
        PURPLE,
        WHITE,
        SWARM,
        ZIGZAG,
        DISGUISED,
        SWIRL
    }

    public List<Mechanic> basicMechanics = new List<Mechanic>{Mechanic.FAST, Mechanic.NINJA, Mechanic.ORANGE, Mechanic.GREEN, Mechanic.PURPLE, Mechanic.DARK, Mechanic.SWARM, Mechanic.ZIGZAG};
    public List<Mechanic> medMechanics = new List<Mechanic>{Mechanic.WHITE, Mechanic.DISGUISED, Mechanic.SWIRL};

    public static int[] ALL =
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
                    startPos = new Vector3(-xBounds, Random.Range(yBounds/2, -yBounds/2), 0);
                    break;
                case 3:
                    startPos = new Vector3(xBounds, Random.Range(yBounds/2, -yBounds/2), 0);
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
            enemyScript.SetVisualColor(enemyColor);
            if (gameManager.activeEnemies.Contains(enemyScript))
            {
                gameManager.activeEnemies.Remove(enemyScript);
            }
            gameManager.activeEnemies.Add(enemyScript);
            inactiveEnemies.Remove(enemyScript);
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
        tutorialSpacing = modelGame.baseTutorialSpacing;
    }

    public void initialize()
    {
        repopulateChunks();
        
        globalWaveNumber = modelGame.baseGlobalWaveNumber;
        globalWaveSpacing = modelGame.baseGlobalWaveSpacing;
        numChunks = modelGame.baseNumChunks;
        numUniqueChunks = modelGame.baseNumUniqueChunks;

        gameManager.addStartingUpgrades();
        clearWave();
        if (Level == 1 && (!gameManager.encounteredEnemies.Contains(Mechanic.BASIC) || gameManager.encounteredEnemies.Count == 0))
        {
            Debug.Log("Spawning First Wave");
            generateFirstWave();
            UIManager.instance.WipeUpgrades();
        }
        else
        {
            Debug.Log("Spawning Normal Wave");
            generateWave();
            RandomizeWave();
            gameManager.GenerateUpgrades();
        }
        enemyTimer = currentWave[0].delayUntilNext;
        currentWaveIndex = 0;
        UIManager.instance.activatePostWaveUI();


        enemyTimer = currentWave[currentWaveIndex].delayUntilNext;
        //Level = 1;
    }

    List<GameModel.GameColor[]> twoColorPermutations(GameModel.GameColor[] colors)
    {
        List<GameModel.GameColor[]>permutations = new List<GameModel.GameColor[]>();

        //selects elements 1 apart, then continues until the space apart is 1 less than array size
        for (int spacer = 1; spacer < colors.Length; spacer++)
        {
            for (int i = 0; i < colors.Length; i++)
            {
                if (i + spacer < colors.Length)
                {
                    permutations.Add(new[] { colors[i], colors[i + spacer] });
                }
            }
        }

        return permutations;
    }

    List<GameModel.GameColor[]> oneColorPermutations(GameModel.GameColor[] colors)
    {
        List<GameModel.GameColor[]> permutations = new List<GameModel.GameColor[]>();
        //reads out each single color in array
        for (int i = 0; i < colors.Length; i++)
        {
                permutations.Add(new[] { colors[i] });
        }
        return permutations;
    }

    List<GameModel.GameColor> additionalColorsReadout()
    {
        List<GameModel.GameColor> readOut = new List<GameModel.GameColor>();
        foreach (Mechanic m in newMechanics)
        {
            switch (m)
            {
                case Mechanic.ORANGE:
                    readOut.Add(GameModel.GameColor.ORANGE);
                    break;
                case Mechanic.GREEN:
                    readOut.Add(GameModel.GameColor.GREEN);
                    break;
                case Mechanic.PURPLE:
                    readOut.Add(GameModel.GameColor.PURPLE);
                    break;
            }
        }
        foreach (Mechanic m in currentMechanics)
        {
            switch (m)
            {
                case Mechanic.ORANGE:
                    if (!readOut.Contains(GameModel.GameColor.ORANGE))
                        readOut.Add(GameModel.GameColor.ORANGE);
                    break;
                case Mechanic.GREEN:
                    if (!readOut.Contains(GameModel.GameColor.GREEN))
                    readOut.Add(GameModel.GameColor.GREEN);
                    break;
                case Mechanic.PURPLE:
                    if (!readOut.Contains(GameModel.GameColor.PURPLE))
                    readOut.Add(GameModel.GameColor.PURPLE);
                    break;
            }
        }
        return readOut;
    }

    void addAllChunkColors(Chunk toAdd)
    {
        GameModel.GameColor[] standardColors = {GameModel.GameColor.YELLOW, GameModel.GameColor.BLUE, GameModel.GameColor.RED};
        GameModel.GameColor[] additionalColors = additionalColorsReadout().ToArray();
        addChunks(standardColors, toAdd);
        addChunks(additionalColors, toAdd);

        if (newMechanics.Contains(Mechanic.WHITE) || currentMechanics.Contains(Mechanic.WHITE))
        {
            addChunks(new[] { GameModel.GameColor.WHITE }, toAdd);
        }
    }

    void addAllDarkChunks()
    {
        Chunk newChunk;
        List<Chunk> tempList = new List<Chunk>();
        foreach (Chunk toCopy in availableChunks)
        {
            newChunk = toCopy.MakeCopy(toCopy.colors, true);
            tempList.Add(newChunk);
        }
        foreach (Chunk c in tempList)
        {
            availableChunks.Add(c);
            availableSpecialChunks.Add(c);
        }
    }

    bool isNonBasic (Chunk c)
    {
        return !(c is BasicChunk) || c.isDarkened || c.isMultiColor;

    }

    void addChunks(GameModel.GameColor[] colors, Chunk toAdd)
    {
        Chunk newChunk;
        foreach (GameModel.GameColor[] singleColor in oneColorPermutations(colors))
        {
            newChunk = toAdd.MakeCopy(singleColor, false);
            availableChunks.Add(newChunk);
            if (isNonBasic(newChunk))
            {
                availableSpecialChunks.Add(newChunk);
            }
        }
        if (colors.Length > 1)
        {
            foreach (GameModel.GameColor[] colorPair in twoColorPermutations(colors))
            {
                newChunk = toAdd.MakeCopy(colorPair, false);
                availableChunks.Add(newChunk);
                if (isNonBasic(newChunk))
                {
                    availableSpecialChunks.Add(newChunk);
                }
            }
        }
        if (colors.Length > 2)
        {
            newChunk = toAdd.MakeCopy(new[] { colors[0], colors[1], colors[2] }, false);
            availableChunks.Add(newChunk);
            if (isNonBasic(newChunk))
            {
                availableSpecialChunks.Add(newChunk);
            }

        }

    }

    public Chunk returnRandomChunk(List<Chunk> chunklist)
    {
        return chunklist[Random.Range(0, chunklist.Count)];
    }

    public Chunk returnRandomChunk(List<Chunk> chunklist, int lessthan)
    {
        Chunk nextChunk = chunklist[Random.Range(0, chunklist.Count)];
        if (nextChunk.difficulty <= lessthan)
        {
            return nextChunk;
        }
        recusionProtection++;
        if (recusionProtection <= 100)
        {
            return returnRandomChunk(chunklist, lessthan);
        }
        Debug.Log("Tried to access a chunk that didn't exist");
        return chunklist[0];
    }

    public Chunk returnRandomChunk(List<Chunk> chunklist, int floor, int ceiling)
    {
        Chunk nextChunk = chunklist[Random.Range(0, chunklist.Count)];
        if (nextChunk.difficulty >= floor && nextChunk.difficulty <= ceiling)
        {
            return nextChunk;
        }
        recusionProtection++;
        if (recusionProtection <= 100)
        {
            return returnRandomChunk(chunklist, floor, ceiling);
        }
        Debug.Log("Tried to access a chunk that didn't exist");

        return chunklist[0];
    }

    public void generateWave()
    {
        Chunk nextChunk;
        UIManager.instance.WipePreviewImages();
        for (int i = 0; i < chunkDifficulties.Count; i++)
        {
            recusionProtection = 0;
            if (i < numUniqueChunks)
            {
                nextChunk = returnRandomChunk(availableSpecialChunks, chunkDifficulties[i]);
            }
            else
            {
                nextChunk = returnRandomChunk(availableChunks, chunkDifficulties[i]);
            }
            if (i < 4)
            {
                UIManager.instance.SetupChunkPreview(nextChunk, 1);
            }
            else if (i < 8)
            {
                UIManager.instance.SetupChunkPreview(nextChunk, 2);
            }
            else
            {
                Debug.Log("Eg");
                UIManager.instance.SetupChunkPreview(nextChunk, 3);
            }
            bool isTutorial = false;
            foreach (Mechanic m in newMechanics)
            {
                if (ChunkIsMechanic(m, nextChunk) && !gameManager.encounteredEnemies.Contains(m))
                {
                    isTutorial = true;
                    currentMechanics.Add(m);
                    gameManager.encounteredEnemies.Add(m);
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
        enemiesToSpawn = currentWave.Count - 1;
        UIManager.instance.SetupWaveModUI();
    }

    public void generateFirstWave()
    {
        UIManager.instance.WipePreviewImages();
        UIManager.instance.HideWaveMods();
        UIManager.instance.RefreshUpgradesButton.SetActive(false);
        Chunk nextChunk = new BasicChunk(new[] {GameModel.GameColor.RED, GameModel.GameColor.BLUE, GameModel.GameColor.YELLOW});
        UIManager.instance.SetupChunkPreview(nextChunk, 1);

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
        enemiesToSpawn = currentWave.Count-1;

        gameManager.encounteredEnemies.Add(Mechanic.BASIC);
    }



    public void SetupNextWave()
    {
        Debug.Log("Next Wave =================");
        UIManager.instance.WipePreviewImages();
        gameManager.currentOfferedUpgrades.Clear();
        clearWave();
        IncreaseDifficulty();
        generateWave();
        RandomizeWave();
        gameManager.GenerateUpgrades();
        enemyTimer = currentWave[0].delayUntilNext;
        currentWaveIndex = 0;
        UIManager.instance.activatePostWaveUI();
        SaveLoadManager.instance.SaveGame();
    }

    public void RandomizeWave()
    {
        for (int j = 0; j < currentWave.Count; j++)
        {
            WaveObject tmp = currentWave[j];
            int rand = Random.Range(0, currentWave.Count);
            currentWave[j] = currentWave[rand];
            currentWave[rand] = tmp;
        }
        MoveTutorialToFront();
    }

    private void MoveTutorialToFront()
    {
        //Moves tutorial wave objects to front
        int tutorialIndex = 0;
        for (int j = 0; j < currentWave.Count; j++)
        {
            WaveObject tmp = currentWave[j];
            if (tmp.isTutorial)
            {
                Debug.Log("Tutorial found");
                currentWave[j] = currentWave[tutorialIndex];
                currentWave[tutorialIndex] = tmp;
                tutorialIndex++;
            }
        }
    }

    public void IncreaseDifficulty()
    {
        Level++;
        if (Level > 30){
            gameManager.SetState(GameManager.GameState.WIN);
            return;
        }
        //every 12th wave, adds an extra chunk
        if (Level % 12 == 0)
        {
            double sum = 0;
            for (int k = 0; k < chunkDifficulties.Count; k++)
            {
                sum += chunkDifficulties[k];
            }
            numChunks++;
            chunkDifficulties.Add((int)Math.Ceiling((sum/chunkDifficulties.Count)));
        }
        //starting on wave 3, every 12th wave adds a guaranteed unique chunk
        if ((Level+9) % 12 == 0)
        {
            numUniqueChunks++;
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
                    globalWaveNumber += 2;
                    UIManager.instance.AddWaveMod(UIManager.WaveModifier.BIGGER_WAVE);
                    break;
                case 1:
                    globalWaveSpacing /= 1.2f;
                    UIManager.instance.AddWaveMod(UIManager.WaveModifier.NUMEROUS);
                    break;
                case 2:
                    for (int k = 0; k < chunkDifficulties.Count; k++)
                    {
                        chunkDifficulties[k]++;
                    }
                    UIManager.instance.AddWaveMod(UIManager.WaveModifier.DIFFICULT);
                    break;
            }
        }
        int rand = Random.Range(0, chunkDifficulties.Count);
        chunkDifficulties[rand] = (int)Math.Ceiling(chunkDifficulties[rand] * 1.5f);
        repopulateChunks();
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
                basicMechanics.Remove(basicMechanics[rand]);
                break;
            case 2:
                if (medMechanics.Count == 0)
                {
                    return;
                }
                rand = Random.Range(0, medMechanics.Count);
                newMechanics.Add(medMechanics[rand]);
                medMechanics.Remove(medMechanics[rand]);
                break;
        }
    }

    void repopulateChunks()
    {
        availableChunks.Clear();
        availableSpecialChunks.Clear();
        Debug.Log("Repopulating Chunks");

        //enemy types
        addAllChunkColors(new BasicChunk(new[] { GameModel.GameColor.NONE }, false));
        if (newMechanics.Contains(Mechanic.FAST) || currentMechanics.Contains(Mechanic.FAST))
        {
            addAllChunkColors(new FastChunk(new[] { GameModel.GameColor.NONE }, false));
        }
        if (newMechanics.Contains(Mechanic.NINJA) || currentMechanics.Contains(Mechanic.NINJA))
        {
            addAllChunkColors(new NinjaChunk(new[] { GameModel.GameColor.NONE }, false));
        }
        if (newMechanics.Contains(Mechanic.SWARM) || currentMechanics.Contains(Mechanic.SWARM))
        {
            addAllChunkColors(new SwarmChunk(new[] { GameModel.GameColor.NONE }, false));
        }
        if (newMechanics.Contains(Mechanic.ZIGZAG) || currentMechanics.Contains(Mechanic.ZIGZAG))
        {
            addAllChunkColors(new ZigZagChunk(new[] { GameModel.GameColor.NONE }, false));
        }
        if (newMechanics.Contains(Mechanic.DISGUISED) || currentMechanics.Contains(Mechanic.DISGUISED))
        {
            addAllChunkColors(new DisguiserChunk(new[] { GameModel.GameColor.NONE }, false));
        }
        if (newMechanics.Contains(Mechanic.SWIRL) || currentMechanics.Contains(Mechanic.SWIRL))
        {
            addAllChunkColors(new SwirlChunk(new[] { GameModel.GameColor.NONE }, false));
        }

        //modifiers (Dark only for now)
        if (newMechanics.Contains(Mechanic.DARK) || currentMechanics.Contains(Mechanic.DARK))
        {
            addAllDarkChunks();
        }
    }   

    public void clearWave()
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
        public string name;
        public bool isTutorialChunk;

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
        public virtual void Generate(bool tutorial)
        {
            isTutorialChunk = tutorial;
            int numToSpawn = globalWaveNumber;

            //spawns 3 tutorial enemies
            if (isTutorialChunk)
            {
                for (int i = 0; i < 3; i++)
                {
                    currentWave.Insert(0, ChunkToWaveObject(isTutorialChunk));
                }
                numToSpawn -= 3;
                if (numToSpawn <= 0) { return; }
            }

            //spawns the rest of the wave
            for (int i = 0; i < numToSpawn; i++)
              {
                    currentWave.Add(ChunkToWaveObject(false));
              }
        }

        public virtual void Generate(bool tutorial, int enemiesToSpawn)
        {
            isTutorialChunk = tutorial;
            int numToSpawn = enemiesToSpawn;

            //spawns 3 tutorial enemies
            if (isTutorialChunk)
            {
                for (int i = 0; i < 3; i++)
                {
                    currentWave.Add(ChunkToWaveObject(isTutorialChunk));
                }
                numToSpawn -= 3;
                if (numToSpawn <= 0) { return; }
            }

            //spawns the rest of the wave
            for (int i = 0; i < numToSpawn; i++)
            {
                currentWave.Add(ChunkToWaveObject(false));
            }
        }

        public abstract Chunk MakeCopy();
        public abstract Chunk MakeCopy(GameModel.GameColor[] colors, bool isDark);

        public virtual void SetDifficulty(GameModel.GameColor[] spawnColors)
        {
            difficulty = baseDifficulty + spawnColors.Length;
            if (isMultiColor)
            {
                difficulty *= 2;
                if (difficulty < 6)
                {
                    difficulty = 6;
                }
            }

            if (isDarkened)
            {
                difficulty *= 2;
                if (difficulty < 4)
                {
                    difficulty = 4;
                }
            }
        }

        public abstract WaveObject ChunkToWaveObject(bool tutorial);
    }

    public class BasicChunk : Chunk
    {
        public override Chunk MakeCopy()
        {
            return new BasicChunk(this.colors, this.isDarkened);
        }

        public override Chunk MakeCopy(GameModel.GameColor[] colors, bool isDark)
        {
            return new BasicChunk(colors, isDark);
        }

        public override WaveObject ChunkToWaveObject(bool isTutorial)
        {
            float spacing = isTutorial ? tutorialSpacing : globalWaveSpacing;
            WaveObject toReturn;
            if (colors.Length == 1)
            {
                toReturn = new WaveObject(spawningSystem.Enemies[0], spawningSystem.EnemyScripts[0], colors[0], spacing, TOPBOTTOM, isDarkened, WaveObject.Type.BASIC);
            }
            else
            {
                int rand = Random.Range(0, colors.Length);
                toReturn = new WaveObject(spawningSystem.Enemies[0], spawningSystem.EnemyScripts[0], colors[rand], spacing, TOPBOTTOM, isDarkened, WaveObject.Type.BASIC);
            }
            toReturn.isTutorial = isTutorial;
            return toReturn;
        }

        public BasicChunk(GameModel.GameColor[] spawnColors, bool dark) : base(spawnColors, dark)
        {
            name = "Basic";
            baseDifficulty = 0;
            image = UIManager.instance.EnemySprites[0];
            SetDifficulty(spawnColors);
        }
        public BasicChunk(GameModel.GameColor[] spawnColors) : base(spawnColors)
        {
            name = "Basic";
            baseDifficulty = 0;
            image = UIManager.instance.EnemySprites[0];
            SetDifficulty(spawnColors);
        }
    }
    
    public class FastChunk : Chunk
    {
        public FastChunk(GameModel.GameColor[] spawnColors, bool dark) : base(spawnColors, dark)
        {
            name = "Fast";
            baseDifficulty = 3;
            image = UIManager.instance.EnemySprites[1];
            SetDifficulty(spawnColors);
        }
        public FastChunk(GameModel.GameColor[] spawnColors) : base(spawnColors)
        {
            name = "Fast";
            baseDifficulty = 3;
            image = UIManager.instance.EnemySprites[1];
            SetDifficulty(spawnColors);
        }
        public override WaveObject ChunkToWaveObject(bool isTutorial)
        {
            float spacing = isTutorial ? tutorialSpacing : globalWaveSpacing;
            WaveObject toReturn;
            if (colors.Length == 1)
            {
                toReturn = new WaveObject(spawningSystem.Enemies[1], spawningSystem.EnemyScripts[1], colors[0], spacing, TOPBOTTOM, isDarkened, WaveObject.Type.FAST);
            }
            else
            {
                int rand = Random.Range(0, colors.Length);
                toReturn = new WaveObject(spawningSystem.Enemies[1], spawningSystem.EnemyScripts[1], colors[rand], spacing, TOPBOTTOM, isDarkened, WaveObject.Type.FAST);
            }
            toReturn.isTutorial = isTutorial;
                return toReturn;
        }

        public override Chunk MakeCopy()
        {
            return new FastChunk(this.colors, this.isDarkened);
        }
        public override Chunk MakeCopy(GameModel.GameColor[] colors, bool isDark)
        {
            return new FastChunk(colors, isDark);
        }
    }
    
    public class NinjaChunk : Chunk
    {
        public NinjaChunk(GameModel.GameColor[] spawnColors, bool dark) : base(spawnColors, dark)
        {
            name = "Ninja";
            baseDifficulty = 2;
            image = UIManager.instance.EnemySprites[2];
            SetDifficulty(spawnColors);
        }
        public NinjaChunk(GameModel.GameColor[] spawnColors) : base(spawnColors)
        {
            name = "Ninja";
            baseDifficulty = 2;
            image = UIManager.instance.EnemySprites[2];
            SetDifficulty(spawnColors);
        }

        public override WaveObject ChunkToWaveObject(bool isTutorial)
        {
            float spacing = isTutorial ? tutorialSpacing : globalWaveSpacing;
            WaveObject toReturn;
            if (colors.Length == 1)
            {
                toReturn = new WaveObject(spawningSystem.Enemies[2], spawningSystem.EnemyScripts[2], colors[0], spacing, LEFTRIGHT, isDarkened, WaveObject.Type.NINJA);
            }
            else
            {
                int rand = Random.Range(0, colors.Length);
                toReturn = new WaveObject(spawningSystem.Enemies[2], spawningSystem.EnemyScripts[2], colors[rand], spacing, LEFTRIGHT, isDarkened, WaveObject.Type.NINJA);
            }
            toReturn.isTutorial = isTutorial;
            return toReturn;
        }

        public override Chunk MakeCopy()
        {
            return new NinjaChunk(colors, isDarkened);
        }
        public override Chunk MakeCopy(GameModel.GameColor[] colors, bool isDark)
        {
            return new NinjaChunk(colors, isDark);
        }
    }

    public class SwarmChunk : Chunk
    {
        private static float SWARMSPACINGMULT = 0.5f;
        public SwarmChunk(GameModel.GameColor[] spawnColors, bool dark) : base(spawnColors, dark)
        {
            name = "Swarm";
            baseDifficulty = 3;
            image = UIManager.instance.EnemySprites[3];
            SetDifficulty(spawnColors);
        }
        public SwarmChunk(GameModel.GameColor[] spawnColors) : base(spawnColors)
        {
            name = "Swarm";
            baseDifficulty = 3;
            image = UIManager.instance.EnemySprites[3];
            SetDifficulty(spawnColors);
        }

        public override WaveObject ChunkToWaveObject(bool isTutorial)
        {
            float spacing = isTutorial ? tutorialSpacing : globalWaveSpacing;
            WaveObject toReturn;
            if (colors.Length == 1)
            {
                toReturn = new WaveObject(spawningSystem.Enemies[3], spawningSystem.EnemyScripts[3], colors[0], spacing * SWARMSPACINGMULT, ALL, isDarkened, WaveObject.Type.SWARM);
            }
            else
            {
                int rand = Random.Range(0, colors.Length);
                toReturn = new WaveObject(spawningSystem.Enemies[3], spawningSystem.EnemyScripts[3], colors[rand], spacing * SWARMSPACINGMULT, ALL, isDarkened, WaveObject.Type.SWARM);
            }
            toReturn.isTutorial = isTutorial;
            return toReturn;
        }

        public override void Generate(bool tutorial)
        {
            base.Generate(tutorial, globalWaveNumber * 2);
        }

        public override Chunk MakeCopy()
        {
            return new SwarmChunk(colors, isDarkened);
        }
        public override Chunk MakeCopy(GameModel.GameColor[] colors, bool isDark)
        {
            return new SwarmChunk(colors, isDark);
        }
    }

    public class ZigZagChunk : Chunk
    {
        public ZigZagChunk(GameModel.GameColor[] spawnColors, bool dark) : base(spawnColors, dark)
        {
            name = "Zigzag";
            baseDifficulty = 5;
            image = UIManager.instance.EnemySprites[4];
            SetDifficulty(spawnColors);
        }
        public ZigZagChunk(GameModel.GameColor[] spawnColors) : base(spawnColors)
        {
            name = "Zigzag";
            baseDifficulty = 5;
            image = UIManager.instance.EnemySprites[4];
            SetDifficulty(spawnColors);
        }

        public override WaveObject ChunkToWaveObject(bool isTutorial)
        {
            float spacing = isTutorial ? tutorialSpacing : globalWaveSpacing;
            WaveObject toReturn;
            if (colors.Length == 1)
            {
                toReturn = new WaveObject(spawningSystem.Enemies[4], spawningSystem.EnemyScripts[4], colors[0], spacing, TOPBOTTOM, isDarkened, WaveObject.Type.ZIGZAG);
            }
            else
            {
                int rand = Random.Range(0, colors.Length);
                toReturn = new WaveObject(spawningSystem.Enemies[4], spawningSystem.EnemyScripts[4], colors[rand], spacing, TOPBOTTOM, isDarkened, WaveObject.Type.ZIGZAG);
            }
            toReturn.isTutorial = isTutorial;
            return toReturn;
        }

        public override Chunk MakeCopy()
        {
            return new ZigZagChunk(colors, isDarkened);
        }
        public override Chunk MakeCopy(GameModel.GameColor[] colors, bool isDark)
        {
            return new ZigZagChunk(colors, isDark);
        }
    }

    public class DisguiserChunk : Chunk
    {
        public DisguiserChunk(GameModel.GameColor[] spawnColors, bool dark) : base(spawnColors, dark)
        {
            name = "Disguiser";
            baseDifficulty = 5;
            image = UIManager.instance.EnemySprites[5];
            SetDifficulty(spawnColors);
        }
        public DisguiserChunk(GameModel.GameColor[] spawnColors) : base(spawnColors)
        {
            name = "Disguiser";
            baseDifficulty = 5;
            image = UIManager.instance.EnemySprites[5];
            SetDifficulty(spawnColors);
        }

        public override WaveObject ChunkToWaveObject(bool isTutorial)
        {
            float spacing = isTutorial ? tutorialSpacing : globalWaveSpacing;
            WaveObject toReturn;
            if (colors.Length == 1)
            {
                toReturn = new WaveObject(spawningSystem.Enemies[5], spawningSystem.EnemyScripts[5], colors[0], spacing, TOPBOTTOM, isDarkened, WaveObject.Type.DISGUISED);
            }
            else
            {
                int rand = Random.Range(0, colors.Length);
                toReturn = new WaveObject(spawningSystem.Enemies[5], spawningSystem.EnemyScripts[5], colors[rand], spacing, TOPBOTTOM, isDarkened, WaveObject.Type.DISGUISED);
            }
            toReturn.isTutorial = isTutorial;
            return toReturn;
        }

        public override Chunk MakeCopy()
        {
            return new DisguiserChunk(colors, isDarkened);
        }
        public override Chunk MakeCopy(GameModel.GameColor[] colors, bool isDark)
        {
            return new DisguiserChunk(colors, isDark);
        }
    }

    public class SwirlChunk : Chunk
    {
        public SwirlChunk(GameModel.GameColor[] spawnColors, bool dark) : base(spawnColors, dark)
        {
            name = "Swirl";
            baseDifficulty = 4;
            image = UIManager.instance.EnemySprites[6];
            SetDifficulty(spawnColors);
        }
        public SwirlChunk(GameModel.GameColor[] spawnColors) : base(spawnColors)
        {
            name = "Swirl";
            baseDifficulty = 4;
            image = UIManager.instance.EnemySprites[6];
            SetDifficulty(spawnColors);
        }

        public override WaveObject ChunkToWaveObject(bool isTutorial)
        {
            float spacing = isTutorial ? tutorialSpacing : globalWaveSpacing;
            WaveObject toReturn;
            if (colors.Length == 1)
            {
                toReturn = new WaveObject(spawningSystem.Enemies[6], spawningSystem.EnemyScripts[6], colors[0], spacing, TOPBOTTOM, isDarkened, WaveObject.Type.SWIRL);
            }
            else
            {
                int rand = Random.Range(0, colors.Length);
                toReturn = new WaveObject(spawningSystem.Enemies[6], spawningSystem.EnemyScripts[6], colors[rand], spacing, TOPBOTTOM, isDarkened, WaveObject.Type.SWIRL);
            }
            toReturn.isTutorial = isTutorial;
            return toReturn;
        }

        public override Chunk MakeCopy()
        {
            return new SwirlChunk(colors, isDarkened);
        }
        public override Chunk MakeCopy(GameModel.GameColor[] colors, bool isDark)
        {
            return new SwirlChunk(colors, isDark);
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
            case Mechanic.ORANGE:
                return c.colors.Contains(GameModel.GameColor.ORANGE);
            case Mechanic.GREEN:
                return c.colors.Contains(GameModel.GameColor.GREEN);
            case Mechanic.PURPLE:
                return c.colors.Contains(GameModel.GameColor.PURPLE);
            case Mechanic.WHITE:
                return c.colors.Contains(GameModel.GameColor.WHITE);
            case Mechanic.SWARM:
                return c is SwarmChunk;
            case Mechanic.ZIGZAG:
                return c is ZigZagChunk;
            case Mechanic.DISGUISED:
                return c is DisguiserChunk;
            case Mechanic.SWIRL:
                return c is SwirlChunk;

        }

        return false;
    }

    public void LoadData(GameData data)
    {
        Level = data.currentLevel;
        Debug.Log("Loaded Level :" + Level);
        currentMechanics = data.currentMechanics;
        newMechanics = data.currentNewMechanics;
        medMechanics = data.undiscoveredMedMechanics;
        basicMechanics = data.undiscoveredEasyMechanics;
        chunkDifficulties = data.chunkDifficulties.ToList();
    }

    public void SaveData(ref GameData data)
    {
        data.currentLevel = Level;
        data.currentMechanics = currentMechanics;
        data.currentNewMechanics = newMechanics;
        data.undiscoveredMedMechanics = medMechanics;
        data.undiscoveredEasyMechanics = basicMechanics;
        data.chunkDifficulties = chunkDifficulties.ToArray();
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
        public bool isTutorial;

        public enum Type
        {
            BASIC,
            FAST,
            NINJA,
            SWARM,
            ZIGZAG,
            DISGUISED,
            SWIRL
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
