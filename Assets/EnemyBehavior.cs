using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{

    public Vector3 destination;
    public Vector3 currentPos;
    public float moveSpeed;
    public float knockbackSpeed = 2f;
    private float SPEED = 0.6f;

    public GameModel.GameColor enemyColor;
    public List<GameModel.GameColor> enemyColors = new List<GameModel.GameColor>();
    private bool isMultiColor;
    public SpriteRenderer rend;
    private GameManager gameManager;
    public GameModel modelGame;

    public bool isDarkened;
    public List<Bullet> immuneBullets = new List<Bullet>();
    public bool knockBack = false;
    private float knockBackTimer;
    private float knockBackDuration = 1f;
    public WaveSpawningSystem.WaveObject.Type enemyType;

    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        modelGame = GameObject.Find("GameManager").GetComponent<GameModel>();
    }

    private GameModel.GameColor SetMixedColor(List<GameModel.GameColor> colors)
    {
        if (colors.Count == 0)
        {
            Debug.Log("empty color array");
            return GameModel.GameColor.RED;
        }
        if (colors.Count == 1)
        {
            return colors[0];
        }
        if (colors.Count == 2)
        {
            if (colors.Contains(GameModel.GameColor.RED) && colors.Contains(GameModel.GameColor.BLUE))
            {
                return GameModel.GameColor.PURPLE;
            }
            if (colors.Contains(GameModel.GameColor.RED) && colors.Contains(GameModel.GameColor.YELLOW))
            {
                return GameModel.GameColor.ORANGE;
            }
            if (colors.Contains(GameModel.GameColor.BLUE) && colors.Contains(GameModel.GameColor.YELLOW))
            {
                return GameModel.GameColor.GREEN;
            }
        }
        if (colors.Count == 3)
        {
            return GameModel.GameColor.WHITE;
        }
        Debug.Log("Something went wrong");
        return GameModel.GameColor.RED;
    }
    
    public virtual void initialize(Vector3 des, GameModel.GameColor color, bool dark, WaveSpawningSystem.WaveObject.Type type)
    {
        currentPos = transform.position;
        destination = des;
        moveSpeed = SPEED;
        initializeMixedColor(color);
        if (isMultiColor)
        {
            enemyColor = SetMixedColor(enemyColors);
        }
        else
        {
            enemyColor = color;
        }

        isDarkened = dark;
        enemyType = type;
        gameObject.SetActive(true);
        immuneBullets.Clear();
    }

    private void initializeMixedColor(GameModel.GameColor color)
    {
        if (color.Equals(GameModel.GameColor.ORANGE))
        {
            enemyColors.Add(GameModel.GameColor.RED);
            enemyColors.Add(GameModel.GameColor.YELLOW);
            isMultiColor = true;
        }
        else if (color.Equals(GameModel.GameColor.PURPLE))
        {
            enemyColors.Add(GameModel.GameColor.RED);
            enemyColors.Add(GameModel.GameColor.BLUE);
            isMultiColor = true;
        }
        else if (color.Equals(GameModel.GameColor.GREEN))
        {
            enemyColors.Add(GameModel.GameColor.BLUE);
            enemyColors.Add(GameModel.GameColor.YELLOW);
            isMultiColor = true;
        }
        else if (color.Equals(GameModel.GameColor.WHITE))
        {
            enemyColors.Add(GameModel.GameColor.RED);
            enemyColors.Add(GameModel.GameColor.YELLOW);
            enemyColors.Add(GameModel.GameColor.BLUE);
            isMultiColor = true;
        }
        else
        {
            isMultiColor = false;
        }
    }

    void setDestination(Vector3 dest)
    {
        destination = dest;
    }

    // Update is called once per frame
    public void EnemyUpdate()
    {
        if (knockBack)
        {
            KnockBack();
        }
        else
        {
            Move();
        }
    }

    public virtual void Move()
    {
        transform.position = Vector3.MoveTowards(currentPos, destination, Time.deltaTime * moveSpeed);
        currentPos = transform.position;
    }
    
    public virtual void KnockBack()
    {
        knockBackTimer += Time.deltaTime;
        transform.position = Vector3.MoveTowards(currentPos, destination, -Time.deltaTime * Mathf.Lerp(knockbackSpeed, 0, knockBackTimer/knockBackDuration));
        currentPos = transform.position;
        if (knockBackTimer > knockBackDuration)
        {
            knockBack = false;
        }
    }
    
    public void SetColor(Color c)
    {
        if (!isDarkened)
        {
            rend.color = c;
        }
        else
        {
            rend.color = new Color(c.r / 1.5f, c.g / 1.5f, c.b / 1.5f, c.a);
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            Debug.Log("Enemy hit player");
            Die();
        }
    }

    public void touchBullet(Bullet bullet)
    {
        if (immuneBullets.Count > 0 && immuneBullets.Contains(bullet))
        {
            return;
        }
        if (!isMultiColor)
        {
            if (bullet.bulletColor == enemyColor)
            {
                TakeHit();
                immuneBullets.Add(bullet);
                bullet.TakeHit();
            }
        }
        else
        {
            if (enemyColors.Contains(bullet.bulletColor))
            {
                enemyColors.Remove(bullet.bulletColor);
                if (enemyColors.Count == 0)
                {
                    Die();
                }
                enemyColor = SetMixedColor(enemyColors);
                SetColor(modelGame.ColorToColor(enemyColor));
                knockBack = true;
                bullet.TakeHit();
            }
        }
    }

    public void TakeHit()
    {
        if (isDarkened)
        {
            isDarkened = false;
            SetColor(modelGame.ColorToColor(enemyColor));
            knockBack = true;
        }
        else
        {
            Die();
        }
    }

    public void Die()
    {
        gameManager.markedForDeathEnemies.Add(this);
    }
}
