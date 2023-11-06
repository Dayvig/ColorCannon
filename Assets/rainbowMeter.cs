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

    public bool selected = false;

    // Start is called before the first frame update
    void Start()
    {
        meter.color = new Color(1f, 1f, 1f, 0.2f);
        rainbows.color = new Color(1f, 1f, 1f, 0.5f);
        rainbows.fillAmount = 0;
        trans = rainbows.gameObject.transform;
    }

    // Update is called once per frame
    void Update()
    {
        trans.Rotate(new Vector3(0, 0, rotationSpeed));

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

    public void SetToActive()
    {
        rainbows.color = new Color(1f, 1f, 1f, 1f);
        meter.color = new Color(1f, 1f, 1f, 1f);
        rotationSpeed = fastRotationSpeed;
    }

    public void SetToInactive()
    {
        rainbows.color = new Color(1f, 1f, 1f, 0.5f);
        meter.color = new Color(1f, 1f, 1f, 0.2f);
        transform.localScale = baseScale;
        rotationSpeed = 0.1f;
    }
}
