using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwarmEnemyBehavior : EnemyBehavior
{

    private float SPEEDMULT = 0.7f;
    public override void initialize(Vector3 des, GameModel.GameColor color, bool darkened, WaveSpawningSystem.WaveObject.Type eType)
    {
        base.initialize(des, color, darkened, eType);
        moveSpeed = moveSpeed * SPEEDMULT;
    }

    public override void Move()
    {
        base.Move();
    }
}

