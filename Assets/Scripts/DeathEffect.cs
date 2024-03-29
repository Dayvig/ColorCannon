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
    public virtual void initialize()
    {
        this.gameObject.SetActive(true);
        fadeInterval = 30f * GameManager.instance.splatterVal;
        fadeTimer = 0.0f;
        ren.sprite = GameModel.instance.giblets[Random.Range(0, 4) + 4];
        this.transform.rotation = Random.rotation;
        Vector3 eulerRotation = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(0, 0, eulerRotation.z);
    }

    public virtual void Update()
    {
        fadeTimer += Time.deltaTime;
        Color col = ren.color;
        ren.color = new Color(ren.color.r, ren.color.g, ren.color.b, ((fadeInterval - fadeTimer) * 0.6f * GameManager.instance.splatterVal) / fadeInterval);
        if (fadeTimer > fadeInterval)
        {
            GameManager.instance.markedForDeathSplatters.Add(this);
        }
    }
}
