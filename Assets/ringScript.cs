using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class ringScript : MonoBehaviour
{
    public float animTimer = 0.0f;
    public float animInterval = 0.4f;
    public float ringCloseRadius = 0.1f;
    public bool reverse = false;
    public bool playAnimation = false;
    public bool activated = false;
    public SpriteRenderer ren;
    public Player player;

    // Start is called before the first frame update
    void Start()
    {
        ren = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (playAnimation)
        {
            animationUpdate();
        }
        if (activated)
        {
            inputUpdate();
        }
        ren.enabled = activated;
    }

    public void StartAnimation(bool inout)
    {
        reverse = inout;
        playAnimation = true;
    }

    public void Open()
    {
        StartAnimation(false);
        activated = true;
        SetPosition();
    }

    private void animationUpdate()
    {
        if (reverse)
        {
            animTimer -= Time.deltaTime * 1.5f;
            if (animTimer <= 0.0f)
            {
                playAnimation = false;
                activated = false;
                ren.enabled = false;
            }
        }
        else
        {
            animTimer += Time.deltaTime;
            if (animTimer >= animInterval)
            {
                playAnimation = false;
            }
        }

        Vector3 newScale = Vector3.Lerp(0.05f * Vector3.one, 0.2f * Vector3.one, animTimer / animInterval);
        this.transform.localScale = newScale;
    }

    private void inputUpdate()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 newMousePos = new Vector3(mousePos.x, mousePos.y, 0);
        float yDiff = newMousePos.y - transform.position.y;
        float xDiff = newMousePos.x - transform.position.x;
        Debug.Log(xDiff + "|"+ yDiff);
        if (Mathf.Abs(xDiff) > ringCloseRadius || Mathf.Abs(yDiff) > ringCloseRadius)
        {
            //StartAnimation(true);
            //player.clicks = 0;
        }
        if (yDiff > 0 && Mathf.Abs(yDiff) > Mathf.Abs(xDiff))
        {
            player.setColor(0);
        }
        else if (xDiff > 0)
        {
            player.setColor(1);
        }
        else if (xDiff < 0)
        {
            player.setColor(2);
        }
    }

    public void SetPosition() { 
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 newMousePos = new Vector3(mousePos.x, mousePos.y, 0);
        this.transform.position = newMousePos;
    }
}
