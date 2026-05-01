using UnityEngine;

public class Ghost : MonoBehaviour
{
    public float lifetime = 0.15f;
    public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    public float startScaleMultiplier = 1.05f;
    public float endScaleMultiplier = 0.95f;

    float timer;
    SpriteRenderer sr;
    Color startColor;
    Vector3 baseScale;

    GhostPool pool;

    public void Init(Sprite sprite, bool flipX, Color color, int sortingOrder, Vector3 scale, GhostPool poolRef)
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();

        sr.sprite = sprite;
        sr.flipX = flipX;
        sr.color = color;
        sr.sortingOrder = sortingOrder;

        baseScale = scale;
        transform.localScale = baseScale * startScaleMultiplier;

        startColor = color;
        timer = 0f;

        pool = poolRef;
        gameObject.SetActive(true);
    }

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        timer += Time.deltaTime;
        float t = timer / lifetime;

        float alpha = fadeCurve.Evaluate(t);
        Color c = startColor;
        c.a *= alpha;
        sr.color = c;

        float scaleT = Mathf.Lerp(startScaleMultiplier, endScaleMultiplier, t);
        transform.localScale = baseScale * scaleT;

        if (t >= 1f)
        {
            pool.ReturnToPool(this);
        }
    }
}