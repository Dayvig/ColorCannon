using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using UnityEngine;

public class SwirlEnemyBehavior : EnemyBehavior
{
    public float angleCounter = 0.0f;
    public float radiusCounter = 0.0f;
    public float radius;
    public float angle;
    private float ANGLEMOVEMENTCOEFF = 0.2f;
    private float DISTANCECOEFF = 0.002f;
    public int flipped = 1;

    public override void initialize(Vector3 des, GameModel.GameColor color, bool darkened, WaveSpawningSystem.WaveObject.Type eType)
    {
        base.initialize(des, color, darkened, eType);
        angleCounter = 0.0f;
        radiusCounter = 0.0f;
        flipped = (UnityEngine.Random.Range(0, 2) == 0) ? -1 : 1;
        setRadiusAndAngle();
    }

    private void setRadiusAndAngle()
    {
        Transform trans = this.transform;
        radius = Vector3.Distance(trans.position, destination);
        angle = Mathf.Atan(Math.Abs(destination.y - trans.position.y)/ Math.Abs(destination.x - trans.position.x));
        angle *= Mathf.Rad2Deg;
        if (angle > 360)
        {
            angle -= 360;
        }
        if (angle < -360)
        {
            angle += 360;
        }
    }

    private void setPos()
    {
        Transform trans = this.transform;
        float x = (float)((radius) * Math.Cos(angle * Mathf.Deg2Rad)) * 0.7f;
        float y = (float)((radius) * Math.Sin(angle * Mathf.Deg2Rad));
        trans.position = new Vector3(x, y, 0);
        currentPos = trans.position;
    }

    public override void Move()
    {
        angleCounter += Time.deltaTime * moveSpeed * ANGLEMOVEMENTCOEFF * flipped;
        radiusCounter += Time.deltaTime * moveSpeed * DISTANCECOEFF;
        angle += angleCounter;
        if (angle > 360 || angle < -360)
        {
            angle += angle > 360 ? -360 : 360;
        }
        radius -= radiusCounter;
        setPos();
    }

    public override void KnockBack()
    {
        knockBackTimer += Time.deltaTime;
        angleCounter += Time.deltaTime * knockbackSpeed * ANGLEMOVEMENTCOEFF * flipped;
        radiusCounter += Time.deltaTime * knockbackSpeed * DISTANCECOEFF;
        angle -= angleCounter;
        if (angle > 360 || angle < -360)
        {
            angle += angle > 360 ? -360 : 360;
        }
        radius += radiusCounter;
        setPos();
        if (knockBackTimer > knockBackDuration)
        {
            knockBack = false;
        }
    }
}