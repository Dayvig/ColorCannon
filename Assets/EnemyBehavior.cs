using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{

    public Vector3 destination;
    public Vector3 currentPos;
    public float moveSpeed;
    private float SPEED = 0.6f;

    public GameModel.GameColor enemyColor;
    public SpriteRenderer rend;
    
    public virtual void initialize(Vector3 des, GameModel.GameColor color)
    {
        currentPos = transform.position;
        destination = des;
        moveSpeed = SPEED;
        enemyColor = color;
    }

    void setDestination(Vector3 dest)
    {
        destination = dest;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Move();
    }

    public virtual void Move()
    {
        transform.position = Vector3.MoveTowards(currentPos, destination, Time.deltaTime * moveSpeed);
        currentPos = transform.position;
    }
    
    public void SetColor(Color c)
    {
        rend.color = c;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            Debug.Log("Enemy hit player");
            Destroy(this.gameObject);
        }
    }
}
