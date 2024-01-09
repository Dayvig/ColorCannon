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
    public Sprite withDTapCycle;
    public Sprite noDTapCycle;
    public Player player;
    public float hoverTimer = 0.0f;
    public float hoverInterval = 1f;
    public SpriteRenderer[] slices = new SpriteRenderer[3];
    public int activeSlice = 0;
    public int prevSlice = 0;

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
            sliceUpdate();
        }
        foreach (SpriteRenderer r in slices)
        {
            r.enabled = activated;
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
        slices[0].color = GameModel.instance.redVisualColor;
        slices[1].color = GameModel.instance.blueVisualColor;
        slices[2].color = GameModel.instance.yellowVisualColor;
        ren.sprite = GameManager.instance.doubleTapCycle ? withDTapCycle : noDTapCycle;

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

    public void sliceUpdate()
    {
        if (hoverTimer < hoverInterval)
        {
            hoverTimer += Time.deltaTime;
        }
        for (int i = 0; i < slices.Length; i++)
        {
            if (activeSlice == i)
            {
                slices[i].color = new Color(slices[i].color.r, slices[i].color.g, slices[i].color.b, Mathf.Lerp(0, 1f, hoverTimer / hoverInterval));
            }
            else
            {
                if (slices[i].color.a >= Mathf.Lerp(1f, 0f, hoverTimer / hoverInterval))
                {
                    slices[i].color = new Color(slices[i].color.r, slices[i].color.g, slices[i].color.b, Mathf.Lerp(1f, 0f, hoverTimer / hoverInterval));
                }
            }
        }
    }

    public bool inRing()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 newMousePos = new Vector3(mousePos.x, mousePos.y, 0);
        float yDiff = newMousePos.y - transform.position.y;
        float xDiff = newMousePos.x - transform.position.x;

        return (Mathf.Abs(xDiff) < ringCloseRadius && Mathf.Abs(yDiff) < ringCloseRadius);
    }

    private void inputUpdate()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 newMousePos = new Vector3(mousePos.x, mousePos.y, 0);
        float yDiff = newMousePos.y - transform.position.y;
        float xDiff = newMousePos.x - transform.position.x;
        if (yDiff > 0 && Mathf.Abs(yDiff) > Mathf.Abs(xDiff))
        {
            player.setColor(0);
            if (activeSlice != 0)
            {
                hoverTimer = 0.0f;
                activeSlice = 0;
                prevSlice = 0;
            }
        }
        else if (xDiff > 0)
        {
            player.setColor(1);
            if (activeSlice != 1)
            {
                hoverTimer = 0.0f;
                activeSlice = 1;
                prevSlice = 1;
            }
        }
        else if (xDiff < 0)
        {
            player.setColor(2);
            if (activeSlice != 2)
            {
                hoverTimer = 0.0f;
                activeSlice = 2;
                prevSlice = 2;
            }
        }
    }

    public void SetPosition() { 
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 newMousePos = new Vector3(mousePos.x, mousePos.y, 0);
        this.transform.position = newMousePos;
    }
}
