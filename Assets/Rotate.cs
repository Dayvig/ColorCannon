using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class Rotate : MonoBehaviour
{
    private float rotationCurrent = 0;
    private float rotationTarget = 0;
    private Vector3 actualRotation;
    [Range(1f, 0.1f)]
    public float shotSpeed = 0.5f;
    [Range(0.005f, 0.1f)]
    public float bulletSpeed = 0.01f;
    private float shotTimer;
    public GameObject bullet;

    private float enemyTimer = 0.0f;
    private float enemySpawn = 1.0f;
    public GameObject enemy;
    
    private float xBounds = 3;
    private float yBounds = 5;
    public SpriteRenderer barrel;

    public GameModel.GameColor playerColor;
    public GameModel modelGame;

    public float mouseTimer = 0.0f;
    private float dClickInterval = 0.4f;
    public int clicks = 0;

    private GameModel.GameColor[] colorOrder =
        {GameModel.GameColor.RED, GameModel.GameColor.BLUE, GameModel.GameColor.YELLOW};

    private int colorPlace = 0;
    
    void Start()
    {
        modelGame = GameObject.Find("GameManager").GetComponent<GameModel>();
        playerColor = colorOrder[colorPlace];
        barrel.color = modelGame.ColorToColor(playerColor);
    }
    
    // Update is called once per frame
    void Update()
    {
        EnemyUpdate();
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
            GameObject newBulletObject = Instantiate(bullet, transform.position, Quaternion.identity);
            Bullet bulletScript = newBulletObject.GetComponent<Bullet>();
            bulletScript.initialize(rotationTarget, bulletSpeed, playerColor);
            bulletScript.SetColor(modelGame.ColorToColor(bulletScript.bulletColor));
            shotTimer = 0.0f;
        }

    }

    void EnemyUpdate()
    {
        enemyTimer += Time.deltaTime;
        if (enemyTimer > enemySpawn)
        {
            int EdgeToSpawnFrom = Random.Range(0, 4);
            Vector3 startPos;
            switch (EdgeToSpawnFrom)
            {
                case 0:
                    startPos = new Vector3(Random.Range(xBounds, -xBounds), yBounds, 0);
                    break;
                case 1:
                    startPos = new Vector3(xBounds, Random.Range(yBounds, -yBounds), 0);
                    break;
                case 2:
                    startPos = new Vector3(Random.Range(xBounds, -xBounds), -yBounds, 0);
                    break;
                case 3:
                    startPos = new Vector3(-xBounds, Random.Range(yBounds, -yBounds), 0);
                    break;
                default:
                    startPos = new Vector3(Random.Range(xBounds, -xBounds), yBounds, 0);
                    break;
            }
            GameObject newEnemy = Instantiate(enemy, startPos, Quaternion.identity);
            EnemyBehavior enemyScript = newEnemy.GetComponent<EnemyBehavior>();
            GameModel.GameColor RandomEnemyColor = colorOrder[(int) Random.Range(0, 3)];
            enemyScript.initialize(transform.position, RandomEnemyColor);
            enemyScript.SetColor(modelGame.ColorToColor(RandomEnemyColor));
            enemyTimer = 0.0f;
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
            if (clicks == 1){ 
                nextColor();
                clicks = 0;
            }
            else
            {
                clicks++;
                mouseTimer = 0.0f;
            }
        }
    }
}
