using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FastEnemyBehavior : EnemyBehavior
{

    public float windUpTime = 0.0f;
    private float windUpInterval = 1.5f;
    private float waitInterval = 1.5f;
    private float WINDUPSPEED = 0.5f;
    private float FASTSPEEDMULT = 3f;
    private float rotationTarget;

    public enum Behavior
    {
        WINDUP,
        WAIT,
        FASTMOVEMENT
    }

    public Behavior fastBehavior;

    public override void initialize(Vector3 des, GameModel.GameColor color, bool darkened, WaveSpawningSystem.WaveObject.Type eType)
    {
        base.initialize(des, color, darkened, eType);
        fastBehavior = Behavior.WINDUP;
        windUpTime = 0.0f;
        
        //Sets rotation to look at player
        Quaternion rot = gameObject.transform.rotation;
        float angle = ((180 / Mathf.PI) * Mathf.Atan2(des.y -currentPos.y,
            des.x - currentPos.x)) - 90;
        rotationTarget = angle;
        gameObject.transform.rotation = Quaternion.Euler(new Vector3(rot.x, rot.y, rotationTarget));
    }

    public override void Move()
    {
        windUpTime += Time.deltaTime;
        if (windUpTime > windUpInterval &&  windUpTime < (waitInterval+windUpInterval))
        {
            fastBehavior = Behavior.WAIT;
        }
        else if (windUpTime > (waitInterval+windUpInterval))
        {
            fastBehavior = Behavior.FASTMOVEMENT;
        }
        else
        {
            fastBehavior = Behavior.WINDUP;
        }
        switch (fastBehavior)
        {
            case Behavior.WINDUP:
                moveSpeed = WINDUPSPEED;
                break;
            case Behavior.WAIT:
                moveSpeed = 0.0f;
                break;
            case Behavior.FASTMOVEMENT:
                moveSpeed = WaveSpawningSystem.globalWaveSpeed * FASTSPEEDMULT;
                break;
        }

        transform.position = Vector3.MoveTowards(currentPos, destination, Time.deltaTime * moveSpeed);
        currentPos = transform.position;
    }
}
