using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        WAVE,
        PAUSED,
        POSTWAVE,
        WIN,
        LOSE
    }

    public enum UpgradeType
    {
        ATTACKSPEED,
        SHOTS,
        SHOTSPEED,
        SHOTSIZE,
        PIERCING
    }
    
    public class Upgrade
    {
        private String name;
        public GameManager.UpgradeType type;
        public GameModel.GameColor color;
        public Sprite image;
        
        public Upgrade(String upgradeName, GameManager.UpgradeType upgradeType, GameModel.GameColor upgradeColor)
        {
            name = upgradeName;
            type = upgradeType;
            color = upgradeColor;
        }
    }

    public List<EnemyBehavior> activeEnemies = new List<EnemyBehavior>();
    public List<Bullet> activeBullets = new List<Bullet>();
    public List<EnemyBehavior> markedForDeathEnemies = new List<EnemyBehavior>();
    public List<Bullet> markedForDeathBullets = new List<Bullet>();
    public List<Bullet> inactiveBullets = new List<Bullet>();
    public Player player;
    public UIManager uiManager;
    public WaveSpawningSystem spawningSystem;
    public static GameModel gameModel;
    private List<Upgrade> possibleUpgrades = new List<Upgrade>();

    public GameState currentState;
    public Upgrade selectedUpgrade;
    public UpgradeType selectedUpgradeType;

    public void SetState(GameState nextState)
    {
        if (!(nextState == GameState.WIN || nextState == GameState.LOSE))
        {
            uiManager.deactivateWinLoseUI();
        }
        if (currentState == GameState.WAVE && nextState == GameState.POSTWAVE)
        {
            spawningSystem.SetupNextWave();
            DisposeAllBullets();
            uiManager.activatePostWaveUI();
        }

        if (currentState == GameState.POSTWAVE && nextState == GameState.WAVE)
        {
            uiManager.deactivatePostWaveUI();
            if (selectedUpgrade != null)
            {
                player.upgrades.Add(selectedUpgrade);
            }
            selectedUpgrade = null;
            player.configureWeapon();
        }

        if (nextState == GameState.WIN && currentState != GameState.WIN)
        {
            uiManager.deactivatePostWaveUI();
            WipeAllEnemiesAndBullets();
            uiManager.activateWinScreen();
        }
        
        if (nextState == GameState.LOSE && currentState != GameState.LOSE)
        {
            uiManager.deactivatePostWaveUI();
            WipeAllEnemiesAndBullets();
            uiManager.activateLoseScreen();
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
        currentState = GameState.POSTWAVE;
        player = GameObject.Find("Player").GetComponent<Player>();
        spawningSystem = GetComponent<WaveSpawningSystem>();
        uiManager = GetComponent<UIManager>();
        uiManager.activatePostWaveUI();
        gameModel = GetComponent<GameModel>();
        
        spawningSystem.initialize();
        //addStartingUpgrades();
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
        for (int i = 0; i < 3; i++)
        {
            Upgrade nextUpgrade = getRandomUpgrade();
            uiManager.SetupUpgradePreview(nextUpgrade);
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
                break;
            case GameState.PAUSED:
                PausedUpdate();
                break;
            default:
                WaveUpdate();
                break;
        }
        Trash();
    }

    void Trash()
    {
        //Take out the TREASSH
        foreach (EnemyBehavior ded in markedForDeathEnemies)
        {
            //TODO: Add object pooling
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
        if ((spawningSystem.currentWaveIndex == WaveSpawningSystem.currentWave.Count-1) && (activeEnemies.Count == 0))
        {
            SetState(GameState.POSTWAVE);
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

    public void PlayAgain()
    {
        spawningSystem.chunkDifficulties.Clear();
        spawningSystem.basicMechanics.Clear();
        spawningSystem.currentMechanics.Clear();
        spawningSystem.newMechanics.Clear();
        spawningSystem.medMechanics.Clear();
        spawningSystem.availableChunks.Clear();
        spawningSystem.clearWave();
        spawningSystem.basicMechanics = new List<WaveSpawningSystem.Mechanic>
        {
            WaveSpawningSystem.Mechanic.DARK,
            WaveSpawningSystem.Mechanic.FAST,
            WaveSpawningSystem.Mechanic.NINJA,
            WaveSpawningSystem.Mechanic.TWOCOLOR
        };
        spawningSystem.medMechanics = new List<WaveSpawningSystem.Mechanic>
        {
            WaveSpawningSystem.Mechanic.THREECOLOR
        };
        spawningSystem.initialize();
        player.lives = player.baseLives;
        player.upgrades.Clear();
        player.configureWeapon();

        SetState(GameState.POSTWAVE);
        uiManager.activatePostWaveUI();
        GenerateUpgrades();
    }
}
