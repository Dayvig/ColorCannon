using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveEnemyBehavior : EnemyBehavior
{
    private float paintRange = 0f;
    private float paintArea = 1f;
    public AudioSource source2;
    public override void TakeHit(GameModel.GameColor bulletColor)
    {
        if (enemyColors.Count <= 1 && !isDarkened)
        {
            Paint();
        }
        base.TakeHit(bulletColor);
    }


    void Paint()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(currentPos, paintArea);
        foreach (Collider2D c in hitColliders)
        {
            if (c.gameObject.CompareTag("Enemy"))
            {
                Debug.Assert(c.gameObject.GetComponent<EnemyBehavior>() != null);
                EnemyBehavior enemy = c.gameObject.GetComponent<EnemyBehavior>();
                foreach (GameModel.GameColor nextColor in enemyColors)
                {
                    enemy.addColor(nextColor, isDarkened);
                }
            }
        }
        GameManager.instance.createSplatter(currentPos, GameModel.instance.ColorToColor(enemyColor), paintArea);
        SoundManager.instance.PlaySFX(GameManager.instance.gameAudio, GameModel.instance.enemySounds[4], -0.1f, 0.1f);
    }
}
