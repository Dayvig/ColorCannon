using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        WAVE,
        PAUSED,
        POSTWAVE
    }

    public List<EnemyBehavior> activeEnemies = new List<EnemyBehavior>();
    public List<Bullet> activeBullets = new List<Bullet>();
    public List<EnemyBehavior> markedForDeathEnemies = new List<EnemyBehavior>();
    public List<Bullet> markedForDeathBullets = new List<Bullet>();
    public Player player;
    public UIManager uiManager;
    public WaveSpawningSystem spawningSystem;

    public GameState currentState;

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
    }

    void DisposeAllBullets()
    {
        foreach (Bullet b in activeBullets)
        {
            markedForDeathBullets.Add(b);
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
