using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GameManager;
using Random = UnityEngine.Random;

public class WaveSpawningSystem : MonoBehaviour, IDataPersistence
{
    public List<GameObject> Enemies = new List<GameObject>();
    public List<EnemyBehavior> EnemyScripts = new List<EnemyBehavior>();

    public int Level;
    public static int globalWaveNumber;
    public static float globalWaveSpacing;
    public static float globalWaveSpeed;
    public float globalRainbowMult;
    public static float tutorialSpacing;
    public int numUniqueChunks;
    public int numChunks;
    public List<Chunk> availableChunks = new List<Chunk>();
    public List<Chunk> availableSpecialChunks = new List<Chunk>();
    public List<Chunk> demoChunks = new List<Chunk>();
    public static List<WaveObject> currentWave = new List<WaveObject>();
    public static List<Chunk> currentChunks = new List<Chunk>();
    public List<EnemyBehavior> inactiveEnemies = new List<EnemyBehavior>();
    public List<DeathEffect> deathEffects = new List<DeathEffect>();
    public List<GameModel.GameColor> currentColors = new List<GameModel.GameColor>();
    public List<int> chunkDifficulties = new List<int>();
    public int currentWaveIndex = 0;
    public List<Mechanic> currentMechanics = new List<Mechanic>();
    public List<Mechanic> newMechanics = new List<Mechanic>();

    [SerializeField]
    public float enemyTimer = 0.0f;
    private float xBounds = 3;
    private float yBounds = 5;
    
    public GameModel modelGame;
    public GameObject player;
    public GameManager gameManager;

    private int recusionProtection = 0;
    public int enemiesToSpawn;

    public int tutorialStage;
    public List<Vector3> tutorialLocations = new List<Vector3>();

    public static WaveSpawningSystem instance { get; private set; }

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
        SWIRL,
        RAGE,
        PAINTER,
        EXPLOSIVE,
        SPLITTER,
        SWOOPER,
        TARGET
    }

    public List<Mechanic> basicMechanics;
    public List<Mechanic> medMechanics;

    public static int[] ALL =
        {0, 1, 2, 3};

    public static int[] TOPBOTTOM = {0, 1};
    public static int[] LEFTRIGHT = {2, 3};


    //================================================  Enemy Spawning ===============================================

    public void EnemyUpdate()
    {
        if (tutorialStage == -2)
        {
            enemyTimer -= Time.deltaTime;
            if (enemyTimer <= 0.0f && currentWaveIndex < currentWave.Count - 1)
            {
                CreateEnemy(currentWave[currentWaveIndex]);
                enemyTimer = currentWave[currentWaveIndex].delayUntilNext;

                if (currentWaveIndex < currentWave.Count - 1)
                {
                    currentWaveIndex++;
                }
            }
        }
        
        else
        {
            if (enemyTimer != -1f)
            {
                enemyTimer -= Time.deltaTime;
            }
            if (enemyTimer <= 0.0f && enemyTimer != -1f)
            {
                CreateEnemyAtLocation(tutorialLocations[tutorialStage], currentWave[tutorialStage]);
                if (tutorialStage == 4 || tutorialStage == 5 || tutorialStage == 6)
                {
                    tutorialStage++;
                    enemyTimer = currentWave[tutorialStage].delayUntilNext;
                }
                else
                {
                    enemyTimer = -1f;
                }
            }

            if (tutorialStage > tutorialLocations.Count - 1)
            {
                EndTutorial();
            }
        }

    }

    public void NextTutorialStage()
    {
        if (tutorialStage != -2)
        {
            tutorialStage++;
            if (tutorialStage > tutorialLocations.Count - 1)
            {
                EndTutorial();
                return;
            }
            enemyTimer = currentWave[tutorialStage].delayUntilNext;
        }
    }

    public void EndTutorial()
    {
        currentWaveIndex = currentWave.Count;
        tutorialStage = -2;
        UIManager.instance.tutorialToggle.init();
    }

    public GameObject CreateEnemy(WaveObject nextWaveObject)
    {
        WaveObject enemy = nextWaveObject;
        int EdgeToSpawnFrom = enemy.locationsToSpawn[Random.Range(0, enemy.locationsToSpawn.Length)];
        Vector3 startPos;
        switch (EdgeToSpawnFrom)
        {
            case 0:
                startPos = new Vector3(Random.Range(xBounds * 1.2f, -xBounds * 1.2f), yBounds, 0);
                break;
            case 1:
                startPos = new Vector3(Random.Range(xBounds * 1.2f, -xBounds * 1.2f), -yBounds, 0);
                break;
            case 2:
                startPos = new Vector3(-xBounds * 1.2f, Random.Range(yBounds / 2, -yBounds / 2), 0);
                break;
            case 3:
                startPos = new Vector3(xBounds * 1.2f, Random.Range(yBounds / 2, -yBounds / 2), 0);
                break;
            default:
                startPos = new Vector3(Random.Range(xBounds * 1.2f, -xBounds * 1.2f), yBounds, 0);
                break;
        }

        GameObject enemyObject = null;
        EnemyBehavior enemyScript;
        foreach (EnemyBehavior w in inactiveEnemies)
        {
            if (w.enemyType == nextWaveObject.enemyType)
            {
                enemyObject = w.gameObject;
                break;
            }
        }
        if (enemyObject == null)
        {
            enemyObject = Instantiate(nextWaveObject.body, startPos, Quaternion.identity);
        }

        enemyObject.SetActive(true);
        enemyObject.transform.position = startPos;
        enemyScript = enemyObject.GetComponent<EnemyBehavior>();
        GameModel.GameColor enemyColor = nextWaveObject.color;
        bool darkEnemy = nextWaveObject.darkened;
        enemyScript.initialize(player.transform.position, enemyColor, darkEnemy, enemyScript.enemyType);
        enemyScript.SetVisualColor(enemyColor);
        if (gameManager.activeEnemies.Contains(enemyScript))
        {
            gameManager.activeEnemies.Remove(enemyScript);
        }
        gameManager.activeEnemies.Add(enemyScript);
        inactiveEnemies.Remove(enemyScript);

        return enemyObject;
    }

    public GameObject CreateEnemyAtLocation(Vector3 location, WaveObject nextWaveObject)
    {
        WaveObject enemy = nextWaveObject;

        GameObject enemyObject = null;
        EnemyBehavior enemyScript;
        foreach (EnemyBehavior w in inactiveEnemies)
        {
            if (w.enemyType == nextWaveObject.enemyType)
            {
                enemyObject = w.gameObject;
                break;
            }
        }
        if (enemyObject == null)
        {
            enemyObject = Instantiate(nextWaveObject.body, location, Quaternion.identity);
        }

        enemyObject.SetActive(true);
        enemyObject.transform.position = location;
        enemyScript = enemyObject.GetComponent<EnemyBehavior>();
        GameModel.GameColor enemyColor = nextWaveObject.color;
        bool darkEnemy = nextWaveObject.darkened;
        enemyScript.initialize(player.transform.position, enemyColor, darkEnemy, enemyScript.enemyType);
        enemyScript.SetVisualColor(enemyColor);
        if (gameManager.activeEnemies.Contains(enemyScript))
        {
            gameManager.activeEnemies.Remove(enemyScript);
        }
        gameManager.activeEnemies.Add(enemyScript);
        inactiveEnemies.Remove(enemyScript);

        return enemyObject;
    }

    private int demoCount = 0;

    public void EnemyDemoUpdate()
    {
        enemyTimer -= Time.deltaTime;
        if (enemyTimer <= 0.0f)
        {
            WaveObject enemy = returnRandomChunk(demoChunks).ChunkToWaveObject(false);
            int EdgeToSpawnFrom = enemy.locationsToSpawn[Random.Range(0, enemy.locationsToSpawn.Length)];
            Vector3 startPos;
            switch (EdgeToSpawnFrom)
            {
                case 0:
                    startPos = new Vector3(Random.Range(xBounds * 1.2f, -xBounds * 1.2f), yBounds, 0);
                    break;
                case 1:
                    startPos = new Vector3(Random.Range(xBounds * 1.2f, -xBounds * 1.2f), -yBounds, 0);
                    break;
                case 2:
                    startPos = new Vector3(-xBounds * 1.2f, Random.Range(yBounds / 2, -yBounds / 2), 0);
                    break;
                case 3:
                    startPos = new Vector3(xBounds * 1.2f, Random.Range(yBounds / 2, -yBounds / 2), 0);
                    break;
                default:
                    startPos = new Vector3(Random.Range(xBounds * 1.2f, -xBounds * 1.2f), yBounds, 0);
                    break;
            }

            GameObject enemyObject = null;
            EnemyBehavior enemyScript;
            foreach (EnemyBehavior w in inactiveEnemies)
            {
                if (w.enemyType == enemy.enemyType)
                {
                    enemyObject = w.gameObject;
                    break;
                }
            }
            if (enemyObject == null)
            {
                enemyObject = Instantiate(enemy.body, startPos, Quaternion.identity);
            }

            enemyObject.SetActive(true);
            enemyObject.transform.position = startPos;
            enemyScript = enemyObject.GetComponent<EnemyBehavior>();
            GameModel.GameColor enemyColor = enemy.color;
            bool darkEnemy = enemy.darkened;
            enemyScript.initialize(player.transform.position, enemyColor, darkEnemy, enemyScript.enemyType);
            enemyScript.SetVisualColor(enemyColor);
            if (gameManager.activeEnemies.Contains(enemyScript))
            {
                gameManager.activeEnemies.Remove(enemyScript);
            }
            gameManager.activeEnemies.Add(enemyScript);
            inactiveEnemies.Remove(enemyScript);
            enemyTimer = enemy.delayUntilNext;
            demoCount++;
            if (demoCount % 20 == 0)
            {
                int rand = Random.Range(0, 4);
                switch (rand)
                {
                    case 0:
                        addAllChunkColors(new FastChunk(new[] { GameModel.GameColor.NONE }, false), true);
                        break;
                    case 1:
                        addAllChunkColors(new NinjaChunk(new[] { GameModel.GameColor.NONE }, false), true);
                        break;
                    case 2:
                        addAllChunkColors(new ZigZagChunk(new[] { GameModel.GameColor.NONE }, false), true);
                        break;
                    case 3:
                        addAllChunkColors(new SwarmChunk(new[] { GameModel.GameColor.NONE }, false), true);
                        break;
                    case 4:
                        addAllChunkColors(new SwooperChunk(new[] { GameModel.GameColor.NONE }, false), true);
                        break;
                }
            }
        }
    }

    void Awake()
    {
        instance = this;
        modelGame = GameObject.Find("GameManager").GetComponent<GameModel>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        tutorialSpacing = modelGame.baseTutorialSpacing;
    }

    public void initialize()
    { 
        repopulateChunks();
        populateDemoChunks();
        
        gameManager.addStartingUpgrades();

        clearWave();

        if (Level == 0 && WaveSpawningSystem.instance.tutorialStage == 0)
        {
            generateFirstWave();
            UIManager.instance.WipeUpgrades();
            UIManager.instance.SetUpgradesVisible(false);
            gameManager.SetState(GameState.WAVE);
        }
        else
        {
            Level = 1;
            generateWave();
            RandomizeWave();

        }
        enemyTimer = currentWave[0].delayUntilNext;
        currentWaveIndex = 0;

        enemyTimer = currentWave[currentWaveIndex].delayUntilNext;
        //SaveLoadManager.instance.SaveGame();

        //Level = 1;
    }

    public void AddProModeFeatures()
    {
        if (gameManager.promodeLevel != 0)
        {
            for (int p = 0; p < gameManager.promodeLevel; p++)
            {
                AddWaveMod();
            }
        }
        if (gameManager.promodeLevel >= 3)
        {
            for (int u = 0; u < (int)gameManager.promodeLevel / 3; u++)
            {
                gameManager.GivePlayerRandomUpgrade();
            }
        }
    }

    public void AddExtraEnemies()
    {
        List<WaveObject> newEnemies = new List<WaveObject>();
        for (int k =0; k < currentWave.Count; k++)
        {
            if (k%2 == 0)
            {
                newEnemies.Add(currentWave[k].MakeCopy());
            }
        }
        foreach (WaveObject w in newEnemies)
        {
            currentWave.Add(w);
        }
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

    void addAllChunkColors(Chunk toAdd, bool toDemo)
    {
        GameModel.GameColor[] standardColors = {GameModel.GameColor.YELLOW, GameModel.GameColor.BLUE, GameModel.GameColor.RED};
        GameModel.GameColor[] additionalColors = additionalColorsReadout().ToArray();
        if (!toDemo)
        {
            addChunks(standardColors, toAdd);
            addChunks(additionalColors, toAdd);
        }
        else
        {
            addChunksToDemo(standardColors, toAdd);
            addChunksToDemo(additionalColors, toAdd);
        }

        if (newMechanics.Contains(Mechanic.WHITE) || currentMechanics.Contains(Mechanic.WHITE))
        {
            if (!toDemo)
            {
                addChunks(new[] { GameModel.GameColor.WHITE }, toAdd);
            }
            else
            {
                addChunksToDemo(new[] { GameModel.GameColor.WHITE }, toAdd);
            }
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
        return !(c is BasicChunk);

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

    void addChunksToDemo(GameModel.GameColor[] colors, Chunk toAdd)
    {
        Chunk newChunk;
        foreach (GameModel.GameColor[] singleColor in oneColorPermutations(colors))
        {
            newChunk = toAdd.MakeCopy(singleColor, false);
            demoChunks.Add(newChunk);
        }
        if (colors.Length > 1)
        {
            foreach (GameModel.GameColor[] colorPair in twoColorPermutations(colors))
            {
                newChunk = toAdd.MakeCopy(colorPair, false);
                demoChunks.Add(newChunk);

            }
        }
        if (colors.Length > 2)
        {
            newChunk = toAdd.MakeCopy(new[] { colors[0], colors[1], colors[2] }, false);
            demoChunks.Add(newChunk);
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

        return chunklist[0];
    }

    Chunk nextChunk;
    public void generateWave()
    {
        if (currentChunks == null || currentChunks.Count == 0) {
            generateNormally();
        }
        else
        {
            generateFromSavedChunks();
        }
    }

    void generateNormally()
    {
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
            if (i < 3)
            {
                UIManager.instance.SetupChunkPreview(nextChunk, 1);
            }
            else
            {
                UIManager.instance.SetupChunkPreview(nextChunk, 2);
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
            currentChunks.Add(nextChunk);
        }
        enemiesToSpawn = currentWave.Count - 1;
    }

    void generateFromSavedChunks()
    {
        UIManager.instance.WipePreviewImages();
        for (int i = 0; i < currentChunks.Count; i++)
        {
            nextChunk = currentChunks[i];
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
        currentWave[0].delayUntilNext += 3f;
    }


    public void generateFirstWave()
    {
        currentWave.Add(new WaveObject(Enemies[13], EnemyScripts[13], GameModel.GameColor.RED,  
            tutorialSpacing,
            TOPBOTTOM, false, WaveObject.Type.TARGET));

        currentWave.Add(new WaveObject(Enemies[13], EnemyScripts[13], GameModel.GameColor.BLUE,  
            tutorialSpacing,
            TOPBOTTOM, false, WaveObject.Type.TARGET));

        currentWave.Add(new WaveObject(Enemies[13], EnemyScripts[13], GameModel.GameColor.YELLOW,  
            tutorialSpacing,
            TOPBOTTOM, false, WaveObject.Type.TARGET));

        currentWave.Add(new WaveObject(Enemies[13], EnemyScripts[13], GameModel.GameColor.PURPLE,
        tutorialSpacing,
        TOPBOTTOM, false, WaveObject.Type.TARGET));

        currentWave.Add(new WaveObject(Enemies[1], EnemyScripts[1], GameModel.GameColor.YELLOW,
        0f,
        TOPBOTTOM, false, WaveObject.Type.FAST));

        currentWave.Add(new WaveObject(Enemies[1], EnemyScripts[1], GameModel.GameColor.RED,
        0.2f,
        TOPBOTTOM, false, WaveObject.Type.FAST));

        currentWave.Add(new WaveObject(Enemies[13], EnemyScripts[13], GameModel.GameColor.BLUE,
        0f,
        TOPBOTTOM, false, WaveObject.Type.TARGET));

        currentWave.Add(new WaveObject(Enemies[13], EnemyScripts[13], GameModel.GameColor.YELLOW,
        0f,
        TOPBOTTOM, false, WaveObject.Type.TARGET));


        gameManager.encounteredEnemies.Add(Mechanic.TARGET);
    }



    public void SetupNextWave()
    {
        if (Level == 0 && WaveSpawningSystem.instance.tutorialStage == 0)
        {
            generateFirstWave();
            enemyTimer = currentWave[0].delayUntilNext;
            currentWaveIndex = 0;
        }
        else
        {
            clearWave();
            generateWave();
            RandomizeWave();
            enemyTimer = currentWave[0].delayUntilNext;
            currentWaveIndex = 0;
        }

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
                currentWave[j] = currentWave[tutorialIndex];
                currentWave[tutorialIndex] = tmp;
                tutorialIndex++;
            }
        }
    }

    public void IncreaseDifficulty()
    {
        Level++;
        if (Level > 15){
            gameManager.SetState(GameManager.GameState.WIN);
            return;
        }
        //Every wave spawns 3% faster.
        globalWaveSpacing /= modelGame.baseWaveSpacingUpgrade;


        //Adds extra chunk on Wave 10 and 14
        if (Level == 9 || Level == 13)
        {
            double sum = 0;
            for (int k = 0; k < chunkDifficulties.Count; k++)
            {
                sum += chunkDifficulties[k];
            }
            numChunks++;
            chunkDifficulties.Add((int)Math.Ceiling((sum/chunkDifficulties.Count)));
            globalWaveNumber += modelGame.baseWaveNumberUpgrade;
        }
        //starting on wave 3, every 7th wave adds a guaranteed unique chunk
        if ((Level+4) % 7 == 0)
        {
            numUniqueChunks++;
        }

        //every 2nd wave, introduces a new mechanic.
        if (Level % 2 == 0)
        {
            if (Level > 7)
            {
                addRandomMechanic(2);
            }
            else
            {
                addRandomMechanic(1);
            }
        }

        //On wave 3, add secondary colors. On wave 10, adds white.
        if (Level == 3)
        {
            newMechanics.Add(Mechanic.PURPLE);
            newMechanics.Add(Mechanic.GREEN);
            newMechanics.Add(Mechanic.ORANGE);
        }
        if (Level == 10)
        {
            newMechanics.Add(Mechanic.WHITE);
        }

        if (Level % 2 == 0)
        {
            AddWaveMod();
        }

        /* Random chunk + 50%
        int rand = Random.Range(0, chunkDifficulties.Count);
        chunkDifficulties[rand] = (int)Math.Ceiling(chunkDifficulties[rand] * 1.5f);*/

        //Add 1 to all, 1 to random.
        int rand = Random.Range(0, chunkDifficulties.Count);
        for (int k = 0; k < chunkDifficulties.Count; k++)
        {
            if (k == rand)
            {
                chunkDifficulties[k] += modelGame.baseStandardDifficultyIncrease;
            }
            chunkDifficulties[k] += modelGame.baseStandardDifficultyIncrease;
        }

        repopulateChunks();
    }


    void AddWaveMod()
    {
        //Every other wave makes waves harder with a modifier.
        int randomWaveMod = Random.Range(0, 4);
        switch (randomWaveMod)
        {
            case 0:
                globalWaveNumber += modelGame.baseNumerousNumberUpgrade;
                globalWaveSpacing /= modelGame.baseNumerousSpacingUpgrade;
                UIManager.instance.AddWaveMod(UIManager.WaveModifier.NUMEROUS);
                break;
            case 1:
                globalWaveSpeed *= modelGame.baseWaveSpeedUpgrade;
                UIManager.instance.AddWaveMod(UIManager.WaveModifier.FASTER);
                break;
            case 2:
                for (int k = 0; k < chunkDifficulties.Count; k++)
                {
                    //multiply all difficulties by 1.5
                    chunkDifficulties[k] = (int)Math.Ceiling(chunkDifficulties[k] * modelGame.baseWaveDifficultyUpgrade);
                }
                UIManager.instance.AddWaveMod(UIManager.WaveModifier.DIFFICULT);
                break;
            case 3:
                globalRainbowMult *= (1 - modelGame.baseWaveMonochrome);
                UIManager.instance.AddWaveMod(UIManager.WaveModifier.MONOCHROME);
                break;

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

        //enemy types
        addAllChunkColors(new BasicChunk(new[] { GameModel.GameColor.NONE }, false), false);
        if (newMechanics.Contains(Mechanic.FAST) || currentMechanics.Contains(Mechanic.FAST))
        {
            addAllChunkColors(new FastChunk(new[] { GameModel.GameColor.NONE }, false), false);
        }
        if (newMechanics.Contains(Mechanic.NINJA) || currentMechanics.Contains(Mechanic.NINJA))
        {
            addAllChunkColors(new NinjaChunk(new[] { GameModel.GameColor.NONE }, false), false);
        }
        if (newMechanics.Contains(Mechanic.SWARM) || currentMechanics.Contains(Mechanic.SWARM))
        {
            addAllChunkColors(new SwarmChunk(new[] { GameModel.GameColor.NONE }, false), false);
        }
        if (newMechanics.Contains(Mechanic.ZIGZAG) || currentMechanics.Contains(Mechanic.ZIGZAG))
        {
            addAllChunkColors(new ZigZagChunk(new[] { GameModel.GameColor.NONE }, false), false);
        }
        if (newMechanics.Contains(Mechanic.DISGUISED) || currentMechanics.Contains(Mechanic.DISGUISED))
        {
            addAllChunkColors(new DisguiserChunk(new[] { GameModel.GameColor.NONE }, false), false);
        }
        if (newMechanics.Contains(Mechanic.SWIRL) || currentMechanics.Contains(Mechanic.SWIRL))
        {
            addAllChunkColors(new SwirlChunk(new[] { GameModel.GameColor.NONE }, false),false);
        }
        if (newMechanics.Contains(Mechanic.RAGE) || currentMechanics.Contains(Mechanic.RAGE))
        {
            addAllChunkColors(new RageChunk(new[] { GameModel.GameColor.NONE }, false), false);
        }
        if (newMechanics.Contains(Mechanic.PAINTER) || currentMechanics.Contains(Mechanic.PAINTER))
        {
            addAllChunkColors(new PainterChunk(new[] { GameModel.GameColor.NONE }, false), false);
        }
        if (newMechanics.Contains(Mechanic.EXPLOSIVE) || currentMechanics.Contains(Mechanic.EXPLOSIVE))
        {
            addAllChunkColors(new ExplosiveChunk(new[] { GameModel.GameColor.NONE }, false), false);
        }
        if (newMechanics.Contains(Mechanic.SPLITTER) || currentMechanics.Contains(Mechanic.SPLITTER))
        {
            addAllChunkColors(new SplitterChunk(new[] { GameModel.GameColor.NONE }, false), false);
        }
        if (newMechanics.Contains(Mechanic.SWOOPER) || currentMechanics.Contains(Mechanic.SWOOPER))
        {
            addAllChunkColors(new SwooperChunk(new[] { GameModel.GameColor.NONE }, false), false);
        }


        //modifiers (Dark only for now)
        if (newMechanics.Contains(Mechanic.DARK) || currentMechanics.Contains(Mechanic.DARK))
        {
            addAllDarkChunks();
        }
    }
    
    public void populateDemoChunks()
    {
        addAllChunkColors(new BasicChunk(new[] { GameModel.GameColor.NONE }, false), true);
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
    
    public Chunk returnSpecialChunkFromGeneric(Chunk c)
    {
        switch (c.name)
        {
            case "Basic":
                return new BasicChunk(c.colors, c.isDarkened);
            case "Fast":
                return new FastChunk(c.colors, c.isDarkened);
            case "Ninja":
                return new NinjaChunk(c.colors, c.isDarkened);
            case "Swarm":
                return new SwarmChunk(c.colors, c.isDarkened);
            case "Zigzag":
                return new ZigZagChunk(c.colors, c.isDarkened);
            case "Disguiser":
                return new DisguiserChunk(c.colors, c.isDarkened);
            case "Swirl":
                return new SwirlChunk(c.colors, c.isDarkened);
            case "Rage":
                return new RageChunk(c.colors, c.isDarkened);
            case "Painter":
                return new PainterChunk(c.colors, c.isDarkened);
            case "Explosive":
                return new ExplosiveChunk(c.colors, c.isDarkened);
            case "Splitter":
                return new SplitterChunk(c.colors, c.isDarkened);
            case "Swooper":
                return new SwooperChunk(c.colors, c.isDarkened);

            default:
                return new BasicChunk(c.colors, c.isDarkened);
        }
    }


    public class Chunk
    {
        public GameModel.GameColor[] colors = {GameModel.GameColor.RED};
        public int imageID;
        public bool isMultiColor;
        public int difficulty;
        public int baseDifficulty;
        public bool isDarkened;
        public string name;
        public bool isTutorialChunk;
        public float speedMultiplier;

        [JsonConstructor]
        public Chunk(GameModel.GameColor[] colors, bool isDarkened, int imageID, bool isMultiColor, int difficulty, int baseDifficulty, string name, bool isTutorialChunk, float speedMultiplier)
        {
            this.colors = colors;
            WaveSpawningSystem.instance = GameObject.Find("GameManager").GetComponent<WaveSpawningSystem>();
            isMultiColor = (colors.Contains(GameModel.GameColor.ORANGE) ||
                            colors.Contains(GameModel.GameColor.GREEN) ||
                            colors.Contains(GameModel.GameColor.PURPLE) ||
                            colors.Contains(GameModel.GameColor.WHITE));
            this.isDarkened = isDarkened;
            if (colors.Contains(GameModel.GameColor.WHITE))
            {
                isDarkened = false;
            }
            this.imageID = imageID;
            this.isMultiColor = isMultiColor;
            this.difficulty = difficulty;
            this.baseDifficulty = baseDifficulty;
            this.name = name;
            this.isTutorialChunk = isTutorialChunk;
            this.speedMultiplier = speedMultiplier;
        }
        public Chunk(GameModel.GameColor[] spawnColors)
        {
            colors = spawnColors;
            WaveSpawningSystem.instance = GameObject.Find("GameManager").GetComponent<WaveSpawningSystem>();
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

        public virtual Chunk MakeCopy()
        {
            return new BasicChunk(this.colors, this.isDarkened);
        }
        public virtual Chunk MakeCopy(GameModel.GameColor[] colors, bool isDark)
        {
            return new BasicChunk(colors, isDark);
        }

        public virtual void SetDifficulty(GameModel.GameColor[] spawnColors)
        {
            difficulty = baseDifficulty + spawnColors.Length;
            if (isMultiColor)
            {
                difficulty *= 2;
            }

            if (isDarkened)
            {
                difficulty *= 2;
            }
        }
        public virtual WaveObject ChunkToWaveObject(bool isTutorial)
        {
            float spacing = isTutorial ? tutorialSpacing : globalWaveSpacing;
            WaveObject toReturn;
            if (colors.Length == 1)
            {
                toReturn = new WaveObject(WaveSpawningSystem.instance.Enemies[0], WaveSpawningSystem.instance.EnemyScripts[0], colors[0], spacing, TOPBOTTOM, isDarkened, WaveObject.Type.BASIC);
            }
            else
            {
                int rand = Random.Range(0, colors.Length);
                toReturn = new WaveObject(WaveSpawningSystem.instance.Enemies[0], WaveSpawningSystem.instance.EnemyScripts[0], colors[rand], spacing, TOPBOTTOM, isDarkened, WaveObject.Type.BASIC);
            }
            toReturn.isTutorial = isTutorial;
            return toReturn;
        }
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
                toReturn = new WaveObject(WaveSpawningSystem.instance.Enemies[0], WaveSpawningSystem.instance.EnemyScripts[0], colors[0], spacing, TOPBOTTOM, isDarkened, WaveObject.Type.BASIC);
            }
            else
            {
                int rand = Random.Range(0, colors.Length);
                toReturn = new WaveObject(WaveSpawningSystem.instance.Enemies[0], WaveSpawningSystem.instance.EnemyScripts[0], colors[rand], spacing, TOPBOTTOM, isDarkened, WaveObject.Type.BASIC);
            }
            toReturn.isTutorial = isTutorial;
            return toReturn;
        }

        public BasicChunk(GameModel.GameColor[] spawnColors, bool dark) : base(spawnColors, dark, 0, false, 0, 0, "Basic", false, 1.0f)
        {
            name = "Basic";
            baseDifficulty = 0;
            imageID = 0;
            SetDifficulty(spawnColors);
        }
        public BasicChunk(GameModel.GameColor[] spawnColors) : base(spawnColors)
        {
            name = "Basic";
            baseDifficulty = 0;
            imageID = 0;
            SetDifficulty(spawnColors);
        }
    }
    
    public class FastChunk : Chunk
    {
        public FastChunk(GameModel.GameColor[] spawnColors, bool dark) : base(spawnColors, dark, 0, false, 0, 0, "Basic", false, 1.0f)
        {
            name = "Fast";
            baseDifficulty = 3;
            imageID = 1;
            SetDifficulty(spawnColors);
        }
        public FastChunk(GameModel.GameColor[] spawnColors) : base(spawnColors)
        {
            name = "Fast";
            baseDifficulty = 3;
            imageID = 1;
            SetDifficulty(spawnColors);
        }
        public override WaveObject ChunkToWaveObject(bool isTutorial)
        {
            float spacing = isTutorial ? tutorialSpacing : globalWaveSpacing;
            WaveObject toReturn;
            if (colors.Length == 1)
            {
                toReturn = new WaveObject(WaveSpawningSystem.instance.Enemies[1], WaveSpawningSystem.instance.EnemyScripts[1], colors[0], spacing, TOPBOTTOM, isDarkened, WaveObject.Type.FAST);
            }
            else
            {
                int rand = Random.Range(0, colors.Length);
                toReturn = new WaveObject(WaveSpawningSystem.instance.Enemies[1], WaveSpawningSystem.instance.EnemyScripts[1], colors[rand], spacing, TOPBOTTOM, isDarkened, WaveObject.Type.FAST);
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
        public NinjaChunk(GameModel.GameColor[] spawnColors, bool dark) : base(spawnColors, dark, 0, false, 0, 0, "Basic", false, 1.0f)
        {
            name = "Ninja";
            baseDifficulty = 2;
            imageID = 2;
            SetDifficulty(spawnColors);
        }
        public NinjaChunk(GameModel.GameColor[] spawnColors) : base(spawnColors)
        {
            name = "Ninja";
            baseDifficulty = 2;
            imageID = 2;
            SetDifficulty(spawnColors);
        }

        public override WaveObject ChunkToWaveObject(bool isTutorial)
        {
            float spacing = isTutorial ? tutorialSpacing : globalWaveSpacing;
            WaveObject toReturn;
            if (colors.Length == 1)
            {
                toReturn = new WaveObject(WaveSpawningSystem.instance.Enemies[2], WaveSpawningSystem.instance.EnemyScripts[2], colors[0], spacing, LEFTRIGHT, isDarkened, WaveObject.Type.NINJA);
            }
            else
            {
                int rand = Random.Range(0, colors.Length);
                toReturn = new WaveObject(WaveSpawningSystem.instance.Enemies[2], WaveSpawningSystem.instance.EnemyScripts[2], colors[rand], spacing, LEFTRIGHT, isDarkened, WaveObject.Type.NINJA);
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
        public SwarmChunk(GameModel.GameColor[] spawnColors, bool dark) : base(spawnColors, dark, 0, false, 0, 0, "Basic", false, 1.0f)
        {
            name = "Swarm";
            baseDifficulty = 3;
            imageID = 3;
            SetDifficulty(spawnColors);
        }
        public SwarmChunk(GameModel.GameColor[] spawnColors) : base(spawnColors)
        {
            name = "Swarm";
            baseDifficulty = 3;
            imageID = 3;
            SetDifficulty(spawnColors);
        }

        public override WaveObject ChunkToWaveObject(bool isTutorial)
        {
            float spacing = isTutorial ? tutorialSpacing : globalWaveSpacing;
            WaveObject toReturn;
            if (colors.Length == 1)
            {
                toReturn = new WaveObject(WaveSpawningSystem.instance.Enemies[3], WaveSpawningSystem.instance.EnemyScripts[3], colors[0], spacing * SWARMSPACINGMULT, ALL, isDarkened, WaveObject.Type.SWARM);
            }
            else
            {
                int rand = Random.Range(0, colors.Length);
                toReturn = new WaveObject(WaveSpawningSystem.instance.Enemies[3], WaveSpawningSystem.instance.EnemyScripts[3], colors[rand], spacing * SWARMSPACINGMULT, ALL, isDarkened, WaveObject.Type.SWARM);
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
        public ZigZagChunk(GameModel.GameColor[] spawnColors, bool dark) : base(spawnColors, dark, 0, false, 0, 0, "Basic", false, 1.0f)
        {
            name = "Zigzag";
            baseDifficulty = 5;
            imageID = 4;
            SetDifficulty(spawnColors);
        }
        public ZigZagChunk(GameModel.GameColor[] spawnColors) : base(spawnColors)
        {
            name = "Zigzag";
            baseDifficulty = 5;
            imageID = 4;
            SetDifficulty(spawnColors);
        }

        public override WaveObject ChunkToWaveObject(bool isTutorial)
        {
            float spacing = isTutorial ? tutorialSpacing : globalWaveSpacing;
            WaveObject toReturn;
            if (colors.Length == 1)
            {
                toReturn = new WaveObject(WaveSpawningSystem.instance.Enemies[4], WaveSpawningSystem.instance.EnemyScripts[4], colors[0], spacing, TOPBOTTOM, isDarkened, WaveObject.Type.ZIGZAG);
            }
            else
            {
                int rand = Random.Range(0, colors.Length);
                toReturn = new WaveObject(WaveSpawningSystem.instance.Enemies[4], WaveSpawningSystem.instance.EnemyScripts[4], colors[rand], spacing, TOPBOTTOM, isDarkened, WaveObject.Type.ZIGZAG);
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
        public DisguiserChunk(GameModel.GameColor[] spawnColors, bool dark) : base(spawnColors, dark, 0, false, 0, 0, "Basic", false, 1.0f)
        {
            name = "Disguiser";
            baseDifficulty = 5;
            imageID = 5;
            SetDifficulty(spawnColors);
        }
        public DisguiserChunk(GameModel.GameColor[] spawnColors) : base(spawnColors)
        {
            name = "Disguiser";
            baseDifficulty = 5;
            imageID = 5;
            SetDifficulty(spawnColors);
        }

        public override WaveObject ChunkToWaveObject(bool isTutorial)
        {
            float spacing = isTutorial ? tutorialSpacing : globalWaveSpacing;
            WaveObject toReturn;
            if (colors.Length == 1)
            {
                toReturn = new WaveObject(WaveSpawningSystem.instance.Enemies[5], WaveSpawningSystem.instance.EnemyScripts[5], colors[0], spacing, TOPBOTTOM, isDarkened, WaveObject.Type.DISGUISED);
            }
            else
            {
                int rand = Random.Range(0, colors.Length);
                toReturn = new WaveObject(WaveSpawningSystem.instance.Enemies[5], WaveSpawningSystem.instance.EnemyScripts[5], colors[rand], spacing, TOPBOTTOM, isDarkened, WaveObject.Type.DISGUISED);
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
        public SwirlChunk(GameModel.GameColor[] spawnColors, bool dark) : base(spawnColors, dark, 0, false, 0, 0, "Basic", false, 1.0f)
        {
            name = "Swirl";
            baseDifficulty = 4;
            imageID = 6;
            SetDifficulty(spawnColors);
        }
        public SwirlChunk(GameModel.GameColor[] spawnColors) : base(spawnColors)
        {
            name = "Swirl";
            baseDifficulty = 4;
            imageID = 6;
            SetDifficulty(spawnColors);
        }

        public override WaveObject ChunkToWaveObject(bool isTutorial)
        {
            float spacing = isTutorial ? tutorialSpacing : globalWaveSpacing;
            WaveObject toReturn;
            if (colors.Length == 1)
            {
                toReturn = new WaveObject(WaveSpawningSystem.instance.Enemies[6], WaveSpawningSystem.instance.EnemyScripts[6], colors[0], spacing, TOPBOTTOM, isDarkened, WaveObject.Type.SWIRL);
            }
            else
            {
                int rand = Random.Range(0, colors.Length);
                toReturn = new WaveObject(WaveSpawningSystem.instance.Enemies[6], WaveSpawningSystem.instance.EnemyScripts[6], colors[rand], spacing, TOPBOTTOM, isDarkened, WaveObject.Type.SWIRL);
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

    public class RageChunk : Chunk
    {
        public RageChunk(GameModel.GameColor[] spawnColors, bool dark) : base(spawnColors, dark, 0, false, 0, 0, "Basic", false, 1.0f)
        {
            name = "Rage";
            baseDifficulty = 4;
            imageID = 7;
            SetDifficulty(spawnColors);
        }
        public RageChunk(GameModel.GameColor[] spawnColors) : base(spawnColors)
        {
            name = "Rage";
            baseDifficulty = 4;
            imageID = 7;
            SetDifficulty(spawnColors);
        }

        public override WaveObject ChunkToWaveObject(bool isTutorial)
        {
            float spacing = isTutorial ? tutorialSpacing : globalWaveSpacing;
            WaveObject toReturn;
            if (colors.Length == 1)
            {
                toReturn = new WaveObject(WaveSpawningSystem.instance.Enemies[7], WaveSpawningSystem.instance.EnemyScripts[7], colors[0], spacing, TOPBOTTOM, isDarkened, WaveObject.Type.RAGE);
            }
            else
            {
                int rand = Random.Range(0, colors.Length);
                toReturn = new WaveObject(WaveSpawningSystem.instance.Enemies[7], WaveSpawningSystem.instance.EnemyScripts[7], colors[rand], spacing, TOPBOTTOM, isDarkened, WaveObject.Type.RAGE);
            }
            toReturn.isTutorial = isTutorial;
            return toReturn;
        }

        public override Chunk MakeCopy()
        {
            return new RageChunk(colors, isDarkened);
        }
        public override Chunk MakeCopy(GameModel.GameColor[] colors, bool isDark)
        {
            return new RageChunk(colors, isDark);
        }
    }

    public class PainterChunk : Chunk
    {
        public PainterChunk(GameModel.GameColor[] spawnColors, bool dark) : base(spawnColors, dark, 0, false, 0, 0, "Basic", false, 1.0f)
        {
            name = "Painter";
            baseDifficulty = 4;
            imageID = 8;
            SetDifficulty(spawnColors);
        }
        public PainterChunk(GameModel.GameColor[] spawnColors) : base(spawnColors)
        {
            name = "Painter";
            baseDifficulty = 4;
            imageID = 8;
            SetDifficulty(spawnColors);
        }

        public override WaveObject ChunkToWaveObject(bool isTutorial)
        {
            float spacing = isTutorial ? tutorialSpacing : globalWaveSpacing;
            WaveObject toReturn;
            if (colors.Length == 1)
            {
                toReturn = new WaveObject(WaveSpawningSystem.instance.Enemies[8], WaveSpawningSystem.instance.EnemyScripts[8], colors[0], spacing, TOPBOTTOM, isDarkened, WaveObject.Type.PAINTER);
            }
            else
            {
                int rand = Random.Range(0, colors.Length);
                toReturn = new WaveObject(WaveSpawningSystem.instance.Enemies[8], WaveSpawningSystem.instance.EnemyScripts[8], colors[rand], spacing, TOPBOTTOM, isDarkened, WaveObject.Type.PAINTER);
            }
            toReturn.isTutorial = isTutorial;
            return toReturn;
        }
        public override void Generate(bool tutorial)
        {
            base.Generate(tutorial, (int)globalWaveNumber / 3);
        }

        public override Chunk MakeCopy()
        {
            return new PainterChunk(colors, isDarkened);
        }
        public override Chunk MakeCopy(GameModel.GameColor[] colors, bool isDark)
        {
            return new PainterChunk(colors, isDark);
        }
    }

    public class ExplosiveChunk : Chunk
    {
        public ExplosiveChunk(GameModel.GameColor[] spawnColors, bool dark) : base(spawnColors, dark, 0, false, 0, 0, "Basic", false, 1.0f)
        {
            name = "Explosive";
            baseDifficulty = 2;
            imageID = 9;
            SetDifficulty(spawnColors);
        }
        public ExplosiveChunk(GameModel.GameColor[] spawnColors) : base(spawnColors)
        {
            name = "Explosive";
            baseDifficulty = 2;
            imageID = 9;
            SetDifficulty(spawnColors);
        }

        public override WaveObject ChunkToWaveObject(bool isTutorial)
        {
            float spacing = isTutorial ? tutorialSpacing : globalWaveSpacing;
            WaveObject toReturn;
            if (colors.Length == 1)
            {
                toReturn = new WaveObject(WaveSpawningSystem.instance.Enemies[9], WaveSpawningSystem.instance.EnemyScripts[9], colors[0], spacing, TOPBOTTOM, isDarkened, WaveObject.Type.EXPLOSIVE);
            }
            else
            {
                int rand = Random.Range(0, colors.Length);
                toReturn = new WaveObject(WaveSpawningSystem.instance.Enemies[9], WaveSpawningSystem.instance.EnemyScripts[9], colors[rand], spacing, TOPBOTTOM, isDarkened, WaveObject.Type.EXPLOSIVE);
            }
            toReturn.isTutorial = isTutorial;
            return toReturn;
        }
        public override void Generate(bool tutorial)
        {
            base.Generate(tutorial, globalWaveNumber);
        }

        public override Chunk MakeCopy()
        {
            return new ExplosiveChunk(colors, isDarkened);
        }
        public override Chunk MakeCopy(GameModel.GameColor[] colors, bool isDark)
        {
            return new ExplosiveChunk(colors, isDark);
        }
    }

    public class SplitterChunk : Chunk
    {
        public SplitterChunk(GameModel.GameColor[] spawnColors, bool dark) : base(spawnColors, dark, 0, false, 0, 0, "Basic", false, 1.0f)
        {
            name = "Splitter";
            baseDifficulty = 5;
            imageID = 10;
            SetDifficulty(spawnColors);
        }
        public SplitterChunk(GameModel.GameColor[] spawnColors) : base(spawnColors)
        {
            name = "Explosive";
            baseDifficulty = 5;
            imageID = 10;
            SetDifficulty(spawnColors);
        }

        public override WaveObject ChunkToWaveObject(bool isTutorial)
        {
            float spacing = isTutorial ? tutorialSpacing : globalWaveSpacing;
            WaveObject toReturn;
            if (colors.Length == 1)
            {
                toReturn = new WaveObject(WaveSpawningSystem.instance.Enemies[10], WaveSpawningSystem.instance.EnemyScripts[10], colors[0], spacing, TOPBOTTOM, isDarkened, WaveObject.Type.SPLITTER);
            }
            else
            {
                int rand = Random.Range(0, colors.Length);
                toReturn = new WaveObject(WaveSpawningSystem.instance.Enemies[10], WaveSpawningSystem.instance.EnemyScripts[10], colors[rand], spacing, TOPBOTTOM, isDarkened, WaveObject.Type.SPLITTER);
            }
            toReturn.isTutorial = isTutorial;
            return toReturn;
        }
        public override void Generate(bool tutorial)
        {
            base.Generate(tutorial, (int)globalWaveNumber / 2);
        }

        public override Chunk MakeCopy()
        {
            return new SplitterChunk(colors, isDarkened);
        }
        public override Chunk MakeCopy(GameModel.GameColor[] colors, bool isDark)
        {
            return new SplitterChunk(colors, isDark);
        }

        public override void SetDifficulty(GameModel.GameColor[] spawnColors)
        {
            difficulty = baseDifficulty + spawnColors.Length;
            if (isMultiColor)
            {
                difficulty *= 4;
            }

            if (isDarkened)
            {
                difficulty *= 2;
            }
        }
    }
    public class SplitterBlob : Chunk
    {
        public SplitterBlob(GameModel.GameColor[] spawnColors, bool dark) : base(spawnColors, dark, 0, false, 0, 0, "Basic", false, 1.0f)
        {
            name = "SplitterBlob";
            baseDifficulty = 1;
            imageID = 11;
            SetDifficulty(spawnColors);
        }
        public SplitterBlob(GameModel.GameColor[] spawnColors) : base(spawnColors)
        {
            name = "SplitterBlob";
            baseDifficulty = 1;
            imageID = 11;
            SetDifficulty(spawnColors);
        }

        public override WaveObject ChunkToWaveObject(bool isTutorial)
        {
            float spacing = isTutorial ? tutorialSpacing : globalWaveSpacing;
            WaveObject toReturn;
            if (colors.Length == 1)
            {
                toReturn = new WaveObject(WaveSpawningSystem.instance.Enemies[11], WaveSpawningSystem.instance.EnemyScripts[11], colors[0], spacing, TOPBOTTOM, isDarkened, WaveObject.Type.SPLITTERBLOB);
            }
            else
            {
                int rand = Random.Range(0, colors.Length);
                toReturn = new WaveObject(WaveSpawningSystem.instance.Enemies[11], WaveSpawningSystem.instance.EnemyScripts[11], colors[rand], spacing, TOPBOTTOM, isDarkened, WaveObject.Type.SPLITTERBLOB);
            }
            toReturn.isTutorial = isTutorial;
            return toReturn;
        }
        public override void Generate(bool tutorial)
        {
            base.Generate(tutorial, (int)globalWaveNumber);
        }

        public override Chunk MakeCopy()
        {
            return new SplitterBlob(colors, isDarkened);
        }
        public override Chunk MakeCopy(GameModel.GameColor[] colors, bool isDark)
        {
            return new SplitterBlob(colors, isDark);
        }
    }
    public class SwooperChunk : Chunk
    {
        public SwooperChunk(GameModel.GameColor[] spawnColors, bool dark) : base(spawnColors, dark, 0, false, 0, 0, "Basic", false, 1.0f)
        {
            name = "Swooper";
            baseDifficulty = 4;
            imageID = 12;
            SetDifficulty(spawnColors);
        }
        public SwooperChunk(GameModel.GameColor[] spawnColors) : base(spawnColors)
        {
            name = "Swooper";
            baseDifficulty = 4;
            imageID = 12;
            SetDifficulty(spawnColors);
        }

        public override WaveObject ChunkToWaveObject(bool isTutorial)
        {
            float spacing = isTutorial ? tutorialSpacing : globalWaveSpacing;
            WaveObject toReturn;
            if (colors.Length == 1)
            {
                toReturn = new WaveObject(WaveSpawningSystem.instance.Enemies[12], WaveSpawningSystem.instance.EnemyScripts[12], colors[0], spacing, TOPBOTTOM, isDarkened, WaveObject.Type.SWOOPER);
            }
            else
            {
                int rand = Random.Range(0, colors.Length);
                toReturn = new WaveObject(WaveSpawningSystem.instance.Enemies[12], WaveSpawningSystem.instance.EnemyScripts[12], colors[rand], spacing, TOPBOTTOM, isDarkened, WaveObject.Type.SWOOPER);
            }
            toReturn.isTutorial = isTutorial;
            return toReturn;
        }
        public override void Generate(bool tutorial)
        {
            base.Generate(tutorial, (int)globalWaveNumber);
        }

        public override Chunk MakeCopy()
        {
            return new SwooperChunk(colors, isDarkened);
        }
        public override Chunk MakeCopy(GameModel.GameColor[] colors, bool isDark)
        {
            return new SwooperChunk(colors, isDark);
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
            case Mechanic.RAGE:
                return c is RageChunk;
            case Mechanic.PAINTER:
                return c is PainterChunk;
            case Mechanic.EXPLOSIVE:
                return c is ExplosiveChunk;
            case Mechanic.SPLITTER:
                return c is SplitterChunk;
            case Mechanic.SWOOPER:
                return c is SwooperChunk;

        }

        return false;
    }

    public void LoadData(GameData data)
    {
        Level = data.currentLevel;
        currentMechanics = data.currentMechanics;
        newMechanics = data.currentNewMechanics;
        medMechanics = data.undiscoveredMedMechanics;
        basicMechanics = data.undiscoveredEasyMechanics;
        chunkDifficulties = data.chunkDifficulties.ToList();
        globalWaveNumber = data.waveNumber;
        globalWaveSpacing = data.waveSpacing;
        globalWaveSpeed = data.waveSpeed;
        globalRainbowMult = data.rainbowMult;
        if (data.chunks == null)
        {
            currentChunks = new List<Chunk>();
        }
        else
        {
            currentChunks = data.chunks;
        }
        numUniqueChunks = data.uniqueChunks;
        numChunks = data.numChunks;
    }

    public void SaveData(ref GameData data)
    {
        data.currentLevel = Level;
        data.currentMechanics = currentMechanics;
        data.currentNewMechanics = newMechanics;
        data.undiscoveredMedMechanics = medMechanics;
        data.undiscoveredEasyMechanics = basicMechanics;
        data.chunkDifficulties = chunkDifficulties.ToArray();
        data.waveNumber = globalWaveNumber;
        data.waveSpacing = globalWaveSpacing;
        data.waveSpeed = globalWaveSpeed;
        data.chunks = currentChunks;
        data.uniqueChunks = numUniqueChunks;
        data.numChunks = numChunks;
        data.rainbowMult = globalRainbowMult;
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
            SWIRL,
            RAGE,
            PAINTER,
            EXPLOSIVE,
            SPLITTER,
            SPLITTERBLOB,
            SWOOPER,
            TARGET
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

        public WaveObject MakeCopy()
        {
            return this;
        }
    }
}
