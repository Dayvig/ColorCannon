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

    public bool isDarkened;
    public List<Bullet> immuneBullets = new List<Bullet>();
    public bool knockBack = false;
    public float knockBackTimer;
    public float knockBackDuration = 1f;
    public WaveSpawningSystem.WaveObject.Type enemyType;
    private bool hitTaken = false;

    public AudioSource enemySource;
    public float lifeTime = 0.0f;
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
        enemyColors.Clear();
        
        currentPos = transform.position;
        destination = des;
        moveSpeed = WaveSpawningSystem.globalWaveSpeed;
        initializeMixedColor(color);
        isMultiColor = enemyColors.Count > 1;
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
        knockBack = false;
        knockBackTimer = 0f;

        lifeTime = 0.0f;
    }

    private void Start()
    {
        enemySource = GetComponent<AudioSource>();
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
            enemyColors.Add(color);
        }
    }

    void setDestination(Vector3 dest)
    {
        destination = dest;
    }

    // Update is called once per frame
    public void EnemyUpdate()
    {
        lifeTime += Time.deltaTime;
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
    
    public virtual void SetVisualColor(GameModel.GameColor thisColor)
    {
        Color c = GameModel.instance.ColorToColor(thisColor);
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
            Debug.Assert(col.GetComponent<Player>() != null, "Player doesn't have correct component");
            Player player = col.GetComponent<Player>();
            player.TakeHit();
            Die(false);
        }
    }

    public void touchBullet(Bullet bullet)
    {
        if (bullet.immuneEnemies.Count > 0 && bullet.immuneEnemies.Contains(this))
        {
            return;
        }
        if (enemyColors.Contains(bullet.bulletColor) || enemyColor == bullet.bulletColor)
        {
            TakeHit(bullet.bulletColor);
            bullet.TakeHit();
            bullet.immuneEnemies.Add(this);
        }
    }

    public virtual void TakeHit(GameModel.GameColor bulletColor)
    {
        if (isDarkened)
        {
            isDarkened = false;
            SetVisualColor(enemyColor);
            StartKnockBack(true);
            return;
        }
        enemyColors.Remove(bulletColor);
        enemyColor = SetMixedColor(enemyColors);
        SetVisualColor(enemyColor);
            if (enemyColors.Count == 0)
            {
                Die(true);
            }
            else { 
            StartKnockBack(true);
            }
    }

       public virtual void StartKnockBack(bool withSound)
    {
        SetVisualColor(enemyColor);
        knockBack = true;
        knockBackTimer = 0.0f;
        if (withSound)
        {
            SoundManager.instance.PlaySound(GameManager.instance.gameAudio, GameModel.instance.enemySounds[1]);
        }
    }

    public void Die(bool withSound)
    {
        if (withSound)
        {
            SoundManager.instance.PlaySound(GameManager.instance.gameAudio, GameModel.instance.enemySounds[0]);
        }
        GameManager.instance.markedForDeathEnemies.Add(this);
    }
}
