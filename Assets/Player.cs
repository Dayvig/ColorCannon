using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class Player : MonoBehaviour
{
    private float rotationCurrent = 0;
    private float rotationTarget = 0;
    private Vector3 actualRotation;
    [Range(1f, 0.1f)]
    public float baseShotSpeed = 0.5f;
    [Range(0.005f, 0.1f)]
    public float baseBulletSpeed = 0.01f;
    [Range(0.5f, 2f)]
    public float baseBulletSize = 1f;

    [Range(1, 5)] public int baseNumShots;
    public float shotSpeed;
    public float bulletSpeed;
    public float bulletSize;
    public int numShots;
    
    private float shotTimer;
    public GameObject bullet;

    public GameObject enemy;
    
    public SpriteRenderer barrel;

    public GameModel.GameColor playerColor;
    public GameModel modelGame;
    public GameManager gameManager;

    public float mouseTimer = 0.0f;
    private float dClickInterval = 0.3f;
    public int clicks = 0;

    private GameModel.GameColor[] colorOrder =
        {GameModel.GameColor.RED, GameModel.GameColor.BLUE, GameModel.GameColor.YELLOW};

    private int colorPlace = 0;

    public List<GameManager.Upgrade> upgrades = new List<GameManager.Upgrade>();
    
    void Start()
    {
        modelGame = GameObject.Find("GameManager").GetComponent<GameModel>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        playerColor = colorOrder[colorPlace];
        barrel.color = modelGame.ColorToColor(playerColor);
        configureWeapon();
    }
    
    // Update is called once per frame
    public void PlayerUpdate()
    {
        DoubleClickUpdate();
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

    private void SpreadFire(GameModel.GameColor gameColor, int numShots)
    {
        if (numShots == 1)
        {
            GameObject newBulletObject = Instantiate(bullet, transform.position, Quaternion.identity);
            newBulletObject.transform.localScale = newBulletObject.transform.localScale * bulletSize;
            Bullet bulletScript = newBulletObject.GetComponent<Bullet>();
            bulletScript.initialize(rotationTarget, bulletSpeed, playerColor);
            bulletScript.SetColor(modelGame.ColorToColor(bulletScript.bulletColor));
            gameManager.activeBullets.Add(bulletScript);
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
                GameObject newBulletObject = Instantiate(bullet, transform.position, Quaternion.identity);
                newBulletObject.transform.localScale = newBulletObject.transform.localScale * bulletSize;
                Bullet bulletScript = newBulletObject.GetComponent<Bullet>();
                bulletScript.initialize(rotationTarget + angleOffSet, bulletSpeed, playerColor);
                bulletScript.SetColor(modelGame.ColorToColor(bulletScript.bulletColor));
                gameManager.activeBullets.Add(bulletScript);
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
        if (Input.GetMouseButtonUp(0))
        {
            clicks++;
            mouseTimer = 0.0f;
            if (clicks > 1){
                if (mouseTimer != -1.0f)
                {
                    nextColor();
                }
            }
        }
    }
}
