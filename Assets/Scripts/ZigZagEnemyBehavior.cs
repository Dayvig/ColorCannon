using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZigZagEnemyBehavior : EnemyBehavior
{

    public float sneakTime = 0.0f;
    private float sneakInterval = 1.0f;
    private float SNEAKINTERVAL = 1.0f;
    private float WAITINTERVAL = 0.5f;
    private float SNEAKSPEEDMULT = 1.5f;
    private float EXTENSIONVALMULT = 0.4f;
    private float extension = 0.4f;
    private Vector3 originalDestination;
    private int flipped = 1;
    private bool firstMove = true;

    public enum Behavior
    {
        MOVE,
        WAIT
    }

    public Behavior zigzagBehavior;

    public override void initialize(Vector3 des, GameModel.GameColor color, bool darkened, WaveSpawningSystem.WaveObject.Type eType)
    {
        base.initialize(des, color, darkened, eType);
        firstMove = true;
        zigzagBehavior = Behavior.MOVE;
        sneakTime = 0f;
        originalDestination = des;
        flipped = (UnityEngine.Random.Range(0, 1) == 0) ? -1 : 1;
        setNewDestination();
    }

    public void setNewDestination()
    {
            Vector3 origin = Vector3.Lerp(this.transform.position, originalDestination, 0.5f);
            Debug.DrawLine(this.transform.position, origin, Color.white, 4f);
            Vector3 newDest = origin + (new Vector3(flipped * -origin.y, flipped * origin.x, origin.z) * (firstMove ? EXTENSIONVALMULT : EXTENSIONVALMULT * 2f));
            Debug.DrawLine(origin, newDest, Color.red, 4f);   
            flipped *= -1;
        if (Vector3.Distance(newDest, originalDestination) <= 0.4f)
        {
            newDest = originalDestination;
        }
            destination = newDest;
            zigzagBehavior = Behavior.WAIT;
            sneakTime = 0.0f;
        if (firstMove)
            firstMove = false;
    }


    public override void Move()
    {
        if (Vector3.Distance(this.transform.position, destination) < 0.01f)
        {
            setNewDestination();
        }
        else
        {
            sneakTime += Time.deltaTime;
            if (sneakTime >= WAITINTERVAL)
            {
                zigzagBehavior = Behavior.MOVE;
            }
        }
        switch (zigzagBehavior)
        {
            case Behavior.MOVE:
                moveSpeed = WaveSpawningSystem.globalWaveSpeed * SNEAKSPEEDMULT;
                sneakInterval = SNEAKINTERVAL;
                break;
            case Behavior.WAIT:
                moveSpeed = 0.0f;
                sneakInterval = WAITINTERVAL;
                break;
        }

        base.Move();
    }
}