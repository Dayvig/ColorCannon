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

    public GameObject enemy;
    
    public SpriteRenderer barrel;

    public GameModel.GameColor playerColor;
    public GameModel modelGame;

    public float mouseTimer = 0.0f;
    private float dClickInterval = 0.3f;
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
