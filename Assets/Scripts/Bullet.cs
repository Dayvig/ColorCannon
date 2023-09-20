using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
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
    public float baseScale = 0.2f;

    public GameModel.GameColor bulletColor;
    public GameModel modelGame;
    private GameManager gameManager;
    private int piercing = 1;
    public CircleCollider2D thisCollider;
    public List<EnemyBehavior> immuneEnemies;
    private float bulletScale;

    // Start is called before the first frame update
    void Start()
    {
        modelGame = GameObject.Find("GameManager").GetComponent<GameModel>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    public void initialize(Vector3 initialPos, float rotation, float speed, GameModel.GameColor color, int pierce, float scale)
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
        bulletScale = scale;
        this.transform.localScale = new Vector3 (scale, scale, 1);

        immuneEnemies.Clear();
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
        RaycastHit2D[] hits = Physics2D.LinecastAll(transform.position, positionCurrent, 7);
        if (hits.Length != 0)
        {
            foreach (RaycastHit2D h in hits)
            {
                if (h.transform.gameObject.CompareTag("Enemy"))
                {
                    Collide(h.collider);
                    break;
                }
            }
        }
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(positionTarget, thisCollider.radius * bulletScale);
        foreach (Collider2D c in hitColliders)
        {
            Collide(c);
        }
        transform.position = positionCurrent;
        if (positionCurrent.x > xBounds || positionCurrent.x < -xBounds ||
            positionCurrent.y > yBounds || positionCurrent.y < -yBounds)
        { 
            Die();
        }
    }

    private void Collide(Collider2D col)
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