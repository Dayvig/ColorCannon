using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class Player : MonoBehaviour, IDataPersistence
{
    private float rotationCurrent = 0;
    private float rotationTarget = 0;
    private Vector3 actualRotation;
    [Range(1f, 0.1f)]
    public float baseShotSpeed = 0.5f;
    [Range(0.001f, 1f)]
    public float baseBulletSpeed = 0.01f;
    [Range(0.01f, 0.1f)]
    public float baseBulletSize = 0.1f;
    [Range(1, 10)] 
    public int basePiercing = 1;
    [Range(1, 5)] 
    public int baseNumShots;
    [Range(1, 10)] 
    public int baseLives;
    
    public float shotSpeed;
    public float bulletSpeed;
    public float bulletSize;
    public int numShots;
    public int piercing;
    public int lives;
    
    private float shotTimer;
    public GameObject bullet;

    public GameObject enemy;
    
    public SpriteRenderer barrel;
    public SpriteRenderer[] lifeShields;

    public GameModel.GameColor playerColor;
    public GameModel modelGame;
    public GameManager gameManager;

    public float mouseTimer = 0.0f;
    public float dClickInterval;
    public int clicks = 0;

    private GameModel.GameColor[] colorOrder =
        {GameModel.GameColor.RED, GameModel.GameColor.BLUE, GameModel.GameColor.YELLOW};

    private int colorPlace = 0;

    public List<GameManager.Upgrade> upgrades = new List<GameManager.Upgrade>();

    public AudioSource playerAudio;

    private enum controlMode
    {
        TOUCH,
        MOUSE
    }

    private controlMode playerControlMode = controlMode.MOUSE;
    
    void Start()
    {
        modelGame = GameObject.Find("GameManager").GetComponent<GameModel>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        playerColor = colorOrder[colorPlace];
        barrel.color = modelGame.ColorToColor(playerColor);
        lives = baseLives;
        configureWeapon();
        playerAudio = GetComponent<AudioSource>();
    }
    
    // Update is called once per frame
    public void PlayerUpdate()
    {
        DoubleClickUpdate();
        LifeUpdate();
        Vector3 currentPos = gameObject.transform.position;
        Quaternion rot = gameObject.transform.rotation;
        if (Input.GetMouseButton(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            {
                float angle = ((180 / Mathf.PI) * Mathf.Atan2(mousePos.y -currentPos.y,
                                          mousePos.x - currentPos.x)) - 90;
                rotationTarget = angle;
            }
        }
        if (Input.touchCount == 1)
        {
            Vector3 touchPos = Camera.main.ScreenToWorldPoint(Input.touches[0].position);
            {
                float angle = ((180 / Mathf.PI) * Mathf.Atan2(touchPos.y -currentPos.y,
                    touchPos.x - currentPos.x)) - 90;
                rotationTarget = angle;
            }
        }
        /*
        actualRotation = Vector3.Lerp(
            new Vector3(0, 0, rotationCurrent),
            new Vector3(0, 0, rotationTarget),
            Time.deltaTime * 30.0f);
        */
        //gameObject.transform.rotation = Quaternion.Euler(actualRotation);
        
        gameObject.transform.rotation = Quaternion.Euler(new Vector3(rot.x, rot.y, rotationTarget));
        
        shotTimer += Time.deltaTime;
        if (shotTimer > shotSpeed)
        {
            SpreadFire(playerColor, numShots);
            shotTimer = 0.0f;
        }
    }

    private void LifeUpdate()
    {
        for (int i = lifeShields.Length-1; i >= 0; i--)
        {
            lifeShields[i].enabled = (lives >= i+1);
        }
    }

    private void SpreadFire(GameModel.GameColor gameColor, int numShots)
    {
        if (numShots == 1)
        {
            GameObject newBulletObject = null;
            foreach (Bullet b in gameManager.inactiveBullets)
            {
                newBulletObject = b.gameObject;
                break;
            }
            if (newBulletObject == null)
            {
                newBulletObject = Instantiate(bullet, transform.position, Quaternion.identity);
            }
            
            //create and instantiate new bullet
            Bullet bulletScript = newBulletObject.GetComponent<Bullet>();
            bulletScript.initialize(transform.position, rotationTarget, bulletSpeed, playerColor, piercing, bulletSize);
            bulletScript.SetColor(modelGame.ColorToColor(bulletScript.bulletColor));
            if (!gameManager.activeBullets.Contains(bulletScript))
            {
                gameManager.activeBullets.Add(bulletScript);
            }
            gameManager.inactiveBullets.Remove(bulletScript);

            //Play firing sound
            SoundManager.instance.PlaySound(playerAudio, GameModel.instance.bulletSounds[0]);
        }
        else
        {
            //Sets initial to 0 if odd and 1 if even
            int initial = 1 - (numShots % 2);
            for (int s = initial; s < numShots + initial; s++)
            {
                float angleOffSet =
                    ((((float) s / (numShots + (1 - (2 * (numShots % 2))))) * modelGame.spreadAngleMax) -
                     (modelGame.spreadAngleMax / 2));
                GameObject newBulletObject = null;
                foreach (Bullet b in gameManager.inactiveBullets)
                {
                    //some conditional later
                    newBulletObject = b.gameObject;
                    break;
                }
                if (newBulletObject == null)
                {
                    newBulletObject = Instantiate(bullet, transform.position, Quaternion.identity);
                }
                Bullet bulletScript = newBulletObject.GetComponent<Bullet>();
                bulletScript.initialize(transform.position, rotationTarget + angleOffSet, bulletSpeed, playerColor, piercing, bulletSize);
                bulletScript.SetColor(modelGame.ColorToColor(bulletScript.bulletColor));
                if (!gameManager.activeBullets.Contains(bulletScript))
                {
                    gameManager.activeBullets.Add(bulletScript);
                }
                gameManager.inactiveBullets.Remove(bulletScript);
            }

            //Play firing sounds
            SoundManager.instance.PlaySound(playerAudio, GameModel.instance.bulletSounds[0]);
            for (int k = 1; k < numShots; k++)
            {
                SoundManager.instance.PlaySound(playerAudio, GameModel.instance.bulletSounds[0], 0.03f * k);
            }

        }
    }
    
    
    private void nextColor()
    {
        colorPlace++;
        if (colorPlace >= colorOrder.Length)
        {
            colorPlace = 0;
        }
        playerColor = colorOrder[colorPlace];
        barrel.color = modelGame.ColorToColor(playerColor);
        configureWeapon();
    }

    public void configureWeapon()
    {
        bulletSpeed = FinalShotSpeed(playerColor);
        bulletSize = FinalShotSize(playerColor);
        shotSpeed = FinalRateOfFire(playerColor);
        numShots = FinalNumShots(playerColor);
        piercing = FinalPiercing(playerColor);
    }

    public float FinalShotSpeed(GameModel.GameColor currentColor)
    {
        bulletSpeed = baseBulletSpeed;
        foreach (GameManager.Upgrade u in upgrades)
        {
            if (u.color.Equals(currentColor) && u.type.Equals(GameManager.UpgradeType.SHOTSPEED))
            {
                bulletSpeed *= modelGame.shotSpeedMultiplier;
            }
        }
        return bulletSpeed;
    }
    public float FinalShotSize(GameModel.GameColor currentColor)
    {
        bulletSize = baseBulletSize;
        foreach (GameManager.Upgrade u in upgrades)
        {
            if (u.color.Equals(currentColor) && u.type.Equals(GameManager.UpgradeType.SHOTSIZE))
            {
                bulletSize *= modelGame.shotSizeMultiplier;
            }
        }
        return bulletSize;
    }
    public float FinalRateOfFire(GameModel.GameColor currentColor)
    {
        shotSpeed = baseShotSpeed;
        foreach (GameManager.Upgrade u in upgrades)
        {
            if (u.color.Equals(currentColor) && u.type.Equals(GameManager.UpgradeType.ATTACKSPEED))
            {
                shotSpeed /= modelGame.shotSpeedMultiplier;
            }
        }
        return shotSpeed;
    }
    
    public int FinalNumShots(GameModel.GameColor currentColor)
    {
        numShots = baseNumShots;
        foreach (GameManager.Upgrade u in upgrades)
        {
            if (u.color.Equals(currentColor) && u.type.Equals(GameManager.UpgradeType.SHOTS))
            {
                numShots += modelGame.numShotsUpgrade;
            }
        }
        return numShots;
    }
    
    public int FinalPiercing(GameModel.GameColor currentColor)
    {
        piercing = basePiercing;
        foreach (GameManager.Upgrade u in upgrades)
        {
            if (u.color.Equals(currentColor) && u.type.Equals(GameManager.UpgradeType.PIERCING))
            {
                piercing += modelGame.piercingUpgrade;
            }
        }
        return piercing;
    }
    
    Vector3 tapPos = new Vector3(0f, 0f, 0f);
    void DoubleClickUpdate()
    {
        if (mouseTimer >= 0.0f)
        {
            mouseTimer += Time.deltaTime;
        }
        if (mouseTimer > dClickInterval)
        {
            mouseTimer = -1.0f;
            clicks = 0;
        }
        if (playerControlMode == controlMode.TOUCH) {
            takeTouchInput();
        }
        else
        {
            takeMouseInput();
        }
    }

    void takeTouchInput()
    {
        //Begin touch
        //Track the touchposition
        //if one tap has already been executed, switch color
        if (Input.touchCount == 1 && Input.touches[0].phase == TouchPhase.Began)
        {
            tapPos = Camera.main.ScreenToWorldPoint(Input.touches[0].position);
            if (clicks > 0)
            {
                nextColor();
            }
        }
        //End Touch
        //If the position is close to where it began, increment clicks
        if (Input.touchCount == 1 && Input.touches[0].phase == TouchPhase.Ended)
        {
            Vector3 newPos = Camera.main.ScreenToWorldPoint(Input.touches[0].position);
            if (Vector3.Distance(tapPos, newPos) < 0.1f)
            {
                clicks++;
                mouseTimer = 0f;
            }
            else
            {
                mouseTimer = -1.0f;
                clicks = 0;
            }
        }
    }
    void takeMouseInput()
    {
        //Begin click
        //Track the clickposition
        //if one click has already been executed, switch color
        if (Input.GetMouseButtonDown(0))
        {
            tapPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (clicks > 0)
            {
                nextColor();
            }
        }
        //End click
        //If the position is close to where it began, increment clicks
        if (Input.GetMouseButtonUp(0))
        {
            Vector3 newPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (Vector3.Distance(tapPos, newPos) < 0.1f)
            {
                clicks++;
                mouseTimer = 0f;
            }
            else
            {
                mouseTimer = -1.0f;
                clicks = 0;
            }
        }
    }

    public void LoadData(GameData data)
    {
        upgrades = data.playerUpgrades;
        foreach (GameManager.Upgrade newUpgrade in upgrades)
        {
            Debug.Log("Adding new upgrade to preview");
            UIManager.instance.AddNewPlayerUpgradeToPreview(newUpgrade);
        }
    }

    public void SaveData(ref GameData data)
    {
        data.playerUpgrades = upgrades;
    }
}
