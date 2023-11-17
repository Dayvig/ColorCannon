using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
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
        MAINMENU
    }

    public enum UpgradeType
    {
        ATTACKSPEED,
        SHOTS,
        SHOTSPEED,
        SHOTSIZE,
        PIERCING,
        SHIELDS,
        NONE
    }

    [System.Serializable]
    public class Upgrade
    {
        public String name;
        public GameManager.UpgradeType type;
        public GameModel.GameColor color;
        public int factor;

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
    public List<DeathEffect> splatters = new List<DeathEffect>();
    public List<DeathEffect> markedForDeathSplatters = new List<DeathEffect>();
    public Player player;
    public WaveSpawningSystem spawningSystem;
    public static GameModel gameModel;
    private List<Upgrade> possibleUpgrades = new List<Upgrade>();
    public List<Upgrade> currentOfferedUpgrades = new List<Upgrade>();
    public List<WaveSpawningSystem.Mechanic> encounteredEnemies = new List<WaveSpawningSystem.Mechanic>();

    public GameState currentState = GameState.POSTWAVE;
    private Upgrade noUpgrade = new Upgrade("None", UpgradeType.NONE);
    public Upgrade selectedUpgrade = new Upgrade("None", UpgradeType.NONE);
    public UpgradeType selectedUpgradeType;
    public int shotsFired;
    public int shotsHit;

    public AudioSource gameAudio;
    public static GameManager instance { get; private set; }


    public void SetState(GameState nextState)
    {
        if ((currentState == GameState.WAVE || currentState == GameState.MAINMENU) && nextState == GameState.POSTWAVE)
        {
            Debug.Log("New Wave Generated");
            WaveSpawningSystem.currentChunks.Clear();
            spawningSystem.SetupNextWave();
            DisposeAllBullets();
            DisposeAllSplatters();
            DisposeAllEnemies();
            player.rainbowRush = false;
            player.meter.rainbows.fillAmount = 0;
            selectedUpgrade = noUpgrade;

            UIManager.instance.activatePostWaveAnimations(true);
            nextState = GameState.UIANIMATIONS;
        }

        if (currentState == GameState.POSTWAVE && nextState == GameState.WAVE)
        {
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
            Win();
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
            if (!SoundManager.instance.mainMusicPlaying)
            {
                SoundManager.instance.PlayMusicAndLoop(SoundManager.instance.mainMusic, GameModel.instance.music[0]);
            }
            else
            {
                SoundManager.instance.mainMusic.clip = GameModel.instance.music[0];
            }
            SoundManager.instance.mainMusicPlaying = true;
        }
        currentState = nextState;

    }

    void addNewUpgrade(Upgrade selected)
    {
        Debug.Log("Adding new Upgrade: Type " + selected.type + " Color " + selected.color);
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
                    Debug.Log("Incrementing existing");
                    addNew = false;
                    u.factor += selected.factor;
                }
            }
            if (addNew)
            {
                player.upgrades.Add(selected);
                UIManager.instance.AddNewPlayerUpgradeToPreview(selected, GameModel.instance.GetPlayerUpgradePreviewColorRowFromColor(selected.color));
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
    
    void Start()
    {
        instance = this;
        gameAudio = GetComponent<AudioSource>();

        SaveLoadManager.instance.initialize();
        player = GameObject.Find("Player").GetComponent<Player>();
        spawningSystem = GetComponent<WaveSpawningSystem>();
        gameModel = GetComponent<GameModel>();
        
        UIManager.instance.initialize();
        spawningSystem.initialize();
        currentState = GameState.MAINMENU;
        SoundManager.instance.PlayMusicAndLoop(SoundManager.instance.mainMusic, GameModel.instance.music[1]);

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
        
        /*
        //Red Shot Speed
        possibleUpgrades.Add(new Upgrade("Shot Speed", UpgradeType.SHOTSPEED, GameModel.GameColor.RED, 3));
        //Blue Shot Speed
        possibleUpgrades.Add(new Upgrade("Shot Speed", UpgradeType.SHOTSPEED, GameModel.GameColor.BLUE, 3));
        //Yellow Shot Speed
        possibleUpgrades.Add(new Upgrade("Shot Speed", UpgradeType.SHOTSPEED, GameModel.GameColor.YELLOW, 3));
        */

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
        
        for (int i = 0; i < GameModel.instance.shieldUpgradeFreq; i++)
        {
            //Shield Recharge
            possibleUpgrades.Add(new Upgrade("Recharge", UpgradeType.SHIELDS, GameModel.GameColor.WHITE, 1));
        }

        for (int i = 0; i < GameModel.instance.basicUpgradeFreq; i++)
        {
            possibleUpgrades.Add(new Upgrade("Rate of Fire", UpgradeType.ATTACKSPEED, GameModel.GameColor.WHITE, 1));
            //possibleUpgrades.Add(new Upgrade("Shot Speed", UpgradeType.SHOTSPEED, GameModel.GameColor.WHITE, 1));
            possibleUpgrades.Add(new Upgrade("Shot Size", UpgradeType.SHOTSIZE, GameModel.GameColor.WHITE, 1));
            possibleUpgrades.Add(new Upgrade("Shot Spread", UpgradeType.SHOTS, GameModel.GameColor.WHITE, 1));
            possibleUpgrades.Add(new Upgrade("Shot Piercing", UpgradeType.PIERCING, GameModel.GameColor.WHITE, 1));
        }
    }

    public Upgrade getRandomUpgrade()
    {
        return possibleUpgrades[Random.Range(0, possibleUpgrades.Count)];
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

    public void GenerateUpgrades()
    {
        if (currentOfferedUpgrades.Count == 0)
        {
            for (int i = 0; i < 3; i++)
            {
                Upgrade nextUpgrade = getRandomUpgrade();
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
            ded.gameObject.SetActive(false);
        }
        foreach (DeathEffect ded in markedForDeathSplatters)
        {
            splatters.Remove(ded);
            inactiveSplatters.Add(ded);
            ded.gameObject.SetActive(false);
        }

        markedForDeathEnemies.Clear();
        markedForDeathBullets.Clear();
        markedForDeathSplatters.Clear();

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
        Random.Range(0.6f, 1.4f);

        splatters.Add(newSplatter.GetComponent<DeathEffect>());
        inactiveSplatters.Remove(newDeathEffect);
    }

    void DemoUpdate()
    {
        player.PlayerDemoUpdate();
        spawningSystem.EnemyDemoUpdate();
    }

    void WaveUpdate()
    {
        player.PlayerUpdate();
        spawningSystem.EnemyUpdate();
        if ((spawningSystem.currentWaveIndex >= WaveSpawningSystem.currentWave.Count - 1) && (activeEnemies.Count <= 0))
        {
            if (WaveSpawningSystem.instance.Level >= 15)
            {
                UIManager.instance.activateWinScreen();
                WaveSpawningSystem.currentChunks.Clear();
                SaveLoadManager.instance.WipeMidRunDataOnly();
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
        UnityEngine.Debug.Log("Lose");
        UIManager.instance.deactivatePostWaveUI();
        WipeAllEnemiesAndBullets();
        UIManager.instance.activateLoseScreen();
        UIManager.instance.activateWinLoseAnimations(true, false);
        SetState(GameState.UIANIMATIONS);
    }

    public void Win()
    {
        UIManager.instance.deactivatePostWaveUI();
        WipeAllEnemiesAndBullets();
        UIManager.instance.activateWinScreen();
        UIManager.instance.activateWinLoseAnimations(true, true);
        SaveLoadManager.instance.WipeMidRunDataOnly();
    }

    public void PlayAgain()
    {
        UIManager.instance.deactivateWinLoseUI();
        SaveLoadManager.instance.WipeMidRunDataOnly();
        SaveLoadManager.instance.LoadGame();
        WaveSpawningSystem.instance.initialize();
        WaveSpawningSystem.instance.Level = 0;
        currentState = GameState.WAVE;
        SetState(GameState.POSTWAVE);
        SaveLoadManager.instance.SaveGame();
    }

    public void LoadData(GameData data)
    {
        currentOfferedUpgrades = data.currentUpgradesOffered;
        encounteredEnemies = data.encounteredEnemies;
    }

    public void SaveData(ref GameData data)
    {
        data.currentUpgradesOffered = currentOfferedUpgrades;
        data.encounteredEnemies = encounteredEnemies;
    }
}
