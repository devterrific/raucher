using System.Collections;
using UnityEngine;

public class PackFlyIn : MonoBehaviour
{
    [SerializeField] private RectTransform rect;
    [SerializeField] private Vector2 startOffset = new Vector2(500f, 0f); // von wo aus reinkommen
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private AnimationCurve curve = null;

    private Vector2 targetPos;

    private bool initialized;
    public bool IsPlaying { get; private set; }

    private void Awake()
    {
        if (rect == null)
            rect = GetComponent<RectTransform>();

        if (curve == null)
            curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        // Zielposition merken & Startposition nach auﬂen versetzen
        targetPos = rect.anchoredPosition;
        rect.anchoredPosition = targetPos + startOffset;
        initialized = true;
    }

    public IEnumerator Play()
    {
        if (!initialized) yield break;

        IsPlaying = true;

        Vector2 startPos = targetPos + startOffset;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            float eased = curve.Evaluate(k);
            rect.anchoredPosition = Vector2.Lerp(startPos, targetPos, eased);
            yield return null;
        }

        rect.anchoredPosition = targetPos;
        IsPlaying = false;
    }
}
