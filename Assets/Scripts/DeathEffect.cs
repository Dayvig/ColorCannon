using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathEffect : MonoBehaviour
{

    private float fadeInterval = 30.0f;
    private float fadeTimer = 0.0f;
    public Vector3 baseScale = new Vector3(0.11f, 0.11f, 0.11f);
    public SpriteRenderer ren;
    // Start is called before the first frame update

    // Update is called once per frame
    public void initialize()
    {
        this.gameObject.SetActive(true);
        fadeTimer = 0.0f;
    }
    public void Update()
    {
        fadeTimer += Time.deltaTime;
        Color col = ren.color;
        ren.color = new Color(ren.color.r, ren.color.g, ren.color.b, (fadeInterval - fadeTimer) * 0.6f / fadeInterval);
        if (fadeTimer > fadeInterval)
        {
            GameManager.instance.markedForDeathSplatters.Add(this);
        }
    }
}
