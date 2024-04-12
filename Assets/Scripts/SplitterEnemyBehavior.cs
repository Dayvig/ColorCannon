using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplitterEnemyBehavior : EnemyBehavior
{

    WaveSpawningSystem.SplitterChunk newSplitters;
    WaveSpawningSystem.SplitterBlob newBlobs;
    WaveSpawningSystem.WaveObject newSplittersObject;
    WaveSpawningSystem.WaveObject newBlobWaveObj;
    bool isLeft = false;
    Vector3 originalDestination;
    public float splitDistance = 1.5f;


    enum Behavior
    {
        SPLIT,
        MOVE
    }

    Behavior thisBehavior = Behavior.MOVE;

    public void initializeSplit(bool isSplit, bool left)
    {
        isLeft = left;
        if (isSplit)
        {
            thisBehavior = Behavior.SPLIT;
            setNewDestination();
        }
        else
        {
            thisBehavior = Behavior.MOVE;
        }
        immuneToDamage = true;
    }

    public override void initialize(Vector3 des, GameModel.GameColor color, bool dark, WaveSpawningSystem.WaveObject.Type type)
    {
        base.initialize(des, color, dark, type);
        enableSway = true;
        originalDestination = des;
    }

    public override void TakeHit(GameModel.GameColor bulletColor)
    {
        GameManager.instance.createSplatter(this.transform.position, GameModel.instance.ColorToColor(enemyColor));
        if (isDarkened)
        {
            isDarkened = false;
            SetVisualColor(enemyColor);
            StartKnockBack(true);
            return;
        }

        if (enemyColors.Count > 2)
        {
            List<GameModel.GameColor> colorsToMix = new List<GameModel.GameColor>();
            colorsToMix.Add(enemyColors[0]);
            colorsToMix.Add(enemyColors[1]);
            CreateNewSplitter(new GameModel.GameColor[] { SetMixedColor(colorsToMix) }, false);
            CreateNewSplitter(new GameModel.GameColor[] { enemyColors[2] }, true);
            Die(false);
        }
        if (enemyColors.Count == 2)
        {
            CreateNewSplitter(new GameModel.GameColor[] { enemyColors[0] }, true);
            CreateNewSplitter(new GameModel.GameColor[] { enemyColors[1] }, false);
            Die(false);
        }
        else if (enemyColors.Count == 1)
        {
            CreateNewSplitterBlob(new GameModel.GameColor[] { enemyColors[0] }, true, transform.position);
            CreateNewSplitterBlob(new GameModel.GameColor[] { enemyColors[0] }, false, transform.position);
            Die(false);
        }

        GameManager.instance.shotsHit++;
    }

    public void CreateNewSplitter(GameModel.GameColor[] color, bool isLeft)
    {
        newSplitters = new WaveSpawningSystem.SplitterChunk(color);
        newSplittersObject = newSplitters.ChunkToWaveObject(false);
        GameObject newSplitter = WaveSpawningSystem.instance.CreateEnemyAtLocation(transform.position, newSplittersObject);
        SplitterEnemyBehavior newScript = newSplitter.GetComponent<SplitterEnemyBehavior>();
        newSplitter.transform.position = transform.position;
        newScript.initializeSplit(true, isLeft);
    }

    public void CreateNewSplitterBlob(GameModel.GameColor[] color, bool isLeft, Vector3 toSpawn)
    {
        newBlobs = new WaveSpawningSystem.SplitterBlob(color);
        newBlobWaveObj = newBlobs.ChunkToWaveObject(false);
        GameObject newBlob = WaveSpawningSystem.instance.CreateEnemyAtLocation(toSpawn, newBlobWaveObj);
        SplitterBlobBehavior newScript = newBlob.GetComponent<SplitterBlobBehavior>();
        newBlob.transform.position = toSpawn;
        newScript.initializeSplit(isLeft);

    }

    public override void Move()
    {
        if (Vector3.Distance(this.transform.position, destination) < 0.01f)
        {
            thisBehavior = Behavior.MOVE;
            enableSway = true;
            moveSpeed = WaveSpawningSystem.globalWaveSpeed;
            destination = originalDestination;
            immuneToDamage = false;
        }
        base.Move();
    }


    public void setNewDestination()
    {
        Vector3 currentPos = gameObject.transform.position;
        int flipped = isLeft ? 1 : -1;

        Vector3 newDest = currentPos + (new Vector3(flipped * -currentPos.y, flipped * currentPos.x, 0).normalized * splitDistance);
        
        setDestination(newDest);
        enableSway = false;
        moveSpeed = WaveSpawningSystem.globalWaveSpeed * 5f;
    }
}
