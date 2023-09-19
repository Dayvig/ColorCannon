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
    public int[] chunkDifficulties;

    public bool refreshActive;
    public GameData(){

        currentLevel = 1;
        refreshActive = true;
        undiscoveredEasyMechanics = new List<Mechanic> { Mechanic.FAST, Mechanic.NINJA, Mechanic.ORANGE, Mechanic.GREEN, Mechanic.PURPLE, Mechanic.DARK, Mechanic.SWARM, Mechanic.ZIGZAG };
        undiscoveredMedMechanics = new List<Mechanic> { Mechanic.WHITE, Mechanic.DISGUISED, Mechanic.SWIRL };
        chunkDifficulties = new int[]{ 1, 1, 2 };
    }
}
