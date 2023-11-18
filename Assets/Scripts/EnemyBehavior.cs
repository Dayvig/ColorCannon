using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
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
    private float swayTimer = 0.0f;
    public float swayInterval = 0.15f;
    public float swayAngle = 10;
    private int mode = 0;
    public bool enableSway = true;

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

    public void addColor(GameModel.GameColor color, bool isNewDarkColor)
    {
        if (!isDarkened && isNewDarkColor)
        {
            isDarkened = true;
            SetVisualColor(enemyColor);
        }
        if (enemyColors.Contains(color))
        {
            return;
        }
        else
        {
            enemyColors.Add(color);
            enemyColor = SetMixedColor(enemyColors);
            isMultiColor = true;
            SetVisualColor(enemyColor);
        }
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
        if (enableSway)
        {
            SwayMovement();
        }
        else
        {
            BasicMovement();
        }
    }

    void SwayMovement()
    {
        swayTimer += Time.deltaTime;
        if (swayTimer < swayInterval)
        {
            if (mode == 0)
            {
                Vector3 newZ = Vector3.Lerp(new Vector3(transform.localRotation.x, transform.localRotation.y, 0), new Vector3(transform.localRotation.x, transform.localRotation.y, swayAngle), swayTimer / swayInterval);
                transform.localRotation = Quaternion.Euler(newZ);
                transform.position = Vector3.MoveTowards(currentPos, destination, Time.deltaTime * moveSpeed * 2);
                currentPos = transform.position;
            }
            else if (mode == 1)
            {
                Vector3 newZ = Vector3.Lerp(new Vector3(transform.localRotation.x, transform.localRotation.y, swayAngle), new Vector3(transform.localRotation.x, transform.localRotation.y, 0), swayTimer / swayInterval);
                transform.localRotation = Quaternion.Euler(newZ);
            }
            else if (mode == 2)
            {
                Vector3 newZ = Vector3.Lerp(new Vector3(transform.localRotation.x, transform.localRotation.y, 0), new Vector3(transform.localRotation.x, transform.localRotation.y, -swayAngle), swayTimer / swayInterval);
                transform.localRotation = Quaternion.Euler(newZ);
                transform.position = Vector3.MoveTowards(currentPos, destination, Time.deltaTime * moveSpeed * 2);
                currentPos = transform.position;
            }
            else
            {
                Vector3 newZ = Vector3.Lerp(new Vector3(transform.localRotation.x, transform.localRotation.y, -swayAngle), new Vector3(transform.localRotation.x, transform.localRotation.y, 0), swayTimer / swayInterval);
                transform.localRotation = Quaternion.Euler(newZ);
            }
        }
        else
        {
            if (mode == 0 || mode == 2)
            {
                swayTimer = -swayInterval;
            }
            else
            {
                swayTimer = 0.0f;
            }
            mode++;
            if (mode > 3)
            {
                mode = 0;
            }
        }
    }

    void BasicMovement()
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
        UnityEngine.Color c = GameModel.instance.ColorToColor(thisColor);
        if (!isDarkened)
        {
            rend.color = c;
        }
        else
        {
            rend.color = new UnityEngine.Color(c.r / 1.5f, c.g / 1.5f, c.b / 1.5f, c.a);
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

    public virtual void touchBullet(Bullet bullet)
    {
        if (bullet.immuneEnemies.Count > 0 && bullet.immuneEnemies.Contains(this))
        {
            return;
        }
        if (enemyColors.Contains(bullet.bulletColor) || enemyColor == bullet.bulletColor || checkForMultiColoredBullet(bullet.bulletColor))
        {
            TakeHit(bullet.bulletColor);
            bullet.TakeHit();
            bullet.immuneEnemies.Add(this);
        }
    }

    public virtual void TakeHit(GameModel.GameColor bulletColor)
    {
        GameManager.instance.createSplatter(this.transform.position, GameModel.instance.ColorToColor(enemyColor));
        if (isDarkened)
        {
            isDarkened = false;
            SetVisualColor(enemyColor);
            StartKnockBack(true);
            return;
        }

        if (!removeMultiColoredBullet(bulletColor)) {
            enemyColors.Remove(bulletColor);
        }
        enemyColor = SetMixedColor(enemyColors);
        SetVisualColor(enemyColor);
            if (enemyColors.Count == 0)
            {
                Die(true);
            }
            else { 
            StartKnockBack(true);
            }
        GameManager.instance.shotsHit++;
    }

    bool removeMultiColoredBullet(GameModel.GameColor bulletColor)
    {
        if (bulletColor.Equals(GameModel.GameColor.ORANGE))
        {
            enemyColors.Remove(GameModel.GameColor.RED);
            enemyColors.Remove(GameModel.GameColor.YELLOW);
            return true;
        }
        else if (bulletColor.Equals(GameModel.GameColor.PURPLE))
        {
            enemyColors.Remove(GameModel.GameColor.RED);
            enemyColors.Remove(GameModel.GameColor.BLUE);
            return true;
        }
        else if (bulletColor.Equals(GameModel.GameColor.GREEN))
        {
            enemyColors.Remove(GameModel.GameColor.BLUE);
            enemyColors.Remove(GameModel.GameColor.YELLOW);
            return true;
        }
        else if (bulletColor.Equals(GameModel.GameColor.WHITE))
        {
            enemyColors.Remove(GameModel.GameColor.RED);
            enemyColors.Remove(GameModel.GameColor.YELLOW);
            enemyColors.Remove(GameModel.GameColor.BLUE);
            return true;
        }
        return false;
    }

    bool checkForMultiColoredBullet(GameModel.GameColor bulletColor)
    {
        if (bulletColor.Equals(GameModel.GameColor.ORANGE))
        {
            return enemyColors.Contains(GameModel.GameColor.RED) || enemyColors.Contains(GameModel.GameColor.YELLOW);
        }
        else if (bulletColor.Equals(GameModel.GameColor.PURPLE))
        {
            return enemyColors.Contains(GameModel.GameColor.RED) || enemyColors.Contains(GameModel.GameColor.BLUE);
        }
        else if (bulletColor.Equals(GameModel.GameColor.GREEN))
        {
            return enemyColors.Contains(GameModel.GameColor.BLUE) || enemyColors.Contains(GameModel.GameColor.YELLOW);
        }
        else if (bulletColor.Equals(GameModel.GameColor.WHITE))
        {
            return true;
        }
        return false;
    }

    public virtual void StartKnockBack(bool withSound)
    {
        SetVisualColor(enemyColor);
        knockBack = true;
        knockBackTimer = 0.0f;
        if (withSound && GameManager.instance.currentState != GameManager.GameState.MAINMENU)
        {
            SoundManager.instance.PlaySFX(GameManager.instance.gameAudio, GameModel.instance.enemySounds[1]);
        }
    }

    public delegate void EnemyDiedEvent();
    public static event EnemyDiedEvent HasDied;
    public virtual void Die(bool withSound)
    {
        if (withSound && GameManager.instance.currentState != GameManager.GameState.MAINMENU)
        {
            SoundManager.instance.PlaySFX(GameManager.instance.gameAudio, GameModel.instance.enemySounds[0]);
        }
        GameManager.instance.markedForDeathEnemies.Add(this);
        if (HasDied != null)
        {
            HasDied();
        }
    }
}
