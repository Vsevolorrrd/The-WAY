using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Highlight : MonoBehaviour
{
    public Color colorA = Color.red;
    public Color colorB = Color.blue;
    public float duration = 1f;

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        float t = Mathf.PingPong(Time.time / duration, 1f);
        spriteRenderer.color = Color.Lerp(colorA, colorB, t);
    }
}