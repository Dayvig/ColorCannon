using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;
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
        LOSE
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

        [JsonConstructor]
        public Upgrade(String upgradeName, GameManager.UpgradeType upgradeType, GameModel.GameColor upgradeColor)
        {
            name = upgradeName;
            type = upgradeType;
            color = upgradeColor;
        }
        public Upgrade(String upgradeName, GameManager.UpgradeType upgradeType)
        {
            name = upgradeName;
            type = upgradeType;
            color = GameModel.GameColor.NONE;
        }
    }

    public List<EnemyBehavior> activeEnemies = new List<EnemyBehavior>();
    public List<Bullet> activeBullets = new List<Bullet>();
    public List<EnemyBehavior> markedForDeathEnemies = new List<EnemyBehavior>();
    public List<Bullet> markedForDeathBullets = new List<Bullet>();
    public List<Bullet> inactiveBullets = new List<Bullet>();
    public Player player;
    public WaveSpawningSystem spawningSystem;
    public static GameModel gameModel;
    private List<Upgrade> possibleUpgrades = new List<Upgrade>();
    public List<Upgrade> currentOfferedUpgrades = new List<Upgrade>();
    public List<WaveSpawningSystem.Mechanic> encounteredEnemies = new List<WaveSpawningSystem.Mechanic>();

    public GameState currentState;
    private Upgrade noUpgrade = new Upgrade("None", UpgradeType.NONE);
    public Upgrade selectedUpgrade = new Upgrade("None", UpgradeType.NONE);
    public UpgradeType selectedUpgradeType;

    public AudioSource gameAudio;
    public static GameManager instance { get; private set; }


    public void SetState(GameState nextState)
    {
        if (currentState == GameState.WAVE && nextState == GameState.POSTWAVE)
        {
            UnityEngine.Debug.Log("New Wave Generated");
            WaveSpawningSystem.currentChunks.Clear();
            spawningSystem.SetupNextWave();
            DisposeAllBullets();
            UIManager.instance.activatePostWaveUI();
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
                    player.upgrades.Add(selectedUpgrade);
                    UIManager.instance.AddNewPlayerUpgradeToPreview(selectedUpgrade);
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
        }
        currentState = nextState;

    }

    private void WipeAllEnemiesAndBullets()
    {
        foreach (EnemyBehavior e in activeEnemies)
        {
            e.Die();
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
        currentState = GameState.POSTWAVE;

        selectedUpgrade = noUpgrade;
    }

    public void addStartingUpgrades()
    {
        //Red Firing Speed
        possibleUpgrades.Add(new Upgrade("Rate of Fire", UpgradeType.ATTACKSPEED, GameModel.GameColor.RED));
        //Blue Firing Speed
        possibleUpgrades.Add(new Upgrade("Rate of Fire", UpgradeType.ATTACKSPEED, GameModel.GameColor.RED));
        //Yellow Firing Speed
        possibleUpgrades.Add(new Upgrade("Rate of Fire", UpgradeType.ATTACKSPEED, GameModel.GameColor.RED));
        
        //Red Shot Speed
        possibleUpgrades.Add(new Upgrade("Shot Speed", UpgradeType.SHOTSPEED, GameModel.GameColor.RED));
        //Blue Shot Speed
        possibleUpgrades.Add(new Upgrade("Shot Speed", UpgradeType.SHOTSPEED, GameModel.GameColor.BLUE));
        //Yellow Shot Speed
        possibleUpgrades.Add(new Upgrade("Shot Speed", UpgradeType.SHOTSPEED, GameModel.GameColor.YELLOW));

        //Red Shot Size
        possibleUpgrades.Add(new Upgrade("Shot Size", UpgradeType.SHOTSIZE, GameModel.GameColor.RED));
        //Blue Shot Size
        possibleUpgrades.Add(new Upgrade("Shot Size", UpgradeType.SHOTSIZE, GameModel.GameColor.BLUE));
        //Yellow Shot Size
        possibleUpgrades.Add(new Upgrade("Shot Size", UpgradeType.SHOTSIZE, GameModel.GameColor.YELLOW));
        
        //Red Shot Spread
        possibleUpgrades.Add(new Upgrade("Shot Spread", UpgradeType.SHOTS, GameModel.GameColor.RED));
        //Blue Shot Spread
        possibleUpgrades.Add(new Upgrade("Shot Spread", UpgradeType.SHOTS, GameModel.GameColor.BLUE));
        //Yellow Shot Spread
        possibleUpgrades.Add(new Upgrade("Shot Spread", UpgradeType.SHOTS, GameModel.GameColor.YELLOW));
        
        //Red Shot Spread
        possibleUpgrades.Add(new Upgrade("Shot Piercing", UpgradeType.PIERCING, GameModel.GameColor.RED));
        //Blue Shot Spread
        possibleUpgrades.Add(new Upgrade("Shot Piercing", UpgradeType.PIERCING, GameModel.GameColor.BLUE));
        //Yellow Shot Spread
        possibleUpgrades.Add(new Upgrade("Shot Piercing", UpgradeType.PIERCING, GameModel.GameColor.YELLOW));
        
        //Shield Recharge
        possibleUpgrades.Add(new Upgrade("Recharge", UpgradeType.SHIELDS, GameModel.GameColor.WHITE));
        //Shield Recharge
        possibleUpgrades.Add(new Upgrade("Recharge", UpgradeType.SHIELDS, GameModel.GameColor.WHITE));
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
        markedForDeathEnemies.Clear();
        markedForDeathBullets.Clear();
    }

    void WaveUpdate()
    {
        player.PlayerUpdate();
        spawningSystem.EnemyUpdate();
        if ((spawningSystem.currentWaveIndex >= WaveSpawningSystem.currentWave.Count-1) && (activeEnemies.Count <= 0))
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
