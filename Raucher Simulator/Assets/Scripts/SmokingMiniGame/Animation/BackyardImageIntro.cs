using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BackyardImageIntro : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image backyardImage;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform backyardRectTransform;

    [Header("Timing")]
    [SerializeField] private float showDelay = 0f;
    [SerializeField] private float fadeInDuration = 0.45f;
    [SerializeField] private float visibleDuration = 1.5f;
    [SerializeField] private float fadeOutDuration = 0.8f;

    [Header("Slide Settings")]
    [SerializeField] private float startOffsetX = -120f;
    [SerializeField] private float endOffsetX = 80f;

    private Coroutine currentRoutine;
    private Vector2 originalAnchoredPosition;
    private bool isPaused;

    private void Awake()
    {
        EnsureReferences();

        if (backyardRectTransform != null)
        {
            originalAnchoredPosition = backyardRectTransform.anchoredPosition;
        }

        HideImmediately();
    }

    private void OnEnable()
    {
        PauseMenuManager.OnPauseStateChanged += HandlePauseStateChanged;

        if (PauseMenuManager.Instance != null)
        {
            isPaused = PauseMenuManager.Instance.IsPaused;
        }
    }

    private void OnDisable()
    {
        PauseMenuManager.OnPauseStateChanged -= HandlePauseStateChanged;
    }

    private void HandlePauseStateChanged(bool paused)
    {
        isPaused = paused;
    }

    public void Play()
    {
        if (backyardImage == null)
        {
            Debug.LogWarning("BackyardImageIntro: backyardImage fehlt.");
            return;
        }

        EnsureReferences();

        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
            currentRoutine = null;
        }

        currentRoutine = StartCoroutine(PlayRoutine());
    }

    public float GetTimeUntilFadeOutStarts()
    {
        return Mathf.Max(0f, showDelay) + Mathf.Max(0f, fadeInDuration) + Mathf.Max(0f, visibleDuration);
    }

    public void HideImmediately()
    {
        EnsureReferences();

        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
            currentRoutine = null;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }

        if (backyardRectTransform != null)
        {
            backyardRectTransform.anchoredPosition = originalAnchoredPosition;
        }

        if (backyardImage != null)
        {
            backyardImage.gameObject.SetActive(false);
        }
    }

    private IEnumerator PlayRoutine()
    {
        backyardImage.gameObject.SetActive(true);

        Vector2 startPosition = originalAnchoredPosition + new Vector2(startOffsetX, 0f);
        Vector2 centerPosition = originalAnchoredPosition;
        Vector2 endPosition = originalAnchoredPosition + new Vector2(endOffsetX, 0f);

        canvasGroup.alpha = 0f;
        backyardRectTransform.anchoredPosition = startPosition;

        yield return WaitForSecondsRealtimePausable(showDelay);

        float time = 0f;
        while (time < fadeInDuration)
        {
            if (isPaused)
            {
                yield return null;
                continue;
            }

            time += Time.unscaledDeltaTime;
            float t = fadeInDuration <= 0f ? 1f : Mathf.Clamp01(time / fadeInDuration);
            float easedT = Mathf.SmoothStep(0f, 1f, t);

            canvasGroup.alpha = easedT;
            backyardRectTransform.anchoredPosition = Vector2.Lerp(startPosition, centerPosition, easedT);

            yield return null;
        }

        canvasGroup.alpha = 1f;
        backyardRectTransform.anchoredPosition = centerPosition;

        yield return WaitForSecondsRealtimePausable(visibleDuration);

        time = 0f;
        while (time < fadeOutDuration)
        {
            if (isPaused)
            {
                yield return null;
                continue;
            }

            time += Time.unscaledDeltaTime;
            float t = fadeOutDuration <= 0f ? 1f : Mathf.Clamp01(time / fadeOutDuration);
            float easedT = Mathf.SmoothStep(0f, 1f, t);

            canvasGroup.alpha = 1f - easedT;
            backyardRectTransform.anchoredPosition = Vector2.Lerp(centerPosition, endPosition, easedT);

            yield return null;
        }

        canvasGroup.alpha = 0f;
        backyardRectTransform.anchoredPosition = originalAnchoredPosition;
        backyardImage.gameObject.SetActive(false);

        currentRoutine = null;
    }

    private IEnumerator WaitForSecondsRealtimePausable(float duration)
    {
        if (duration <= 0f)
            yield break;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (!isPaused)
            {
                elapsed += Time.unscaledDeltaTime;
            }

            yield return null;
        }
    }

    private void EnsureReferences()
    {
        if (backyardImage == null)
            return;

        if (canvasGroup == null)
        {
            canvasGroup = backyardImage.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = backyardImage.gameObject.AddComponent<CanvasGroup>();
            }
        }

        if (backyardRectTransform == null)
        {
            backyardRectTransform = backyardImage.GetComponent<RectTransform>();
        }
    }
}