using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class SwooperEnemyBehavior : EnemyBehavior
{

    public int swoopCount = 0;
    int swoopNumber = 3;
    private float rotationTarget;
    public float currentAngle;
    public float minSpeed;
    public float maxSpeed;
    public float maxDist;
    public float dist;

    public override void initialize(Vector3 des, GameModel.GameColor color, bool dark, WaveSpawningSystem.WaveObject.Type type)
    {
        base.initialize(des, color, dark, type);
        enableSway = false;
        setNewDestination();
        minSpeed = WaveSpawningSystem.globalWaveSpeed;
        maxSpeed = WaveSpawningSystem.globalWaveSpeed * 4f;
        swoopCount = 0;
    }

    void setNewDestination()
    {
        swoopCount++;
        LookAtPlayer(swoopCount >= swoopNumber);
        maxDist = Vector3.Distance(currentPos, destination);
    }

    void LookAtPlayer(bool final)
    {
        Quaternion rot = gameObject.transform.rotation;
        float angle = Mathf.Rad2Deg * Mathf.Atan2(-currentPos.x,
            -currentPos.y);
        rotationTarget = angle;
        if (!final)
        {
            if (Random.Range(0, 1) == 1)
            {
               rotationTarget += Random.Range(15, 40);
            }
            else
            {
                rotationTarget -= Random.Range(15, 40);
            }
        }
        currentAngle = rotationTarget;
        float xSpeed = Mathf.Sin(Mathf.Deg2Rad * rotationTarget);
        float ySpeed = Mathf.Cos(Mathf.Deg2Rad * rotationTarget);
        Vector3 flight = new Vector3(xSpeed, ySpeed, 0).normalized;
        while (GameModel.instance.isInBounds(flight))
        {
            flight *= 1.2f;
        }
        destination = flight;
        angle = Mathf.Rad2Deg * Mathf.Atan2(destination.y - currentPos.y,
        destination.x - currentPos.x) - 90;
        gameObject.transform.rotation = Quaternion.Euler(new Vector3(rot.x, rot.y, angle));

    }

    public override void Move()
    {
        dist = Vector3.Distance(currentPos, destination);
        if (dist < maxDist * 0.75f)
        {
            moveSpeed = Mathf.Lerp(minSpeed, maxSpeed, (maxDist - dist) / maxDist * 0.75f);
        }
        else
        {
            moveSpeed = Mathf.Lerp(maxSpeed, minSpeed, (maxDist - dist) / maxDist * 0.25f);
        }
        base.Move();
        if (Vector3.Distance(this.transform.position, destination) < 0.01f)
        {
            setNewDestination();
        }
    }
}
