using System.Collections;
using UnityEngine;

public class FilterAimer : MonoBehaviour
{
    private RectTransform rt;
    private float speed;
    private float leftX;
    private float rightX;
    private int dir;
    private bool isMoving;
    private bool hasDropped;
    private bool loopMovement;
    private System.Action onMiss;

    [Header("Fail Fall Settings")]
    [SerializeField] private float failFallDistance = 1000f;
    [SerializeField] private float failFallTime = 1.2f;

    public RectTransform Rect => rt;

    public void Init(RectTransform parent, float speed, float rangeWidth,
                     bool startRightToLeft, bool loopMovement, System.Action onMiss)
    {
        rt = GetComponent<RectTransform>();
        this.speed = speed;
        this.loopMovement = loopMovement;
        this.onMiss = onMiss;

        float halfRange = rangeWidth * 0.5f;
        leftX = -halfRange;
        rightX = halfRange;

        dir = startRightToLeft ? -1 : 1;

        rt.anchoredPosition = new Vector2(startRightToLeft ? rightX : leftX, 0f);
        isMoving = true;
        hasDropped = false;
    }

    private void Update()
    {
        if (!isMoving || hasDropped) return;

        var p = rt.anchoredPosition;
        p.x += dir * speed * Time.deltaTime;

        if (p.x <= leftX)
        {
            p.x = leftX;
            if (loopMovement)
            {
                dir = 1;
            }
            else
            {
                isMoving = false;
                onMiss?.Invoke();
            }
        }
        else if (p.x >= rightX)
        {
            p.x = rightX;
            if (loopMovement)
            {
                dir = -1;
            }
            else
            {
                isMoving = false;
                onMiss?.Invoke();
            }
        }

        rt.anchoredPosition = p;
    }

    // NEU: Stoppt nur die horizontale Bewegung (für die Bewertung)
    public void StopMove()
    {
        isMoving = false;
    }

    /// <summary>
    /// Normaler Drop (Erfolg): stoppt Bewegung & droppt leicht nach unten.
    /// </summary>
    public void Drop(float dropDistance = 195f, float dropTime = 0.25f)
    {
        if (hasDropped) return;
        hasDropped = true;
        isMoving = false;
        StartCoroutine(DropRoutine(dropDistance, dropTime));
    }

    /// <summary>
    /// ✅Fail-Fall: fällt langsam nach unten und verschwindet.
    /// Überschreibt IMMER evtl. laufende Coroutines.
    /// </summary>
    public void FailFall()
    {
        // IMMER überschreiben, egal ob vorher Drop lief
        StopAllCoroutines();

        hasDropped = true;
        isMoving = false;

        StartCoroutine(FailFallRoutine());
    }

    private IEnumerator FailFallRoutine()
    {
        Vector2 start = rt.anchoredPosition;
        Vector2 end = start + Vector2.down * failFallDistance;

        float t = 0f;
        while (t < failFallTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / failFallTime);

            // bewusst linear: ruhig & klarer Fail-Moment
            rt.anchoredPosition = Vector2.Lerp(start, end, k);
            yield return null;
        }

        rt.anchoredPosition = end;

        // verschwinden
        gameObject.SetActive(false);
    }

    private IEnumerator DropRoutine(float dropDistance, float dropTime)
    {
        Vector2 start = rt.anchoredPosition;
        Vector2 end = start + new Vector2(0f, -dropDistance);

        float t = 0f;
        while (t < dropTime)
        {
            t += Time.deltaTime;
            float lerp = Mathf.Clamp01(t / dropTime);
            rt.anchoredPosition = Vector2.Lerp(start, end, lerp);
            yield return null;
        }

        rt.anchoredPosition = end;
    }

    public Vector2 CenterWorld()
    {
        return rt.TransformPoint(rt.rect.center);
    }
}
