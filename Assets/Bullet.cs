using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float xSpeed;
    public float ySpeed;
    public Vector3 flight;
    private float xBounds = 3;
    private float yBounds = 5;
    private Vector3 positionTarget;
    private Vector3 positionCurrent;
    public SpriteRenderer ren;

    public GameModel.GameColor bulletColor;
    public GameModel modelGame;
    private GameManager gameManager;
    private int piercing = 1;
    private CircleCollider2D thisCollider;

    // Start is called before the first frame update
    void Start()
    {
        modelGame = GameObject.Find("GameManager").GetComponent<GameModel>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        thisCollider = GetComponent<CircleCollider2D>();
    }

    public void initialize(Vector3 initialPos, float rotation, float speed, GameModel.GameColor color, int pierce)
    {
        rotation = -(rotation * (Mathf.PI / 180));
        
        xSpeed = Mathf.Sin(rotation);
        ySpeed = Mathf.Cos(rotation);
        flight = new Vector3(xSpeed, ySpeed, 0).normalized * speed;

        transform.position = initialPos;
        positionCurrent = initialPos;
        positionTarget = initialPos;
        bulletColor = color;
        piercing = pierce;
        gameObject.SetActive(true);
    }

    public void SetColor(Color c)
    {
        ren.color = c;
    }

    // Update is called once per frame
    public void BulletUpdate()
    {
        positionTarget += flight;
        positionCurrent = Vector3.Lerp(
            positionCurrent, 
            positionTarget, 
            Time.deltaTime * 30.0f);
        transform.position = positionCurrent;
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(positionTarget, thisCollider.radius);
        foreach (Collider2D c in hitColliders)
        {
            TriggerCollisions(c);
        }
        if (positionCurrent.x > xBounds || positionCurrent.x < -xBounds ||
            positionCurrent.y > yBounds || positionCurrent.y < -yBounds)
        { 
            Die();
        }
    }
    
    private void TriggerCollisions(Collider2D col)
    {
        if (col.gameObject.CompareTag("Enemy"))
        {
            Debug.Assert(col.gameObject.GetComponent<EnemyBehavior>() != null);
            EnemyBehavior enemy = col.gameObject.GetComponent<EnemyBehavior>();
            enemy.touchBullet(this);
        }
    }

    public void TakeHit()
    {
        piercing--;
        if (piercing <= 0)
        {
            Die();
        }
    }
    
    public void Die()
    {
        gameManager.markedForDeathBullets.Add(this);
    }
    
    
}
