using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NinjaEnemyBehavior : EnemyBehavior
{

    public float sneakTime = 0.0f;
    private float sneakInterval = 1.0f;
    private float SNEAKINTERVAL = 1.5f;
    private float WAITINTERVAL = 0.65f;
    private float SNEAKSPEEDMULT = 1.75f;

    public enum Behavior
    {
        SNEAK,
        WAIT
    }

    public Behavior ninjaBehavior;

    public override void initialize(Vector3 des, GameModel.GameColor color, bool darkened, WaveSpawningSystem.WaveObject.Type eType)
    {
        base.initialize(des, color, darkened, eType);
        ninjaBehavior = Behavior.SNEAK;
        sneakTime = 0f;
        swayInterval = 0.1f;
    }

    
    public override void Move()
    {
        sneakTime += Time.deltaTime;
        if (sneakTime > sneakInterval)
        {
            //switches behavior from sneak to wait.
            ninjaBehavior = (ninjaBehavior.Equals(Behavior.WAIT) ? Behavior.SNEAK : Behavior.WAIT);
            sneakTime = 0.0f;
        }
        switch (ninjaBehavior)
        {
            case Behavior.SNEAK:
                moveSpeed = WaveSpawningSystem.globalWaveSpeed * SNEAKSPEEDMULT;
                sneakInterval = SNEAKINTERVAL;
                enableSway = true;
                break;
            case Behavior.WAIT:
                moveSpeed = 0.0f;
                sneakInterval = WAITINTERVAL;
                enableSway = false;
                break;
        }
        
        base.Move();
    }
}