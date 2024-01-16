using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GibletScript : DeathEffect
{
    public Vector3 destination;
    public Vector3 playerPos;
    public float gibletSpeed1;
    public float gibletSpeed2;
    // Start is called before the first frame update

    // Update is called once per frame
    public override void initialize()
    {
        this.gameObject.SetActive(true);
        ren.sprite = GameModel.instance.giblets[Random.Range(0, 4)];
        this.transform.rotation = Random.rotation;
        Vector3 eulerRotation = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(0, 0, eulerRotation.z);
        destination = this.transform.position + RandomPointOnCircleEdge(0.6f);
        playerPos = GameManager.instance.player.transform.position;
    }

    private Vector3 RandomPointOnCircleEdge(float radius)
    {
        var vector2 = Random.insideUnitCircle.normalized * radius;
        return new Vector3(vector2.x, vector2.y, 0);
    }

    public override void Update()
    {
        Vector3 currentPos = transform.position;
        if (destination != playerPos)
        {
            transform.position = Vector3.MoveTowards(currentPos, destination, Time.deltaTime * gibletSpeed1);
            if (Vector3.Distance(currentPos, destination) < 0.1f)
            {
                destination = playerPos;
            }
        }
        else
        {
            transform.position = Vector3.MoveTowards(currentPos, destination, Time.deltaTime * gibletSpeed2);
            if (Vector3.Distance(currentPos, destination) < 0.1f)
            {
                GameManager.instance.markedForDeathGiblets.Add(this);
            }
        }
    }

}
