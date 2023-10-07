using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisguiserEnemyBehavior : EnemyBehavior
{

    public float timer = 0.0f;
    private float disguiseInterval = 0.6f;
    private float windupInterval = 1.0f;
    private float WINDUPSPEED = 0.4f;
    [SerializeField]
    private bool disguised = false;

    public enum Behavior
    {
        WINDUP,
        DISGUISE,
        MOVE
    }

    public Behavior disguiserBehavior;

    public override void initialize(Vector3 des, GameModel.GameColor color, bool darkened, WaveSpawningSystem.WaveObject.Type eType)
    {
        base.initialize(des, color, darkened, eType);
        disguiserBehavior = Behavior.WINDUP;
        timer = 0.0f;
        disguised = false;
    }

    public override void SetVisualColor(GameModel.GameColor thisColor)
    {
        Color c = disguised ? GameModel.instance.OppositeColor(thisColor) : GameModel.instance.ColorToColor(thisColor);
        if (!isDarkened)
        {
            rend.color = c;
        }
        else
        {
            rend.color = new Color(c.r / 1.5f, c.g / 1.5f, c.b / 1.5f, c.a);
        }
    }

    public override void KnockBack()
    {
        base.KnockBack();
    }

    public override void TakeHit(GameModel.GameColor bulletColor)
    {
        base.TakeHit(bulletColor);
        disguised = false;
        SetVisualColor(enemyColor);
        disguiserBehavior = Behavior.DISGUISE;
        timer = windupInterval;
    }

    public override void Move()
    {
        if (timer != -7f)
        {
            timer += Time.deltaTime;
            if (timer > windupInterval && timer < (disguiseInterval + windupInterval))
            {
                disguiserBehavior = Behavior.DISGUISE;
            }
            else if (timer > (disguiseInterval + windupInterval))
            {
                disguiserBehavior = Behavior.MOVE;
                disguised = true;
                SetVisualColor(enemyColor);
                timer = -7f;
            }
            else
            {
                disguiserBehavior = Behavior.WINDUP;
            }
        }
        switch (disguiserBehavior)
        {
            case Behavior.WINDUP:
                moveSpeed = WINDUPSPEED;
                break;
            case Behavior.DISGUISE:
                moveSpeed = 0.0f;
                break;
            case Behavior.MOVE:
                moveSpeed = GameModel.instance.baseGlobalWaveSpeed;
                break;
        }

        base.Move();
    }
}
