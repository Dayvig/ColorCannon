using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using UnityEngine;

public class PainterEnemyBehavior : EnemyBehavior
{
    public float stateTimer = 0.0f;
    public float paintTimer = 0.0f;
    private float paintInterval = 0.75f;
    private float waitInterval = 0.25f;
    private float moveInterval = 4f;
    public Behavior paintBehavior;
    public Vector3 playerPos;
    public float nextAngle;
    private float paintRange = 0.8f;
    private float paintArea = 0.8f;

    public enum Behavior
    {
        WAIT,
        PAINT,
        MOVE
    }

    public override void initialize(Vector3 des, GameModel.GameColor color, bool darkened, WaveSpawningSystem.WaveObject.Type eType)
    {
        base.initialize(des, color, darkened, eType);
        playerPos = des;
        paintBehavior = Behavior.MOVE;
        stateTimer = 0.0f;
    }

    Vector3 ChooseNewDirection()
    {
        if (GameModel.instance.isInBoundsPercent(currentPos, 0.8f))
        {
            return currentPos + (Vector3)(Random.insideUnitCircle * moveSpeed * moveInterval);
        }
        else
        {
            return playerPos;
        }
    }

    void Paint()
    {
        Vector3 paintPos = (Vector3)(Random.insideUnitCircle) + (Vector3.one * (paintRange / 2));
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(currentPos + paintPos, paintArea);
        foreach (Collider2D c in hitColliders)
        {
            if (c.gameObject.CompareTag("Enemy"))
            {
                EnemyBehavior enemy = c.gameObject.GetComponent<EnemyBehavior>();
                foreach (GameModel.GameColor nextColor in enemyColors)
                {
                    enemy.addColor(nextColor, isDarkened);
                }
            }
        }
        GameManager.instance.createSplatter(currentPos + paintPos, GameModel.instance.ColorToColor(enemyColor), paintArea);
        SoundManager.instance.PlaySFX(GameManager.instance.gameAudio, GameModel.instance.enemySounds[4], -0.1f, 0.1f);

    }


    public override void Move()
    {
        stateTimer += Time.deltaTime;
        if (stateTimer < moveInterval)
        {
            paintBehavior = Behavior.MOVE;
            moveSpeed = WaveSpawningSystem.globalWaveSpeed;
            enableSway = true;
        }
        else if (stateTimer < moveInterval + waitInterval)
        {
            paintBehavior = Behavior.WAIT;
            moveSpeed = 0.0f;
            enableSway = false;
        }
        else if (stateTimer < moveInterval + waitInterval + paintInterval)
        {
            paintBehavior = Behavior.PAINT;
            paintTimer += Time.deltaTime;
            if (paintTimer > paintInterval / 2)
            {
                Paint();
                paintTimer = 0.0f;
            }
            //todo: add some animation
        }
        else if (stateTimer < moveInterval + (waitInterval * 2) + paintInterval)
        {
            paintBehavior = Behavior.WAIT;
        }
        else
        {
            stateTimer = 0.0f;
            moveSpeed = WaveSpawningSystem.globalWaveSpeed;
            destination = ChooseNewDirection();
        }

        base.Move();
    }
}
