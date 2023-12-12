using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetEnemyBehavior : EnemyBehavior
{
    public bool jumpInAnimation = false;
    public float jumpInTimer = 0f;
    public float jumpInInterval = 0.2f;
    private Vector3 jumpVector = Vector3.up * 0.4f;
    public Vector3 originalPos;
    public override void initialize(Vector3 des, GameModel.GameColor color, bool dark, WaveSpawningSystem.WaveObject.Type type)
    {
        base.initialize(des, color, dark, type);
        jumpInAnimation = true;
        SetVisualColor(color);
        rend.color = new Color(rend.color.r, rend.color.g, rend.color.b, 0f);
        jumpInTimer = 0f;
        destination = currentPos + jumpVector;
        originalPos = currentPos;
    }

    public override void Move()
    {
        if (jumpInAnimation)
        {
            jumpInTimer += Time.deltaTime;
            rend.color = new Color(rend.color.r, rend.color.g, rend.color.b, jumpInTimer / jumpInInterval);
            if (jumpInTimer < jumpInInterval / 2)
            {
                transform.position = Vector3.Lerp(originalPos, destination, jumpInTimer / (jumpInInterval/2));
                currentPos = transform.position;
            }
            else
            {
                transform.position = Vector3.Lerp(destination, originalPos, (jumpInTimer - (jumpInInterval/2)) / (jumpInInterval / 2));
                currentPos = transform.position;
            }
            if (jumpInTimer > jumpInInterval)
            {
                jumpInAnimation = false;
                destination = Vector3.zero;
            }
        }
    }
    public override void touchBullet(Bullet bullet)
    {
        if (!jumpInAnimation)
        {
            base.touchBullet(bullet);
        }
    }

    public override void Die(bool withSound)
    {
        base.Die(withSound);
        WaveSpawningSystem.instance.NextTutorialStage();
    }
}
