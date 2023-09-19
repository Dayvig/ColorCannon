using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwirlEnemyBehavior : EnemyBehavior
{
    public float angleCounter = 0.0f;
    public float radiusCounter = 0.0f;
    public float radius;
    public float angle;
    private float ANGLEMOVEMENTCOEFF = 0.004f;
    private float DISTANCECOEFF = 0.002f;
    private int flipped = 1;

    public override void initialize(Vector3 des, GameModel.GameColor color, bool darkened, WaveSpawningSystem.WaveObject.Type eType)
    {
        base.initialize(des, color, darkened, eType);
        angleCounter = 0.0f;
        radiusCounter = 0.0f;
        radius = Vector3.Distance(this.transform.position, des);
        angle = ((180 / Mathf.PI) * Mathf.Atan2(des.y - currentPos.y,
            des.x - currentPos.x)) - 90;
        flipped = (UnityEngine.Random.Range(0, 1) == 0) ? -1 : 1;
    }

    public override void Move()
    {
        angleCounter += Time.deltaTime * moveSpeed * ANGLEMOVEMENTCOEFF * flipped;
        radiusCounter += Time.deltaTime * moveSpeed * DISTANCECOEFF;
        angle += angleCounter;
        if (angle > 360 || angle < -360)
        {
            angle = 0;
        }
        radius -= radiusCounter;
        Transform trans = this.transform;
        float x = (float) ((radius) * Math.Cos(angle));
        float y = (float) ((radius) * Math.Sin(angle));
        transform.position = new Vector3(x, y, 0);
        currentPos = trans.position;

    }
}