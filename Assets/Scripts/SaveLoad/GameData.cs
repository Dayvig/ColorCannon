using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static WaveSpawningSystem;

public class GameData
{
    public int currentLevel;
    public List<GameManager.Upgrade> playerUpgrades = new List<GameManager.Upgrade>();
    public List<UIManager.WaveModifier> waveUpgrades = new List<UIManager.WaveModifier>();
    public List<GameManager.Upgrade> currentUpgradesOffered = new List<GameManager.Upgrade>();
    public List<WaveSpawningSystem.Mechanic> currentNewMechanics = new List<WaveSpawningSystem.Mechanic>();
    public List<WaveSpawningSystem.Mechanic> currentMechanics = new List<WaveSpawningSystem.Mechanic>();
    public List<WaveSpawningSystem.Mechanic> undiscoveredEasyMechanics = new List<WaveSpawningSystem.Mechanic>();
    public List<WaveSpawningSystem.Mechanic> undiscoveredMedMechanics = new List<WaveSpawningSystem.Mechanic>();
    public List<WaveSpawningSystem.Mechanic> encounteredEnemies = new List<WaveSpawningSystem.Mechanic>();
    public List<Chunk> chunks;
    public int[] chunkDifficulties;
    public int uniqueChunks;
    public int waveNumber;
    public float waveSpacing;
    public float waveSpeed;
    public int playerLives;
    public int numChunks;

    public bool refreshActive;
    public GameData(){
        currentLevel = 0;
        refreshActive = true;
        undiscoveredEasyMechanics = new List<Mechanic> { Mechanic.FAST, Mechanic.NINJA, Mechanic.DARK, Mechanic.SWARM, Mechanic.ZIGZAG };
        undiscoveredMedMechanics = new List<Mechanic> { Mechanic.SWIRL, Mechanic.RAGE };
        chunkDifficulties = new int[]{ 1, 1, 2 };
        waveNumber = GameModel.instance.baseGlobalWaveNumber;
        waveSpacing = GameModel.instance.baseGlobalWaveSpacing;
        waveSpeed = GameModel.instance.baseGlobalWaveSpeed;
        numChunks = GameModel.instance.baseNumChunks;
        uniqueChunks = GameModel.instance.baseNumUniqueChunks;
        playerLives = 3;
    }
}
