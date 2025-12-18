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

    /// <summary>
    /// parent:   der RectTransform, in dessen lokaler Fläche wir uns bewegen (z.B. FilterAnchor)
    /// rangeWidth: Gesamtbreite, die der Filter horizontal abfahren darf
    /// startRightToLeft: Startet rechts und geht nach links (true) oder umgekehrt (false)
    /// loopMovement: true = Hin-und-Her-Bewegung, false = nur ein Durchlauf, danach onMiss
    /// onMiss: Callback, wenn der Filter den Bereich verlässt, ohne gedroppt zu werden
    /// </summary>
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

        // Startposition: ganz rechts oder ganz links
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
                dir = 1; // Richtung umdrehen
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
                dir = -1; // Richtung umdrehen
            }
            else
            {
                isMoving = false;
                onMiss?.Invoke();
            }
        }

        rt.anchoredPosition = p;
    }

    /// <summary>
    /// Wird beim Drücken der Leertaste aufgerufen: Stoppt horizontale Bewegung
    /// und lässt den Filter leicht nach unten „einrasten“.
    /// </summary>
    public void Drop(float dropDistance = 195f, float dropTime = 0.25f)
    {
        if (hasDropped) return;
        hasDropped = true;
        isMoving = false;
        StartCoroutine(DropRoutine(dropDistance, dropTime));
    }

    // Zweite Drop-Methode, das der Filter wegfällt
    public RectTransform Rect => rt;

    public void DropToVoid(float dropDistance = 800f, float dropTime = 0.35f)
    {
        if (hasDropped) return;
        hasDropped = true;
        isMoving = false;
        StartCoroutine(DropRoutine(dropDistance, dropTime));
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

    /// <summary>
    /// Weltposition der Mitte (fürs Scoring)
    /// </summary>
    public Vector2 CenterWorld()
    {
        return rt.TransformPoint(rt.rect.center);
    }
}
