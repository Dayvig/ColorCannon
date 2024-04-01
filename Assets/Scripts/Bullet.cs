using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEditor.Experimental;
using UnityEngine;


public class Bullet : MonoBehaviour
{
    private float xSpeed;
    private float ySpeed;
    public float overallSpeed;
    public Vector3 flight;
    public Vector3 positionTarget;
    private Vector3 positionCurrent;
    public SpriteRenderer ren;
    public UnityEngine.Rendering.Universal.Light2D lightRen;
    public float baseScale = 0.2f;

    public GameModel.GameColor bulletColor;
    public GameModel modelGame;
    private GameManager gameManager;
    private int piercing = 1;
    public CircleCollider2D thisCollider;
    public List<EnemyBehavior> immuneEnemies;
    private float bulletScale;
    private float baseOuterRadius = 4.2f;

    public bool isSeeking;
    private float seekingFactor = 0.05f;
    public GameObject seekingTarget = null;
    private float lifeTime = 0.0f;
    EnemyBehavior targetBehavior;

    // Start is called before the first frame update
    void Awake()
    {
        modelGame = GameObject.Find("GameManager").GetComponent<GameModel>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    public void initialize(Vector3 initialPos, float rotation, float speed, GameModel.GameColor color, int pierce, float scale, bool seeking)
    {
        rotation = -(rotation * (Mathf.PI / 180));
        if (seeking)
        {
            speed *= 0.8f;
        }
        Quaternion rot = gameObject.transform.rotation;
        float angle = -rotation * Mathf.Rad2Deg;
        gameObject.transform.rotation = Quaternion.Euler(new Vector3(rot.x, rot.y, angle));

        xSpeed = Mathf.Sin(rotation);
        ySpeed = Mathf.Cos(rotation);

        flight = new Vector3(xSpeed, ySpeed, 0).normalized * speed;
        overallSpeed = speed;

        transform.position = initialPos;
        positionCurrent = initialPos;
        positionTarget = initialPos;
        bulletColor = color;
        piercing = pierce;
        gameObject.SetActive(true);
        thisCollider.enabled = true;
        bulletScale = scale;
        this.transform.localScale = new Vector3 (scale, scale, 1);
        isSeeking = seeking;
        lifeTime = 0.0f;
        if (isSeeking)
        {
            AcquireTarget();
            ren.sprite = GameModel.instance.bulletImages[1];
        }
        else
        {
            ren.sprite = GameModel.instance.bulletImages[0];
        }


        immuneEnemies.Clear();
    }

    public void SetColor(Color c)
    {
        ren.color = c;
        lightRen.color = c;
        lightRen.pointLightOuterRadius = (baseOuterRadius * bulletScale);
    }

    // Update is called once per frame
    public void BulletUpdate()
    {
        lifeTime += Time.deltaTime;

        if (isSeeking && (seekingTarget == null || !seekingTarget.activeInHierarchy))
        {
            lifeTime += Time.deltaTime;
            if (lifeTime > 0.5f)
            {
                AcquireTarget();
            }
        }
        if (isSeeking && seekingTarget != null)
        {
            positionTarget = Vector3.MoveTowards(positionTarget, seekingTarget.transform.position, overallSpeed);
            flight = -(positionCurrent - positionTarget).normalized * overallSpeed;
            Quaternion rot = gameObject.transform.rotation;
            Transform t = seekingTarget.transform;
            float angle = ((180 / Mathf.PI) * Mathf.Atan2(t.position.y - positionCurrent.y,
                t.position.x - positionCurrent.x)) - 90;
            gameObject.transform.rotation = Quaternion.Euler(new Vector3(rot.x, rot.y, angle));
            if (!targetBehavior.enemyColors.Contains(bulletColor))
            {
                seekingTarget = null;
            }
        }
        else
        {
            positionTarget += flight;
        }
        positionCurrent = Vector3.Lerp(
            positionCurrent, 
            positionTarget, 
            Time.deltaTime * 30.0f);
        if (thisCollider.enabled)
        {
            CheckCollisions();
        }
        
    }

    public void AcquireTarget()
    {
        seekingTarget = null;
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(this.transform.position, 1.5f);
        float shortestDist = -1;
        foreach (Collider2D c in hitColliders)
        {
            if (c.gameObject.CompareTag("Enemy") && c.gameObject.activeInHierarchy)
            {
                EnemyBehavior e = c.GetComponent<EnemyBehavior>();
                if (e.enemyColors.Contains(bulletColor))
                {
                    float dist = Vector3.Distance(c.gameObject.transform.position, this.transform.position);
                    if (dist < shortestDist || shortestDist == -1)
                    {
                        shortestDist = dist;
                        seekingTarget = c.gameObject;
                    }
                }
            }
        }
        lifeTime = 0.0f;
        if (seekingTarget != null)
        {
            targetBehavior = seekingTarget.GetComponent<EnemyBehavior>();
        }
    }

    void CheckCollisions()
    {
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
        if (positionCurrent.x > GameModel.instance.xBounds || positionCurrent.x < -GameModel.instance.xBounds ||
            positionCurrent.y > GameModel.instance.yBounds || positionCurrent.y < -GameModel.instance.yBounds)
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
        thisCollider.enabled = false;
    }
    
}
