using UnityEngine;

public class pulseEffect : MonoBehaviour
{

    private float growInterval = 0.2f;
    private float effectTimer = 0.0f;
    private float fadeInterval = 0.4f;
    public float baseScale = 0.01f;
    public float finalScale = 1f;
    public SpriteRenderer ren;
    // Start is called before the first frame update

    // Update is called once per frame
    public void initialize()
    {
        effectTimer = 0.0f;
    }

    public void initialize(float scale)
    {
        effectTimer = 0.0f;
        float currentSize = ren.bounds.size.x;
        float scalar = 2 / currentSize;
        finalScale = (scale * scalar);
    }

    public void Update()
    {
        effectTimer += Time.deltaTime;
        Color col = ren.color;
        this.transform.localScale = Vector3.one * Mathf.Lerp(baseScale, finalScale, effectTimer / growInterval);
        if (effectTimer > growInterval)
        {
            ren.color = new Color(ren.color.r, ren.color.g, ren.color.b, ((growInterval + fadeInterval) - (effectTimer - growInterval)) * 0.6f / fadeInterval);
        }
        if (effectTimer > fadeInterval + growInterval)
        {
            Destroy(this.gameObject);
        }
    }
}