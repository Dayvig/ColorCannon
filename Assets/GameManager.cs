using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        WAVE,
        PAUSED,
        POSTWAVE
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
        currentState = nextState;
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

    void Update()
    {
        switch (currentState)
        {
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
        
        //Take out the TREASSH
        foreach (EnemyBehavior ded in markedForDeathEnemies)
        {
            //TODO: Add object pooling
            activeEnemies.Remove(ded);
            Destroy(ded.gameObject);
        }

        foreach (Bullet ded in markedForDeathBullets)
        {
            activeBullets.Remove(ded);
            Destroy(ded.gameObject);
        }
        markedForDeathEnemies.Clear();
        markedForDeathBullets.Clear();
    }

    void WaveUpdate()
    {
        player.PlayerUpdate();
        spawningSystem.EnemyUpdate();
        foreach (EnemyBehavior enemy in activeEnemies)
        {
            enemy.EnemyUpdate();
        }
        foreach (Bullet bullet in activeBullets)
        {
            bullet.BulletUpdate();
        }
        if ((spawningSystem.currentWaveIndex == WaveSpawningSystem.currentWave.Count-1) && (activeEnemies.Count == 0))
        {
            SetState(GameState.POSTWAVE);
        }
    }

    void PostWaveUpdate()
    {

    }

    void PausedUpdate()
    {
        
    }
}
