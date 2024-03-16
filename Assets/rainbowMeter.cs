using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class rainbowMeter : MonoBehaviour
{

    public Image meter;
    public Image rainbows;
    public Vector3 baseScale = new Vector3(0.18f, 0.18f, 0.18f);
    public Vector3 bigScale = new Vector3(0.36f, 0.36f, 0.36f);
    public float rotationSpeed = 0.1f;
    private float fastRotationSpeed = 4f;
    public float targetFill;
    public float fillInterval = 1.2f;
    public float fillTimer = 0.0f;
    public bool isActive = false;
    public bool prevActive = false;
    Transform trans;
    public AudioSource source;

    public bool selected = false;
    public float ringAnimTimer = 0.0f;
    public float ringAnimInterval = 0.06f;
    public int state;
    //0- grow
    //1- shrink
    //2- nothing

    private Vector3 targetScale;

    // Start is called before the first frame update
    void Start()
    {
        meter.color = new Color(1f, 1f, 1f, 0.2f);
        rainbows.color = new Color(1f, 1f, 1f, 0.5f);
        rainbows.fillAmount = 0;
        trans = rainbows.gameObject.transform;
        targetScale = baseScale;
        state = 2;
    }

    // Update is called once per frame
    void Update()
    {
        trans.Rotate(new Vector3(0, 0, rotationSpeed));
        RingAnimationUpdate();

        if (!prevActive && isActive)
        {
            SetToActive();
            prevActive = isActive;
        }
        else if (prevActive && !isActive)
        {
            rainbows.fillAmount = 0.0f;
            targetFill = 0.0f;
            SetToInactive();
            prevActive = isActive;
        }
        if (targetFill - rainbows.fillAmount < 0.05f)
        {
            rainbows.fillAmount = targetFill;
        }
        if (rainbows.fillAmount != targetFill)
        {
            fillTimer += Time.deltaTime;
            rainbows.fillAmount = Mathf.Lerp(rainbows.fillAmount, targetFill, fillTimer / fillInterval);
        }
        else
        {
            fillTimer = 0.0f;
        }
        if (rainbows.fillAmount >= 1)
        {
            isActive = true;
        }
    }

    public void RingAnimationUpdate()
    {
        if (state == 0)
        {
            ringAnimTimer += Time.deltaTime;
            this.transform.localScale = Vector3.Lerp(baseScale, bigScale, ringAnimTimer / ringAnimInterval);
            if (ringAnimTimer >= ringAnimInterval)
            {
                state = 2;
            }
        }
        else if (state == 1)
        {
            ringAnimTimer -= Time.deltaTime;
            this.transform.localScale = Vector3.Lerp(baseScale, bigScale, ringAnimTimer / ringAnimInterval);
            if (ringAnimTimer <= 0.0f)
            {
                state = 2;
            }
        }

    }

    public void SetToActive()
    {
        rainbows.color = new Color(1f, 1f, 1f, 1f);
        meter.color = new Color(1f, 1f, 1f, 1f);
        rotationSpeed = fastRotationSpeed;
    }

    public void SetToBig()
    {
        targetScale = bigScale;
        state = 0;
    }

    public void SetToSmall()
    {
        targetScale = baseScale;
        state = 1;
    }

    public void SetToInactive()
    {
        rainbows.color = new Color(1f, 1f, 1f, 0.5f);
        meter.color = new Color(1f, 1f, 1f, 0.2f);
        rotationSpeed = 0.1f;
    }
}
