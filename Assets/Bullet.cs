using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float xSpeed;
    private float ySpeed;
    private Vector3 flight;
    private float xBounds = 3;
    private float yBounds = 5;
    private Vector3 positionTarget;
    private Vector3 positionCurrent;
    public SpriteRenderer ren;

    public GameModel.GameColor bulletColor;
    public GameModel modelGame;
    
    // Start is called before the first frame update
    void Start()
    {
        modelGame = GameObject.Find("GameManager").GetComponent<GameModel>();
    }

    public void initialize(float rotation, float speed, GameModel.GameColor color)
    {
        rotation = -(rotation * (Mathf.PI / 180));
        
        xSpeed = Mathf.Sin(rotation);
        ySpeed = Mathf.Cos(rotation);
        flight = new Vector3(xSpeed, ySpeed, 0).normalized * speed;

        positionCurrent = this.transform.position;
        bulletColor = color;
    }

    public void SetColor(Color c)
    {
        ren.color = c;
    }

    // Update is called once per frame
    void Update()
    {
        positionTarget += flight;
        positionCurrent = Vector3.Lerp(
            positionCurrent, 
            positionTarget, 
            Time.deltaTime * 30.0f);
        transform.position = positionCurrent;
        if (positionCurrent.x > xBounds || positionCurrent.x < -xBounds ||
            positionCurrent.y > yBounds || positionCurrent.y < -yBounds)
        {
            Destroy(this.gameObject);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("Enemy"))
        {
            Debug.Assert(col.gameObject.GetComponent<EnemyBehavior>() != null);
            EnemyBehavior enemy = col.gameObject.GetComponent<EnemyBehavior>();
            if (enemy.enemyColor == bulletColor)
            {
                Debug.Log("Player hit Enemy");
                Destroy(col.gameObject);
                Destroy(this.gameObject);
            }
        }
    }
}
