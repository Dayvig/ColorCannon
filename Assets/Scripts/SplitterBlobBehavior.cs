using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplitterBlobBehavior : EnemyBehavior
{

    public bool left = false;
    Vector3 originalDestination;
    public float splitDistance = 1.5f;

    public enum Behavior
    {
        SPLIT,
        MOVE
    }

    public Behavior thisBehavior = Behavior.MOVE;

    // Start is called before the first frame update
    public override void initialize(Vector3 des, GameModel.GameColor color, bool dark, WaveSpawningSystem.WaveObject.Type type)
    {
        base.initialize(des, color, dark, type);
        swayAngle = 5;
        thisBehavior = Behavior.SPLIT;
        originalDestination = des;
        Debug.Log("1");
    }

    public void initializeSplit(bool isLeft)
    {
        Debug.Log("2");
        left = isLeft;
        setNewDestination();
    }

    public void setNewDestination()
    {
        Vector3 currentPos = gameObject.transform.position;
        int flipped = left ? 1 : -1;

        Vector3 newDest = currentPos + (new Vector3(flipped * -currentPos.y, flipped * currentPos.x, 0).normalized * splitDistance);
        Debug.DrawLine(currentPos, newDest, Color.red, 4f);
        setDestination(newDest);
        enableSway = false;
        moveSpeed = WaveSpawningSystem.globalWaveSpeed * 5f;

        Debug.Log("Setting new Destination " + newDest);
        Debug.Log("New Destination" + destination);
    }

    public override void Move()
    {
        if (Vector3.Distance(this.transform.position, destination) < 0.01f)
        {
            Debug.Log("Are you being called?");
            thisBehavior = Behavior.MOVE;
            enableSway = true;
            moveSpeed = WaveSpawningSystem.globalWaveSpeed;
            destination = originalDestination;
        }
        base.Move();
    }
}


