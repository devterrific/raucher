using UnityEngine;

public class FilterAimer : MonoBehaviour
{
    private RectTransform rt;
    private float speed;
    private bool moving = false;
    private bool rightToLeft = true;
    private float leftLimitX, rightLimitX; // in local anchoredPosition
    private System.Action onPassedLimits;

    public void Init(RectTransform host, float speed, bool rightToLeft, float rangeWidth, System.Action onPassedLimits)
    {
        this.rt = GetComponent<RectTransform>();
        this.speed = speed;
        this.rightToLeft = rightToLeft;
        this.onPassedLimits = onPassedLimits;

        // Start rechts, laufe nach links
        float startX = rangeWidth * 0.5f;
        float endX = -rangeWidth * 0.5f;
        rightLimitX = Mathf.Max(startX, endX);
        leftLimitX = Mathf.Min(startX, endX);

        rt.anchoredPosition = new Vector2(rightToLeft ? rightLimitX : leftLimitX, 0f);
        moving = true;
    }

    private void Update()
    {
        if (!moving) return;
        var p = rt.anchoredPosition;
        float dir = rightToLeft ? -1f : 1f;
        p.x += dir * speed * Time.deltaTime;
        rt.anchoredPosition = p;

        // Wenn komplett aus dem Bereich -> Auto-Ende (Miss)
        if (rightToLeft && p.x <= leftLimitX || (!rightToLeft && p.x >= rightLimitX))
        {
            moving = false;
            onPassedLimits?.Invoke();
        }
    }

    public void Freeze() => moving = false;

    public Vector2 CenterWorld() => rt.TransformPoint(rt.rect.center);
}
