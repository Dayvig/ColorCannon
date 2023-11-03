using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RageEnemyBehavior : EnemyBehavior
{
    private float rageSpeedMultInit = 2f;
    private float rageSpeedMult = 1.05f;
    private float rageWaitTimer = 0.0f;
    private float rageDelay = 0.5f;
    private int timesMultiplied = 0;
    // Start is called before the first frame update

    public override void initialize(Vector3 des, GameModel.GameColor color, bool dark, WaveSpawningSystem.WaveObject.Type type)
    {
        base.initialize(des, color, dark, type);
        timesMultiplied = 0;
        enableSway = true;
    }
    public override void touchBullet(Bullet bullet)
    {
        if (bullet.immuneEnemies.Count > 0 && bullet.immuneEnemies.Contains(this))
        {
            return;
        }
        if (enemyColors.Contains(bullet.bulletColor) || enemyColor == bullet.bulletColor)
        {
            TakeHit(bullet.bulletColor);
            bullet.TakeHit();
            bullet.immuneEnemies.Add(this);
        }
        else
        {
            //play sound
            bullet.TakeHit();
            bullet.immuneEnemies.Add(this);
            timesMultiplied++;
            if (timesMultiplied == 1)
            {
                this.transform.rotation = Quaternion.Euler(new Vector3(transform.localRotation.x, transform.localRotation.y, 0));
                if (enableSway)
                {
                    rageWaitTimer = rageDelay;
                    SoundManager.instance.PlaySound(this.enemySource, GameModel.instance.enemySounds[3]);
                    moveSpeed *= rageSpeedMultInit;
                }
                enableSway = false;
            }
            else
            {
                moveSpeed *= rageSpeedMult;
            }
        }
    }

    public override void Move()
    {
        if (rageWaitTimer <= 0.0f)
        {
            base.Move();
        }
        else
        {
            rageWaitTimer -= Time.deltaTime;
            transform.position = Vector3.MoveTowards(currentPos, destination, -Time.deltaTime * Mathf.Lerp((moveSpeed / 4), 0, (rageDelay-rageWaitTimer) / rageDelay));
        }
    }
}
