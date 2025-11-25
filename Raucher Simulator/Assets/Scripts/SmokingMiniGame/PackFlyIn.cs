using System.Collections;
using UnityEngine;

public class PackFlyIn : MonoBehaviour
{
    [SerializeField] private RectTransform rect;
    [SerializeField] private Vector2 startOffset = new Vector2(500f, 0f);
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private AnimationCurve curve = null;

    // NEU: Soll das Pack vor dem Fly-In unsichtbar sein?
    [SerializeField] private bool hideBeforePlay = true;

    public bool IsPlaying { get; private set; }

    // NEU: zum Ein-/Ausblenden
    private CanvasGroup canvasGroup;
    private UnityEngine.UI.Image image;

    private void Awake()
    {
        if (rect == null)
            rect = GetComponent<RectTransform>();

        if (curve == null)
            curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        canvasGroup = GetComponent<CanvasGroup>();
        image = GetComponent<UnityEngine.UI.Image>();

        // Direkt beim Start unsichtbar machen (aber GameObject bleibt aktiv)
        if (hideBeforePlay)
            HideVisual();
    }

    private void HideVisual()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        else if (image != null)
        {
            image.enabled = false;
        }
    }

    private void ShowVisual()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        else if (image != null)
        {
            image.enabled = true;
        }
    }

    public IEnumerator Play()
    {
        if (rect == null)
            yield break;

        IsPlaying = true;

        // NEU: Ab hier wird das Pack überhaupt sichtbar
        ShowVisual();

        Vector2 targetPos = rect.anchoredPosition;
        Vector2 startPos = targetPos + startOffset;

        rect.anchoredPosition = startPos;

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
