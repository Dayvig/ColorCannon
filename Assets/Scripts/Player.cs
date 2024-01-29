using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using UnityEngine;
using UnityEngine.UIElements;
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
    public float shotSpread;
    public int combineChance;
    public bool deathPulse = false;
    public float shieldPulseRadius;
    public float meterMult;
    
    public float shotTimer;
    public float rocketTimer;
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

    public Transform[] orbs = new Transform[3];
    public Transform mainOrb;
    public GameObject shieldPulse;
    public Vector3[] orbPositions = new Vector3[] { new Vector3(-0.13f, 0.482f, 0), new Vector3(-0.355f, 0.263f, 0f), new Vector3(-0.415f, 0.09f, 0f) };
    public Vector3[] orbTargetPos = new Vector3[3];

    private GameModel.GameColor[] colorOrder =
        {GameModel.GameColor.RED, GameModel.GameColor.BLUE, GameModel.GameColor.YELLOW};

    public List<GameModel.GameColor> rocketColors = new List<GameModel.GameColor>();
    public float rocketRoF;

    private int colorPlace = 0;

    public List<GameManager.Upgrade> upgrades = new List<GameManager.Upgrade>();

    public AudioSource playerAudio;

    public float orbAnimTimer = 0.0f;
    public float orbAnimInterval = 1.0f;

    public float rainbowMeter = 0.0f;
    public float rainbowRushTime = 10.0f;
    public float meterMax = 100.0f;
    public rainbowMeter meter;
    public bool rainbowRush = false;
    public float rainbowTimer = 0.0f;
    public int rainbowInkCollected = 0;
    public RainbowInkScript rainbowInkScript;

    public ringScript SelectorRing;
    public bool movementLocked = false;

    public float pulseTimer = 0.0f;
    public float pulseInterval = -1f;
    public bool pulseShield = false;

    public float rainbowRushReminderTimer = 0.0f;
    public float rainbowRushReminderInterval = 10.0f;
    public pulseReminder rainbowReminder;
    public enum controlMode
    {
        TOUCH,
        MOUSE
    }

    public controlMode playerControlMode = controlMode.TOUCH;
    
    void Start()
    {
        modelGame = GameObject.Find("GameManager").GetComponent<GameModel>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        playerColor = colorOrder[colorPlace];
        barrel.color = modelGame.ColorToColor(playerColor);
        lives = baseLives;
        setNextOrb();
        configureWeapon();
        playerAudio = GetComponent<AudioSource>();
        EnemyBehavior.HasDied += this.IncreaseMeter;
    }
    
    // Update is called once per frame
    public void PlayerUpdate()
    {
        DoubleClickUpdate();
        LifeUpdate();
        OrbUpdate();
        RainbowRushUpdate();
        Vector3 currentPos = gameObject.transform.position;
        Quaternion rot = gameObject.transform.rotation;
        if (!SelectorRing.activated)
        {
            LookAtMouse(currentPos);
            if (Input.touchCount == 1)
            {
                Vector3 touchPos = Camera.main.ScreenToWorldPoint(Input.touches[0].position);
                {
                    float angle = ((180 / Mathf.PI) * Mathf.Atan2(touchPos.y - currentPos.y,
                        touchPos.x - currentPos.x)) - 90;
                    rotationTarget = angle;
                }
            }
        }

        gameObject.transform.rotation = Quaternion.Euler(new Vector3(rot.x, rot.y, rotationTarget));
        
        shotTimer += Time.deltaTime;
        if (shotTimer > shotSpeed)
        {
            SpreadFire(playerColor, numShots);
            shotTimer = 0.0f;
        }

        if (rocketColors.Count != 0)
        {
            rocketTimer += Time.deltaTime;
            if (rocketTimer > rocketRoF)
            {
                RocketFire(rocketColors);
                rocketTimer = 0.0f;
            }
        }
        if (pulseShield)
        {
            pulseTimer += Time.deltaTime;
            if (pulseTimer > pulseInterval)
            {
                PulseShield();
                pulseTimer = 0.0f;
            }
        }

        if (rainbowMeter >= meterMax && !rainbowRush && !rainbowReminder.active)
        {
            rainbowRushReminderTimer += Time.deltaTime;
            if (rainbowRushReminderTimer >= rainbowRushReminderInterval)
            {
                rainbowReminder.Initialize();
            }
        }
    }

    float randomRotateTimer = 0.0f;
    float randomRotateInterval = 5f;

    public void PlayerDemoUpdate()
    {
        OrbUpdate();
        Vector3 currentPos = gameObject.transform.position;
        Quaternion rot = gameObject.transform.rotation;
        gameObject.transform.rotation = Quaternion.Euler(new Vector3(rot.x, rot.y, rotationTarget));
        shotTimer += Time.deltaTime;
        if (shotTimer > shotSpeed)
        {
            SpreadFire(playerColor, numShots);
            shotTimer = 0.0f;
        }
        Rotate(40f);
        randomRotateTimer += Time.deltaTime;
        if (randomRotateTimer > randomRotateInterval)
        {
            nextColor();
            randomRotateTimer = 0.0f;
            randomRotateInterval = UnityEngine.Random.Range(1f, 10f);
        }
    }

    private void LookAtMouse(Vector3 current)
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float angle = ((180 / Mathf.PI) * Mathf.Atan2(mousePos.y - current.y, mousePos.x - current.x)) - 90;
        rotationTarget = angle;
    }

    private void Rotate(float speed)
    {
        rotationTarget += (1 * speed * Mathf.Deg2Rad);
    }

    private void LifeUpdate()
    {
        for (int i = lifeShields.Length-1; i >= 0; i--)
        {
            lifeShields[i].enabled = (lives >= i+1);
        }
    }
    private void RainbowRushUpdate()
    {
        if (rainbowRush)
        {
            rainbowTimer += Time.deltaTime;
            WaveSpawningSystem.instance.enemyTimer -= Time.deltaTime * 3f;
            rainbowMeter = Mathf.Lerp(meterMax, 0f, rainbowTimer / rainbowRushTime);
            meter.targetFill = rainbowMeter / meterMax;
            if (rainbowTimer > rainbowRushTime)
            {
                rainbowRush = false;
                rainbowTimer = 0.0f;
                rainbowRushReminderTimer = 0.0f;
                PulseShield();
                configureWeapon();
                SoundManager.instance.PlaySFX(meter.source, GameModel.instance.uiSounds[4], 0.4f, 0.6f);
            }
        }
    }
    private void OrbUpdate()
    {
        if (orbAnimTimer != -1f)
        {
            orbAnimTimer += Time.deltaTime;
        }
        if (orbAnimTimer > orbAnimInterval)
        {
            orbAnimTimer = -1f;
            return;
        }
        for (int i = 0; i < orbs.Length; i++)
        {
            if (orbs[i] != mainOrb)
            {
                orbs[i].localScale = Vector3.Lerp(orbs[i].localScale, new Vector3(0.17f, 0.17f, 0f), orbAnimTimer / orbAnimInterval);
            }
            else
            {
                orbs[i].localScale = Vector3.Lerp(orbs[i].localScale, new Vector3(0.28f, 0.28f, 0f), orbAnimTimer / orbAnimInterval);
            }
            orbs[i].localPosition = Vector3.Lerp(orbs[i].localPosition, orbTargetPos[i], orbAnimTimer / orbAnimInterval);
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
            float rotation = rotationTarget;
            if (rainbowRush)
            {
                rotation += UnityEngine.Random.Range(-15f, 15f);
                gameColor = (GameModel.GameColor)UnityEngine.Random.Range(0, 6);
            }
            bulletScript.initialize(transform.position, rotation, bulletSpeed, ApplyCombiners(gameColor), piercing, bulletSize, false);
            bulletScript.SetColor(modelGame.ColorToColor(bulletScript.bulletColor));
            if (!gameManager.activeBullets.Contains(bulletScript))
            {
                gameManager.activeBullets.Add(bulletScript);
                GameManager.instance.shotsFired++;
            }
            gameManager.inactiveBullets.Remove(bulletScript);

            //Play firing sound
            if (GameManager.instance.currentState != GameManager.GameState.MAINMENU)
            {
                SoundManager.instance.PlaySFX(playerAudio, GameModel.instance.bulletSounds[0]);
            }
        }
        else
        {
            //Sets initial to 0 if odd and 1 if even
            int initial = 1 - (numShots % 2);
            for (int s = initial; s < numShots + initial; s++)
            {
                float angleOffSet =
                    ((((float)s / (numShots + (1 - (2 * (numShots % 2))))) * shotSpread) -
                     (shotSpread / 2));
                if (rainbowRush)
                {
                    angleOffSet += UnityEngine.Random.Range(-10f, 10f);
                    gameColor = colorOrder[UnityEngine.Random.Range(0, 2)];
                }
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
                bulletScript.initialize(transform.position, rotationTarget + angleOffSet, bulletSpeed, ApplyCombiners(gameColor), piercing, bulletSize, false);
                bulletScript.SetColor(modelGame.ColorToColor(bulletScript.bulletColor));
                if (!gameManager.activeBullets.Contains(bulletScript))
                {
                    gameManager.activeBullets.Add(bulletScript);
                    GameManager.instance.shotsFired++;
                }
                gameManager.inactiveBullets.Remove(bulletScript);
            }

            //Play firing sounds
            if (GameManager.instance.currentState != GameManager.GameState.MAINMENU)
            {
                SoundManager.instance.PlaySFX(playerAudio, GameModel.instance.bulletSounds[0]);
                for (int k = 1; k < numShots; k++)
                {
                    SoundManager.instance.PlaySFX(playerAudio, GameModel.instance.bulletSounds[0], 0.03f * k);
                }
            }
        }
    }

    private void RocketFire(List<GameModel.GameColor> colors)
    {
        if (colors.Count == 1)
        {
            GameModel.GameColor gameColor = colors[0];
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
            float rotation = rotationTarget;
            if (rainbowRush)
            {
                rotation += UnityEngine.Random.Range(-15f, 15f);
                gameColor = (GameModel.GameColor)UnityEngine.Random.Range(0, 6);
            }
            bulletScript.initialize(transform.position, rotation, bulletSpeed, ApplyCombiners(gameColor), piercing, bulletSize, true);
            bulletScript.SetColor(modelGame.ColorToColor(bulletScript.bulletColor));
            if (!gameManager.activeBullets.Contains(bulletScript))
            {
                gameManager.activeBullets.Add(bulletScript);
                GameManager.instance.shotsFired++;
            }
            gameManager.inactiveBullets.Remove(bulletScript);

            //Play firing sound
            if (GameManager.instance.currentState != GameManager.GameState.MAINMENU)
            {
                SoundManager.instance.PlaySFX(playerAudio, GameModel.instance.bulletSounds[4]);
            }
        }
        else
        {
            //Sets initial to 0 if odd and 1 if even
            int initial = 1 - (colors.Count % 2);
            for (int s = initial; s < colors.Count + initial; s++)
            {
                GameModel.GameColor gameColor;

                if (initial == 1)
                {
                    gameColor = colors[s - 1];
                }
                else
                {
                    gameColor = colors[s];
                }
                float angleOffSet =
                    ((((float)s / (colors.Count + (1 - (2 * (colors.Count % 2))))) * 180) -
                     (180 / 2));
                if (rainbowRush)
                {
                    angleOffSet += UnityEngine.Random.Range(-10f, 10f);
                    gameColor = colorOrder[UnityEngine.Random.Range(0, 2)];
                }
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

                bulletScript.initialize(transform.position, rotationTarget + angleOffSet, bulletSpeed, ApplyCombiners(gameColor), piercing, bulletSize, true);
                bulletScript.SetColor(modelGame.ColorToColor(bulletScript.bulletColor));

                if (!gameManager.activeBullets.Contains(bulletScript))
                {
                    gameManager.activeBullets.Add(bulletScript);
                    GameManager.instance.shotsFired++;
                }
                gameManager.inactiveBullets.Remove(bulletScript);
            }

            //Play firing sounds
            if (GameManager.instance.currentState != GameManager.GameState.MAINMENU)
            {
                SoundManager.instance.PlaySFX(playerAudio, GameModel.instance.bulletSounds[4]);
                for (int k = 1; k < numShots; k++)
                {
                    SoundManager.instance.PlaySFX(playerAudio, GameModel.instance.bulletSounds[4], 0.06f * k);
                }
            }
        }
    }

        public GameModel.GameColor ApplyCombiners(GameModel.GameColor testColor)
    {
        return combineChance > UnityEngine.Random.Range(0, 100) ? GameModel.instance.ReturnRandomMixedColor(testColor) : testColor;
    }
    
    public void setColor(int index)
    {
        colorPlace = index;
        playerColor = colorOrder[colorPlace];
        barrel.color = modelGame.ColorToColor(playerColor);
        setNextOrb();
        configureWeapon();
    }

    public void nextColor()
    {
        colorPlace++;
        if (colorPlace >= colorOrder.Length)
        {
            colorPlace = 0;
        }
        playerColor = colorOrder[colorPlace];
        barrel.color = modelGame.ColorToColor(playerColor);
        setNextOrb();
        configureWeapon();
    }

    private void setNextOrb()
    {
        mainOrb = orbs[colorPlace];
        for (int i= 0; i < orbs.Length; i++)
        {
            if (colorPlace + i < orbs.Length)
            {
                orbTargetPos[colorPlace + i] = orbPositions[i];
            }
            else
            {
                orbTargetPos[colorPlace - orbs.Length + i] = orbPositions[i];
            }
        }
        orbAnimTimer = 0.0f;
    }

    public void configureWeapon()
    {
        bulletSpeed = FinalShotSpeed(playerColor);
        bulletSize = FinalShotSize(playerColor);
        shotSpeed = FinalRateOfFire(playerColor);
        numShots = FinalNumShots(playerColor);
        piercing = FinalPiercing(playerColor);
        combineChance = FinalCombineChance();
        meterMult = FinalMeterMult();
        rainbowRushTime = FinalRainbowRushTime();
        deathPulse = false;
        pulseShield = false;
        foreach (GameManager.Upgrade u in upgrades)
        {
            if (u.type.Equals(GameManager.UpgradeType.DEATHBLAST))
            {
                deathPulse = true;
            }
            if (u.type.Equals(GameManager.UpgradeType.PULSE))
            {
                pulseShield = true;
            }
        }
        pulseInterval = FinalPulseInterval();
        shieldPulseRadius = FinalShieldPulseRadius();
        rocketColors = FinalRocketColors();
        rocketRoF = FinalRocketRateOfFire();

        SetMeter();
    }

    public float FinalRainbowRushTime()
    {
        float rushTime = 10.0f;
        foreach (GameManager.Upgrade u in upgrades)
        {
            if (u.type.Equals(GameManager.UpgradeType.RAINBOWMULT))
            {
                rushTime += u.factor;
            }
        }
        return rushTime;
    }


    public float FinalMeterMult()
    {
        float mult = 1f;
        foreach (GameManager.Upgrade u in upgrades)
        {
            if (u.type.Equals(GameManager.UpgradeType.RAINBOWMULT))
            {
                mult += (u.factor * modelGame.meterMultInc);
            }
        }
        mult *= WaveSpawningSystem.instance.globalRainbowMult;
        return mult;
    }

    public float FinalPulseInterval()
    {
        float pulse = -1f;
        foreach (GameManager.Upgrade u in upgrades)
        {
            if (u.type.Equals(GameManager.UpgradeType.PULSE))
            {
                if (pulse == -1f)
                {
                    pulse = 10f;
                }
                else
                {
                    pulse -= 2f;
                }
            }
        }
        return pulse;
    }

    public float FinalShieldPulseRadius()
    {
        float shieldPulse = modelGame.shieldPulseRadius;
        foreach (GameManager.Upgrade u in upgrades)
        {
            if (u.type.Equals(GameManager.UpgradeType.PULSERADIUS))
            {
                shieldPulse += (u.factor * modelGame.shieldPulseInc);
            }
        }
        return shieldPulse;
    }

    public float FinalRocketRateOfFire()
    {
        rocketRoF = modelGame.rocketInterval;
        float totalRofScalar = 1f;
        foreach (GameManager.Upgrade u in upgrades)
        {
            if (u.type.Equals(GameManager.UpgradeType.BARRAGE))
            {
                totalRofScalar += (u.factor * modelGame.rocketBarrageUpg);
            }
        }
        return rocketRoF / totalRofScalar;

    }

    public List<GameModel.GameColor> FinalRocketColors()
    {
        rocketColors.Clear();
        rocketRoF = modelGame.rocketInterval;
        foreach (GameManager.Upgrade u in upgrades)
        {
            if (u.type.Equals(GameManager.UpgradeType.ROCKETS))
            {
                for (int i = 0; i < u.factor; i++)
                {
                    rocketColors.Add(u.color);
                }
            }
        }
        foreach (GameManager.Upgrade u in upgrades)
        {
            if (u.type.Equals(GameManager.UpgradeType.ROCKETS))
            {
                for (int i = 0; i < u.factor; i++)
                {
                    rocketColors.Add(u.color);
                }
            }
            if (u.type.Equals(GameManager.UpgradeType.BARRAGE))
            {
                rocketRoF /= (1 / u.factor * modelGame.rocketBarrageUpg);
            }
        }

        return rocketColors;
    }


    public int FinalCombineChance()
    {
        int chance = 0;
        foreach (GameManager.Upgrade u in upgrades)
        {
            if (u.type.Equals(GameManager.UpgradeType.COMBINER))
            {
                chance += (u.factor * modelGame.combinerBaseChance);
            }
        }
        return chance;
    }

    public float FinalShotSpeed(GameModel.GameColor currentColor)
    {
        if (rainbowRush)
        {
            return baseBulletSpeed * 2f;
        }
        bulletSpeed = baseBulletSpeed;
        foreach (GameManager.Upgrade u in upgrades)
        {
            if ((u.color.Equals(currentColor) || u.color.Equals(GameModel.GameColor.WHITE)) && u.type.Equals(GameManager.UpgradeType.PIERCING))
            {
                for (int k = 0; k < u.factor; k++)
                {
                    bulletSpeed *= modelGame.shotSpeedMultiplier;
                }
            }
        }
        return bulletSpeed;
    }
    public float FinalShotSize(GameModel.GameColor currentColor)
    {
        if (rainbowRush)
        {
            return baseBulletSize * modelGame.shotSizeMultiplier;
        }
        bulletSize = baseBulletSize;
        foreach (GameManager.Upgrade u in upgrades)
        {
            if ((u.color.Equals(currentColor) || u.color.Equals(GameModel.GameColor.WHITE)) && u.type.Equals(GameManager.UpgradeType.SHOTSIZE))
            {
                for (int k = 0; k < u.factor; k++)
                {
                    bulletSize *= modelGame.shotSizeMultiplier;
                }
            }
        }
        return bulletSize;
    }
    public float FinalRateOfFire(GameModel.GameColor currentColor)
    {
        shotSpeed = baseShotSpeed;
        if (rainbowRush)
        {
            for (int i = 0; i < 3; i++)
            {
                foreach (GameManager.Upgrade u in upgrades)
                {
                    if ((u.color.Equals(colorOrder[i]) || u.color.Equals(GameModel.GameColor.WHITE)) && u.type.Equals(GameManager.UpgradeType.ATTACKSPEED))
                    {
                        for (int k = 0; k < u.factor; k++)
                        {
                            shotSpeed /= modelGame.rapidFireMultiplier;
                        }
                    }
                }

            }
            shotSpeed /= 8;
        }
        else
        {
            foreach (GameManager.Upgrade u in upgrades)
            {
                if ((u.color.Equals(currentColor) || u.color.Equals(GameModel.GameColor.WHITE)) && u.type.Equals(GameManager.UpgradeType.ATTACKSPEED))
                {
                    for (int k = 0; k < u.factor; k++)
                    {
                        shotSpeed /= modelGame.rapidFireMultiplier;
                    }
                }
            }
        }
        return shotSpeed;
    }
    
    public int FinalNumShots(GameModel.GameColor currentColor)
    {
        if (rainbowRush)
        {
            return 1;
        }
        numShots = baseNumShots;
        shotSpread = modelGame.baseSpreadAngle;
        foreach (GameManager.Upgrade u in upgrades)
        {
            if ((u.color.Equals(currentColor) || u.color.Equals(GameModel.GameColor.WHITE)) && u.type.Equals(GameManager.UpgradeType.SHOTS))
            {
                for (int k = 0; k < u.factor; k++)
                {
                    numShots += modelGame.numShotsUpgrade;
                    shotSpread += modelGame.baseSpreadAngle;
                }
            }
        }
        return numShots;
    }
    
    public int FinalPiercing(GameModel.GameColor currentColor)
    {
        if (rainbowRush)
        {
            return 1;
        }
        piercing = basePiercing;
        foreach (GameManager.Upgrade u in upgrades)
        {
            if ((u.color.Equals(currentColor) || u.color.Equals(GameModel.GameColor.WHITE)) && u.type.Equals(GameManager.UpgradeType.PIERCING))
            {
                for (int k = 0; k < u.factor; k++)
                {
                    piercing += modelGame.piercingUpgrade;
                }
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
            //clicks = 0;
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
        if (Input.touches.Length > 0)
        {
            Vector3 touchPos = Camera.main.ScreenToWorldPoint(Input.touches[0].position);
            touchPos = new Vector3(touchPos.x, touchPos.y, 0);

            //Begin click
            //Track the clickposition
            //if one click has already been executed, switch color
            if (Input.touches[0].phase == TouchPhase.Began)
            {
                if (clicks == 0)
                {
                    firstTapPos = touchPos;
                }
                if (clicks > 0 && GameManager.instance.doubleTapCycle && SelectorRing.inRing(touchPos))
                {
                    nextColor();
                    SelectorRing.Close();
                }
            }
            //End click
            //If the position is close to where it began, increment clicks
            if (Input.touches[0].phase == TouchPhase.Ended)
            {
                if (Vector3.Distance(transform.position, touchPos) < 1f && rainbowMeter >= meterMax)
                {
                    if (meter.selected)
                    {
                        meter.SetToSmall();
                        rainbowTimer = 0f;
                        rainbowRush = true;
                        rainbowMeter = meterMax;
                        meter.isActive = false;
                        meter.rotationSpeed = 0.1f;
                        WaveSpawningSystem.instance.AddExtraEnemies();
                        configureWeapon();
                        rainbowReminder.Hide();
                        rainbowRushReminderTimer = 0.0f;
                    }
                    else
                    {
                        meter.selected = true;
                        meter.SetToBig();
                    }
                    return;
                }
                else
                {
                    meter.selected = false;
                    meter.SetToSmall();
                }


                clicks++;
                mouseTimer = 0f;
                if (clicks == 1 && Vector3.Distance(firstTapPos, touchPos) <= 0.4f && !SelectorRing.activated)
                {
                    SelectorRing.Open();
                }
                else
                {
                    SelectorRing.Close();
                    mouseTimer = -1.0f;
                    clicks = 0;
                }
            }
        }
    }

    public float selectorTimer = 0.0f;
    public Vector3 firstTapPos;
    void takeMouseInput()
    {

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos = new Vector3(mousePos.x, mousePos.y, 0);

        //Begin click
        //Track the clickposition
        //if one click has already been executed, switch color
        if (Input.GetMouseButtonDown(0))
        {

            if (clicks == 0)
            {
                firstTapPos = mousePos;
            }
            if (clicks > 0 && GameManager.instance.doubleTapCycle && SelectorRing.inRing(mousePos))
            {
                nextColor();
            }
        }
        //End click
        //If the position is close to where it began, increment clicks
        if (Input.GetMouseButtonUp(0))
        {
            if (Vector3.Distance(transform.position, mousePos) < 1f && rainbowMeter >= meterMax )
            {
                if (meter.selected)
                {
                    rainbowRush = true;
                    meter.isActive = false;
                    meter.rotationSpeed = 0.1f;
                    WaveSpawningSystem.instance.AddExtraEnemies();
                    configureWeapon();
                    meter.SetToSmall();
                    SoundManager.instance.PlaySFX(meter.source, GameModel.instance.bulletSounds[5]);
                }
                else
                {
                    meter.selected = true;
                    meter.SetToBig();
                }
                return;
            }
            else
            {
                meter.selected = false;
                meter.SetToSmall();
            }


            clicks++;
            mouseTimer = 0f;
            if (clicks == 1 && Vector3.Distance(firstTapPos, mousePos) <= 0.1f)
            {
                SelectorRing.Open();
            }
            else
            {
                SelectorRing.StartAnimation(true);
                mouseTimer = -1.0f;
                clicks = 0;
            }
        }

        if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            setColor(0);
        }
        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            setColor(1);
        }
        if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            setColor(2);
        }
    }

    void PulseShield()
    {
        GameObject newPulse = Instantiate(shieldPulse, this.transform.position, Quaternion.identity);
        newPulse.GetComponent<pulseEffect>().initialize(shieldPulseRadius);
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(this.transform.position, shieldPulseRadius);
        foreach (Collider2D c in hitColliders)
        {
            if (c.gameObject.tag == "Enemy")
            {
                EnemyBehavior thisB = c.gameObject.GetComponent<EnemyBehavior>();
                if (deathPulse)
                {
                    thisB.TakeHit(thisB.enemyColor);
                }
                else
                {
                    thisB.StartKnockBack(false, shieldPulseRadius);
                }
            }
        }
        
    }

    public void TakeHit()
    {
        if (GameManager.instance.currentState != GameManager.GameState.MAINMENU)
        {
            lives--;
        }
        if (lives < 0 && GameManager.instance.currentState != GameManager.GameState.MAINMENU)
        {
            GameManager.instance.SetState(GameManager.GameState.LOSE);
        }
        else
        {
            PulseShield();
        }
        if (GameManager.instance.currentState != GameManager.GameState.MAINMENU)
        {
            SoundManager.instance.PlaySFX(playerAudio, GameModel.instance.bulletSounds[3]);
        }
    }

    public void IncreaseMeter()
    {
        if (!rainbowRush)
        {
            float fillAmnt = UnityEngine.Random.Range(meterMax / (60 / (meterMult)), meterMax / (45 / (meterMult)));
            GameManager.instance.rainbowInk += (int)(fillAmnt * 50);
            rainbowMeter += fillAmnt;
            meter.targetFill = rainbowMeter / meterMax;
            meter.fillTimer = 0.0f;
            if (meter.targetFill > 1)
            {
                rainbowMeter = meterMax;
                meter.targetFill = 1;
            }
            rainbowInkScript.Appear();
        }
    }

    public void SetMeter()
    {
        if (rainbowMeter > meterMax)
        {
            rainbowMeter = meterMax;
        }
        meter.targetFill = rainbowMeter / meterMax;
        if (rainbowMeter >= meterMax)
        {
            meter.SetToActive();
        }
        else
        {
            meter.SetToInactive();
        }
    }

    public void LoadData(GameData data)
    {
        upgrades = data.playerUpgrades;
        lives = data.playerLives;
        rainbowMeter = data.rainbowMeter;
        SetMeter();
    }

    public void SaveData(ref GameData data)
    {
        data.playerUpgrades = upgrades;
        data.playerLives = lives;
        data.rainbowMeter = rainbowMeter;
    }
}
