using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ringScript : MonoBehaviour
{
    public float animTimer = 0.0f;
    public float animInterval = 0.4f;
    public float ringScale = 1f;
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

        Debug.DrawLine(this.transform.position, new Vector3(this.transform.position.x, this.transform.position.y + (ringScale / 2), 0f), Color.green, 1f);
        Debug.DrawLine(this.transform.position, new Vector3(this.transform.position.x+0.1f, this.transform.position.y + ringScale, 0f), Color.red, 1f);

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

    public void Close()
    {
        StartAnimation(true);
        activated = false;
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

        Vector3 newScale = Vector3.Lerp(0.05f * Vector3.one, ((ringScale+0.05f)/2) * Vector3.one, animTimer / animInterval);
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

    public bool inRingCenter(Vector3 pos)
    {
        Vector3 newPos = new Vector3(pos.x, pos.y, 0);
        float yDiff = newPos.y - transform.position.y;
        float xDiff = newPos.x - transform.position.x;

        return (Mathf.Abs(xDiff) < ringScale/2 && Mathf.Abs(yDiff) < ringScale / 2);
    }

    private void inputUpdate()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 newMousePos = new Vector3(mousePos.x, mousePos.y, 0);
        float yDiff = newMousePos.y - transform.position.y;
        float xDiff = newMousePos.x - transform.position.x;
        if (yDiff > ringScale/2 && Mathf.Abs(yDiff) > Mathf.Abs(xDiff))
        {
            player.setColor(0);
            if (activeSlice != 0)
            {
                hoverTimer = 0.0f;
                activeSlice = 0;
                prevSlice = 0;
            }
        }
        else if (xDiff > ringScale / 2)
        {
            player.setColor(1);
            if (activeSlice != 1)
            {
                hoverTimer = 0.0f;
                activeSlice = 1;
                prevSlice = 1;
            }
        }
        else if (xDiff < -ringScale/2)
        {
            player.setColor(2);
            if (activeSlice != 2)
            {
                hoverTimer = 0.0f;
                activeSlice = 2;
                prevSlice = 2;
            }
        }
        else
        {
            activeSlice = -1;
        }
    }

    public void SetPosition() { 
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 newMousePos = new Vector3(mousePos.x, mousePos.y, 0);
        this.transform.position = newMousePos;
    }
}
