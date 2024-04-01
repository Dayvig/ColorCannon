using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour, IDataPersistence
{
    public enum GameState
    {
        WAVE,
        PAUSED,
        POSTWAVE,
        UIANIMATIONS,
        WIN,
        LOSE,
        MAINMENU,
        SETTINGS,
        NOTEBOOK
    }

    public enum UpgradeType
    {
        ATTACKSPEED,
        SHOTS,
        SHOTSPEED,
        SHOTSIZE,
        PIERCING,
        SHIELDS,
        MAXSHIELDS,
        PULSERADIUS,
        ROCKETS,
        COMBINER,
        DEATHBLAST,
        RAINBOWMULT,
        BARRAGE,
        PULSE,
        NONE
    }

    [System.Serializable]
    public class Upgrade
    {
        public String name;
        public GameManager.UpgradeType type;
        public GameModel.GameColor color;
        public int factor;
        public bool isPlayerUpgrade;

        [JsonConstructor]
        public Upgrade(String upgradeName, GameManager.UpgradeType upgradeType, GameModel.GameColor upgradeColor, int factor)
        {
            name = upgradeName;
            type = upgradeType;
            color = upgradeColor;
            this.factor = factor;
        }
        public Upgrade(String upgradeName, GameManager.UpgradeType upgradeType)
        {
            name = upgradeName;
            type = upgradeType;
            color = GameModel.GameColor.NONE;
            factor = 1;
        }

        public Upgrade MakeCopy()
        {
            return new Upgrade(name, type, color, factor);
        }
    }

    public List<EnemyBehavior> activeEnemies = new List<EnemyBehavior>();
    public List<Bullet> activeBullets = new List<Bullet>();
    public List<EnemyBehavior> markedForDeathEnemies = new List<EnemyBehavior>();
    public List<Bullet> markedForDeathBullets = new List<Bullet>();
    public List<Bullet> inactiveBullets = new List<Bullet>();
    public List<DeathEffect> inactiveSplatters = new List<DeathEffect>();
    public List<DeathEffect> inactiveGiblets = new List<DeathEffect>();
    public List<DeathEffect> splatters = new List<DeathEffect>();
    public List<DeathEffect> giblets = new List<DeathEffect>();
    public List<DeathEffect> markedForDeathSplatters = new List<DeathEffect>();
    public List<DeathEffect> markedForDeathGiblets = new List<DeathEffect>();
    public Player player;
    public WaveSpawningSystem spawningSystem;
    public static GameModel gameModel;
    private List<Upgrade> possibleUpgrades = new List<Upgrade>();
    private List<Upgrade> specialUpgrades = new List<Upgrade>();
    public List<Upgrade> currentOfferedUpgrades = new List<Upgrade>();
    public List<WaveSpawningSystem.Mechanic> encounteredEnemies = new List<WaveSpawningSystem.Mechanic>();

    public GameState currentState = GameState.MAINMENU;
    public GameState returnState = GameState.POSTWAVE;
    private Upgrade noUpgrade = new Upgrade("None", UpgradeType.NONE);
    public Upgrade selectedUpgrade = new Upgrade("None", UpgradeType.NONE);
    public UpgradeType selectedUpgradeType;
    public int shotsFired;
    public int shotsHit;

    public AudioSource gameAudio;

    public float splatterVal = 0.5f;
    public bool doubleTapCycle = false;

    public int promodeLevel = 0;
    public int maxProModeLevelUnlocked = 0;
    public int rainbowInk = 0;

    public static GameManager instance { get; private set; }

    public bool justLaunched = true;
    public int arena = 0;
    public int lastValidArena = 0;
    public List<int> unlockedArenas = new List<int>();

    void SetupForWave()
    {
        UIManager.instance.activatePostWaveUI();
        justLaunched = false;
        selectedUpgrade = noUpgrade;
        if (WaveSpawningSystem.instance.Level > 1)
        {
            if (WaveSpawningSystem.instance.Level % 2 == 0)
            {
                GenerateUpgrades();
            }
            else
            {
                GenerateSpecialUpgrades();
            }
        }
        else
        {
            UIManager.instance.SetUpgradesVisible(false);
        }
        DisposeAllBullets();
        DisposeAllSplatters();
        DisposeAllEnemies();
        
        if (player.rainbowRush)
        {
            player.rainbowRush = false;
            PostProcessingManager.instance.SetRainbowRush(false);
        }

        UIManager.instance.activatePostWaveAnimations(true);

        UIManager.instance.ClearSpecialUpgrades();
        UIManager.instance.SetupWaveModUI();
    }
    public void SetState(GameState nextState)
    {

        if (currentState != GameState.SETTINGS && currentState != GameState.UIANIMATIONS && nextState == GameState.SETTINGS)
        {
            UIManager.instance.SettingsPanel.SetActive(true);
            UIManager.instance.initSettings();
            UIManager.instance.activateSettingsAnimations(true);
            returnState = currentState;
            nextState = GameState.UIANIMATIONS;
        }

        if (currentState == GameState.SETTINGS && nextState != GameState.SETTINGS)
        {
            UIManager.instance.activateSettingsAnimations(false);
            nextState = GameState.UIANIMATIONS;
            SaveLoadManager.instance.SaveSettings();
        }

        if (currentState == GameState.MAINMENU && nextState == GameState.POSTWAVE)
        {
            spawningSystem.SetupNextWave();
            SetupForWave();
            nextState = GameState.UIANIMATIONS;
        }
        if (currentState == GameState.MAINMENU && nextState == GameState.NOTEBOOK)
        {
            nextState = GameState.UIANIMATIONS;
            UIManager.instance.activateMainMenuAnimations(false);

            DisposeAllBullets();
            DisposeAllSplatters();
            DisposeAllEnemies();

        }
        if (currentState == GameState.WAVE && nextState == GameState.POSTWAVE)
        {
            spawningSystem.IncreaseDifficulty();
            currentOfferedUpgrades.Clear();
            spawningSystem.SetupNextWave();
            SetupForWave();
            nextState = GameState.UIANIMATIONS;
            SaveLoadManager.instance.SaveGame();
        }

        if (currentState == GameState.POSTWAVE && nextState == GameState.WAVE)
        {
            DisposeAllBullets();
            DisposeAllSplatters();
            DisposeAllEnemies();

            UIManager.instance.deactivateMainMenuUI();
            if (selectedUpgrade != null && selectedUpgrade.type != UpgradeType.NONE)
            {
                if (selectedUpgrade.type == UpgradeType.SHIELDS)
                {
                    player.lives = player.baseLives;
                }
                else
                {
                    addNewUpgrade(selectedUpgrade);
                }
            }
            selectedUpgrade = noUpgrade;
            player.configureWeapon();

            UIManager.instance.activatePostWaveAnimations(false);
            nextState = GameState.UIANIMATIONS;
        }

        if (nextState == GameState.WIN && currentState != GameState.UIANIMATIONS)
        {
            nextState = GameState.UIANIMATIONS;
        }

        if (nextState == GameState.LOSE && currentState != GameState.UIANIMATIONS)
        {
            Lose();
            nextState = GameState.UIANIMATIONS;
        }
        if (currentState == GameState.UIANIMATIONS && nextState == GameState.WAVE)
        {
            UIManager.instance.deactivatePostWaveUI();
            if (!SoundManager.instance.mainMusicPlaying || SoundManager.instance.mainMusic.clip == GameModel.instance.music[0])
            {
                SoundManager.instance.PlayRandomMainTheme();
            }
                
            SoundManager.instance.mainMusicPlaying = true;
        }

        if (nextState == GameState.PAUSED)
        {
            SoundManager.instance.mainMusic.Pause();
        }
        if (currentState == GameState.PAUSED && nextState == GameState.WAVE)
        {
            SoundManager.instance.mainMusic.UnPause();
        }
        if ((currentState == GameState.PAUSED || currentState == GameState.WIN || currentState == GameState.LOSE || currentState == GameState.NOTEBOOK) && nextState == GameState.MAINMENU)
        {
            DisposeAllBullets();
            DisposeAllSplatters();
            DisposeAllEnemies();
            PostProcessingManager.instance.SetBlur(true);
            UIManager.instance.deactivateWaveUI();
            UIManager.instance.deactivateWinLoseUI();
            UIManager.instance.activateMainMenuUI();
            UIManager.instance.activateMainMenuAnimations(true);
            nextState = GameState.UIANIMATIONS;
        }
        currentState = nextState;

    }

    void addNewUpgrade(Upgrade selected)
    {
        if (selected.color == GameModel.GameColor.WHITE)
        {
            Upgrade red = selected.MakeCopy();
            red.color = GameModel.GameColor.RED;
            addNewUpgrade(red);

            Upgrade blue = selected.MakeCopy();
            blue.color = GameModel.GameColor.BLUE;
            addNewUpgrade(blue);

            Upgrade yellow = selected.MakeCopy();
            yellow.color = GameModel.GameColor.YELLOW;
            addNewUpgrade(yellow);
        }
        else
        {
            bool addNew = true;
            foreach (Upgrade u in player.upgrades)
            {
                if (u.type == selected.type && u.color == selected.color)
                {
                    addNew = false;
                    u.factor += selected.factor;
                }
            }
            if (addNew)
            {
                Upgrade newU = selected.MakeCopy();
                newU.isPlayerUpgrade = true;
                player.upgrades.Add(newU);
            }
        }
    }
    private void WipeAllEnemiesAndBullets()
    {
        foreach (EnemyBehavior e in activeEnemies)
        {
            e.Die(false);
        }

        foreach (Bullet b in activeBullets)
        {
            b.Die();
        }
        Trash();
    }

    void Awake()
    {
        instance = this;
        gameAudio = GetComponent<AudioSource>();
        spawningSystem = GetComponent<WaveSpawningSystem>();
        gameModel = GetComponent<GameModel>();
        player = GameObject.Find("Player").GetComponent<Player>();
    }

    void Start()
    {
        SaveLoadManager.instance.initialize();
        UIManager.instance.initialize();
        spawningSystem.initialize();
        currentState = GameState.MAINMENU;
        SoundManager.instance.PlayMusicAndLoop(SoundManager.instance.mainMusic, GameModel.instance.music[0]);

        selectedUpgrade = noUpgrade;
    }
    public void addStartingUpgrades()
    {
        //Red Firing Speed
        possibleUpgrades.Add(new Upgrade("Rate of Fire", UpgradeType.ATTACKSPEED, GameModel.GameColor.RED, 3));
        //Blue Firing Speed
        possibleUpgrades.Add(new Upgrade("Rate of Fire", UpgradeType.ATTACKSPEED, GameModel.GameColor.BLUE, 3));
        //Yellow Firing Speed
        possibleUpgrades.Add(new Upgrade("Rate of Fire", UpgradeType.ATTACKSPEED, GameModel.GameColor.YELLOW, 3));

        //Red Shot Size
        possibleUpgrades.Add(new Upgrade("Shot Size", UpgradeType.SHOTSIZE, GameModel.GameColor.RED, 3));
        //Blue Shot Size
        possibleUpgrades.Add(new Upgrade("Shot Size", UpgradeType.SHOTSIZE, GameModel.GameColor.BLUE, 3));
        //Yellow Shot Size
        possibleUpgrades.Add(new Upgrade("Shot Size", UpgradeType.SHOTSIZE, GameModel.GameColor.YELLOW, 3));
        
        //Red Shot Spread
        possibleUpgrades.Add(new Upgrade("Shot Spread", UpgradeType.SHOTS, GameModel.GameColor.RED, 3));
        //Blue Shot Spread
        possibleUpgrades.Add(new Upgrade("Shot Spread", UpgradeType.SHOTS, GameModel.GameColor.BLUE, 3));
        //Yellow Shot Spread
        possibleUpgrades.Add(new Upgrade("Shot Spread", UpgradeType.SHOTS, GameModel.GameColor.YELLOW, 3));
        
        //Red Shot Spread
        possibleUpgrades.Add(new Upgrade("Shot Piercing", UpgradeType.PIERCING, GameModel.GameColor.RED, 3));
        //Blue Shot Spread
        possibleUpgrades.Add(new Upgrade("Shot Piercing", UpgradeType.PIERCING, GameModel.GameColor.BLUE, 3));
        //Yellow Shot Spread
        possibleUpgrades.Add(new Upgrade("Shot Piercing", UpgradeType.PIERCING, GameModel.GameColor.YELLOW, 3));

        for (int i = 0; i < GameModel.instance.basicUpgradeFreq; i++)
        {
            possibleUpgrades.Add(new Upgrade("Rate of Fire", UpgradeType.ATTACKSPEED, GameModel.GameColor.WHITE, 1));
            possibleUpgrades.Add(new Upgrade("Shot Size", UpgradeType.SHOTSIZE, GameModel.GameColor.WHITE, 1));
            possibleUpgrades.Add(new Upgrade("Shot Spread", UpgradeType.SHOTS, GameModel.GameColor.WHITE, 1));
            possibleUpgrades.Add(new Upgrade("Shot Piercing", UpgradeType.PIERCING, GameModel.GameColor.WHITE, 1));
        }

        for (int i = 0; i < GameModel.instance.shieldUpgradeFreq; i++)
        {
            //Shield Recharge
            specialUpgrades.Add(new Upgrade("Recharge", UpgradeType.SHIELDS, GameModel.GameColor.WHITE, 1));
        }

        //Add special upgrades
        for (int k = 0; k < 2; k++)
        {
            specialUpgrades.Add(new Upgrade("Rockets", UpgradeType.ROCKETS, GameModel.GameColor.RED, 1));
            specialUpgrades.Add(new Upgrade("Rockets", UpgradeType.ROCKETS, GameModel.GameColor.BLUE, 1));
            specialUpgrades.Add(new Upgrade("Rockets", UpgradeType.ROCKETS, GameModel.GameColor.YELLOW, 1));
        }

        specialUpgrades.Add(new Upgrade("Shield Pulse Radius", UpgradeType.PULSERADIUS, GameModel.GameColor.NONE, 1));
        specialUpgrades.Add(new Upgrade("Shield Pulse Radius", UpgradeType.PULSERADIUS, GameModel.GameColor.NONE, 1));
        specialUpgrades.Add(new Upgrade("Shield Pulse Radius", UpgradeType.PULSERADIUS, GameModel.GameColor.NONE, 1));

        specialUpgrades.Add(new Upgrade("Pulse Generator", UpgradeType.PULSE, GameModel.GameColor.NONE, 1));
        specialUpgrades.Add(new Upgrade("Pulse Generator", UpgradeType.PULSE, GameModel.GameColor.NONE, 1));
        specialUpgrades.Add(new Upgrade("Pulse Generator", UpgradeType.PULSE, GameModel.GameColor.NONE, 1));

        specialUpgrades.Add(new Upgrade("Splat Pulse", UpgradeType.DEATHBLAST, GameModel.GameColor.NONE, 1));
        specialUpgrades.Add(new Upgrade("Splat Pulse", UpgradeType.DEATHBLAST, GameModel.GameColor.NONE, 1));

        specialUpgrades.Add(new Upgrade("Combiner", UpgradeType.COMBINER, GameModel.GameColor.NONE, 1));
        specialUpgrades.Add(new Upgrade("Combiner", UpgradeType.COMBINER, GameModel.GameColor.NONE, 1));
        specialUpgrades.Add(new Upgrade("Combiner", UpgradeType.COMBINER, GameModel.GameColor.NONE, 1));

        specialUpgrades.Add(new Upgrade("More Rainbows!", UpgradeType.RAINBOWMULT, GameModel.GameColor.NONE, 1));
        specialUpgrades.Add(new Upgrade("More Rainbows!", UpgradeType.RAINBOWMULT, GameModel.GameColor.NONE, 1));
        specialUpgrades.Add(new Upgrade("More Rainbows!", UpgradeType.RAINBOWMULT, GameModel.GameColor.NONE, 1));

    }

    public Upgrade getRandomUpgrade(List<Upgrade> upgradeList)
    {
        return upgradeList[Random.Range(0, upgradeList.Count)];
    }

    void DisposeAllBullets()
    {
        foreach (Bullet b in activeBullets)
        {
            markedForDeathBullets.Add(b);
        }
    }
    void DisposeAllSplatters()
    {
        foreach (DeathEffect d in splatters)
        {
            markedForDeathSplatters.Add(d);
        }
    }

    void DisposeAllEnemies()
    {
        foreach (EnemyBehavior e in activeEnemies)
        {
            markedForDeathEnemies.Add(e);
        }
    }

    void AddBarrage()
    {
        bool hasBarrage = false;
        foreach (Upgrade u in possibleUpgrades)
        {
            if (u.type.Equals(UpgradeType.BARRAGE))
            {
                hasBarrage = true;
            }
        }
        if (!hasBarrage)
        {
            possibleUpgrades.Add(new Upgrade("Barrage", UpgradeType.BARRAGE, GameModel.GameColor.NONE, 1));
            possibleUpgrades.Add(new Upgrade("Barrage", UpgradeType.BARRAGE, GameModel.GameColor.NONE, 1));
            possibleUpgrades.Add(new Upgrade("Barrage", UpgradeType.BARRAGE, GameModel.GameColor.NONE, 1));
        }

    }

    public void RemoveUpgradeFromPool(UpgradeType typeToRemove, GameModel.GameColor colorToRemove)
    {
        List<Upgrade> toRemoveList = new List<Upgrade>();
        foreach (Upgrade u in possibleUpgrades)
        {
            if (u.type.Equals(typeToRemove) && u.color.Equals(colorToRemove))
            {
                toRemoveList.Add(u);
            }
        }
        foreach (Upgrade u in specialUpgrades)
        {
            if (u.type.Equals(typeToRemove) && u.color.Equals(colorToRemove))
            {
                toRemoveList.Add(u);
            }
        }
        foreach (Upgrade u2 in toRemoveList)
        {
            possibleUpgrades.Remove(u2);
            specialUpgrades.Remove(u2);
        }

    }

    public void RemoveUpgradeFromPool(UpgradeType typeToRemove)
    {
        List<Upgrade> toRemoveList = new List<Upgrade>();
        foreach (Upgrade u in possibleUpgrades)
        {
            if (u.type.Equals(typeToRemove))
            {
                toRemoveList.Add(u);
            }
        }
        foreach (Upgrade u in specialUpgrades)
        {
            if (u.type.Equals(typeToRemove))
            {
                toRemoveList.Add(u);
            }
        }
        foreach (Upgrade u2 in toRemoveList)
        {
            possibleUpgrades.Remove(u2);
            specialUpgrades.Remove(u2);
        }

    }

    public void GenerateUpgrades()
    {
        foreach (Upgrade p in player.upgrades)
        {
            if (p.type.Equals(UpgradeType.ROCKETS))
            {
                AddBarrage();
            }
            if (p.type.Equals(UpgradeType.PULSE) && p.factor > 3)
            {
                RemoveUpgradeFromPool(p.type, p.color);
            }
            if (p.type.Equals(UpgradeType.DEATHBLAST)){ 
                RemoveUpgradeFromPool(p.type, p.color);
            }
        }

        if (currentOfferedUpgrades.Count == 0)
        {
            for (int i = 0; i < 3; i++)
            {
                Upgrade nextUpgrade = getRandomUpgrade(possibleUpgrades);

                bool regenerate = false;
                do
                {
                    regenerate = false;
                    foreach (Upgrade u in currentOfferedUpgrades)
                    {
                        if (u.type.Equals(nextUpgrade.type) && u.color.Equals(nextUpgrade.color))
                        {
                            regenerate = true;
                            nextUpgrade = getRandomUpgrade(possibleUpgrades);
                        }
                    }
                }
                while (regenerate);

                currentOfferedUpgrades.Add(nextUpgrade);
                UIManager.instance.SetupNextUpgradePreview(nextUpgrade);
            }
        }
        else
        {
            foreach (Upgrade u in currentOfferedUpgrades)
            {
                UIManager.instance.SetupNextUpgradePreview(u);
            }
        }
    }

    public void GenerateSpecialUpgrades()
    {
        if (currentOfferedUpgrades.Count == 0)
        {
            for (int i = 0; i < 3; i++)
            {
                Upgrade nextUpgrade = getRandomUpgrade(specialUpgrades);
                bool regenerate = false;
                do
                {
                    regenerate = false;
                    foreach (Upgrade u in currentOfferedUpgrades)
                    {
                        if (u.type == nextUpgrade.type && u.color == nextUpgrade.color)
                        {
                            regenerate = true;
                            nextUpgrade = getRandomUpgrade(specialUpgrades);
                        }
                    }
                }
                while (regenerate);

                currentOfferedUpgrades.Add(nextUpgrade);
                UIManager.instance.SetupNextUpgradePreview(nextUpgrade);
            }
        }
        else
        {
            foreach (Upgrade u in currentOfferedUpgrades)
            {
                UIManager.instance.SetupNextUpgradePreview(u);
            }
        }
    }

    public void GivePlayerRandomUpgrade()
    {
        addNewUpgrade(getRandomUpgrade(possibleUpgrades));
    }

    private void FixedUpdate()
    {
        switch (currentState)
        {
            case GameState.WAVE:
                WaveFixedUpdate();
                break;
            case GameState.MAINMENU:
                WaveFixedUpdate();
                break;
        }
        Trash();
    }
    
    void Update()
    {
        switch (currentState)
        {
            case GameState.WIN:
                break;
            case GameState.LOSE:
                break;
            case GameState.WAVE:
                WaveUpdate();
                break;
            case GameState.POSTWAVE:
                PostWaveUpdate();
                UIManager.instance.PostWaveUIAndAnimationUpdate();
                break;
            case GameState.PAUSED:
                PausedUpdate();
                break;
            case GameState.UIANIMATIONS:
                UIManager.instance.PostWaveUIAndAnimationUpdate();
                break;
            case GameState.MAINMENU:
                DemoUpdate();
                break;
            case GameState.SETTINGS:
                SettingsUpdate();
                break;
            case GameState.NOTEBOOK:
                NotebookUpdate();
                break;
            default:
                WaveUpdate();
                break;
        }
        Trash();
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit(0);
        }
    }

    void Trash()
    {
        //Take out the TREASSH
        foreach (EnemyBehavior ded in markedForDeathEnemies)
        {
            activeEnemies.Remove(ded);
            spawningSystem.inactiveEnemies.Add(ded);
            ded.gameObject.SetActive(false);
        }

        foreach (Bullet ded in markedForDeathBullets)
        {
            activeBullets.Remove(ded);
            inactiveBullets.Add(ded);
            ded.thisCollider.enabled = false;
            ded.gameObject.SetActive(false);
        }
        foreach (DeathEffect ded in markedForDeathSplatters)
        {
            splatters.Remove(ded);
            inactiveSplatters.Add(ded);
            ded.gameObject.SetActive(false);
        }
        foreach (DeathEffect ded in markedForDeathGiblets)
        {
            giblets.Remove(ded);
            inactiveGiblets.Add(ded);
            ded.gameObject.SetActive(false);
        }

        markedForDeathEnemies.Clear();
        markedForDeathBullets.Clear();
        markedForDeathSplatters.Clear();
        markedForDeathGiblets.Clear();

    }

    public void createSplatter(Vector3 location, Color color)
    {
        GameObject newSplatter;
        location += (Vector3)(Random.insideUnitCircle * 0.3f);
        if (inactiveSplatters.Count == 0)
        {
           newSplatter = Instantiate(GameModel.instance.DeathSplatter, location, Quaternion.identity);
        }
        else
        {
            newSplatter = inactiveSplatters[0].gameObject;
        }
        newSplatter.transform.position = location;
        DeathEffect newDeathEffect = newSplatter.GetComponent<DeathEffect>();
        newDeathEffect.initialize();
        newDeathEffect.ren.color = color;
        newSplatter.transform.localScale = newDeathEffect.baseScale;
        newSplatter.transform.localScale *= Random.Range(0.5f, 2f);
        Random.Range(0.6f, 1.4f);

        splatters.Add(newSplatter.GetComponent<DeathEffect>());
        inactiveSplatters.Remove(newDeathEffect);                    
    }

    public void createSplatter(Vector3 location, Color color, float scale)
    {
        GameObject newSplatter;
        location += (Vector3)(Random.insideUnitCircle * 0.3f);
        if (inactiveSplatters.Count == 0)
        {
            newSplatter = Instantiate(GameModel.instance.DeathSplatter, location, Quaternion.identity);
        }
        else
        {
            newSplatter = inactiveSplatters[0].gameObject;
        }
        newSplatter.transform.position = location;
        DeathEffect newDeathEffect = newSplatter.GetComponent<DeathEffect>();
        newDeathEffect.initialize();
        newDeathEffect.ren.color = color;
        newSplatter.transform.localScale = Vector3.one * scale;

        splatters.Add(newSplatter.GetComponent<DeathEffect>());
        inactiveSplatters.Remove(newDeathEffect);
    }

    public void createGiblet(Vector3 location, Color color)
    {
        GameObject newGiblet;
        DeathEffect newDeathEffect;

        location += (Vector3)(Random.insideUnitCircle * 0.3f);
        if (inactiveSplatters.Count == 0)
        {
            newGiblet = Instantiate(GameModel.instance.DeathGiblet, location, Quaternion.identity);
            newDeathEffect = newGiblet.GetComponent<DeathEffect>();

        }
        else
        {
            newGiblet = inactiveGiblets[0].gameObject;
            newDeathEffect = newGiblet.GetComponent<DeathEffect>();
            inactiveGiblets.Remove(newGiblet.GetComponent<DeathEffect>());
        }

        newGiblet.transform.position = location;
        newDeathEffect.initialize();
        newDeathEffect.ren.color = color;
        newGiblet.transform.localScale = newDeathEffect.baseScale;
        newGiblet.transform.localScale *= Random.Range(0.5f, 2f);

        giblets.Add(newDeathEffect);
    }

    void DemoUpdate()
    {
        player.PlayerDemoUpdate();
        spawningSystem.EnemyDemoUpdate();
    }

    void SettingsUpdate()
    {
        UIManager.instance.SettingsUpdate();
    }

    void NotebookUpdate()
    {

    }

    void WaveUpdate()
    {
        player.PlayerUpdate();
        spawningSystem.EnemyUpdate();
        if ((spawningSystem.currentWaveIndex >= WaveSpawningSystem.currentWave.Count - 1) && (activeEnemies.Count <= 0))
        {
            if (WaveSpawningSystem.instance.Level == 0)
            {
                player.lives = 3;
            }

            if (WaveSpawningSystem.instance.Level >= 15)
            {
                Win();
                GameManager.instance.SetState(GameState.WIN);
            }
            else
            {
                UIManager.instance.activatePostWaveUI();
                GameManager.instance.SetState(GameState.POSTWAVE);
                WaveSpawningSystem.currentChunks.Clear();
            }
        }
    }

    void WaveFixedUpdate()
    {
        foreach (EnemyBehavior enemy in activeEnemies)
        {
            enemy.EnemyUpdate();
        }
        foreach (Bullet bullet in activeBullets)
        {
            bullet.BulletUpdate();
        }
    }

    void PostWaveUpdate()
    {

    }

    void PausedUpdate()
    {
        
    }

    public void Lose()
    {
        UIManager.instance.deactivatePostWaveUI();
        WipeAllEnemiesAndBullets();
        UIManager.instance.activateLoseScreen();
        UIManager.instance.activateWinLoseAnimations(true, false);
        WaveSpawningSystem.instance.Level = 0;

        SaveLoadManager.instance.WipeMidRunDataOnly();
        SoundManager.instance.mainMusic.Stop();
        SoundManager.instance.PlayMusicAndLoop(SoundManager.instance.mainMusic, GameModel.instance.music[5]);
        SaveLoadManager.instance.LoadGame();
    }

    public void Win()
    {
        maxProModeLevelUnlocked++;
        GameManager.instance.rainbowInk += 5000;

        UIManager.instance.deactivatePostWaveUI();
        WipeAllEnemiesAndBullets();
        UIManager.instance.activateWinScreen();
        UIManager.instance.activateWinLoseAnimations(true, true);
        SoundManager.instance.mainMusic.Stop();
        SaveLoadManager.instance.WipeMidRunDataOnly();
        SaveLoadManager.instance.SaveUnlocks();
        SaveLoadManager.instance.LoadUnlocks();
        SaveLoadManager.instance.LoadGame();
    }

    public void PlayAgain()
    {
        int prevProMode = GameManager.instance.promodeLevel;

        PostProcessingManager.instance.SetBlur(false);
        UIManager.instance.deactivateWinLoseUI();
        SaveLoadManager.instance.WipeMidRunDataOnly();
        SaveLoadManager.instance.LoadGame();
        GameManager.instance.promodeLevel = prevProMode;
        WaveSpawningSystem.instance.Level = 0;
        player.rainbowMeter = 0f;
        player.rainbowRush = false;
        PostProcessingManager.instance.SetRainbowRush(false);
        player.rainbowTimer = 0f;
        player.lives = 3;
        WaveSpawningSystem.instance.initialize();
        WaveSpawningSystem.instance.AddProModeFeatures();
        /*if (WaveSpawningSystem.instance.tutorialStage == -1)
        {
            currentState = GameState.POSTWAVE;
            SetState(GameState.WAVE);
        }
        else
        {
            currentState = GameState.MAINMENU;
            SetState(GameState.POSTWAVE);
        }*/
        SoundManager.instance.mainMusic.Stop();
        SoundManager.instance.PlayRandomMainTheme();

        currentState = GameState.POSTWAVE;
        SetState(GameState.WAVE);
    }

    public void LoadData(GameData data)
    {
        currentOfferedUpgrades = data.currentUpgradesOffered;
        encounteredEnemies = data.encounteredEnemies;
        promodeLevel = data.promodeLevel;
    }

    public void SaveData(ref GameData data)
    {
        data.currentUpgradesOffered = currentOfferedUpgrades;
        data.encounteredEnemies = encounteredEnemies;
        data.promodeLevel = promodeLevel;
    }
}
