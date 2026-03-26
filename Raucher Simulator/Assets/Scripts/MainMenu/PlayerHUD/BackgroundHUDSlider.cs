using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(Button))]
public class BackgroundHUDFader : MonoBehaviour
{
    [Header("Positionen")]
    [SerializeField] private Vector2 visiblePosition = new Vector2(-754f, 442f);
    [SerializeField] private Vector2 hiddenPosition = new Vector2(-971f, 442f);
    [SerializeField] private Vector2 startOutsidePosition = new Vector2(-1300f, 442f);

    [Header("Animation")]
    [SerializeField] private float slideDuration = 0.4f;
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float autoCloseDelay = 10f;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Button button;

    private Coroutine moveRoutine;
    private Coroutine autoCloseRoutine;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        button = GetComponent<Button>();

        button.onClick.AddListener(OnHudClicked);
    }

    private void Start()
    {
        ResetToStartOutside();
        OpenFromOutside();
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(OnHudClicked);
    }

    private void OnHudClicked()
    {
        OpenFromHidden();
    }

    public void ResetToHidden()
    {
        StopAllHudCoroutines();
        rectTransform.anchoredPosition = hiddenPosition;   // x = -971
        canvasGroup.alpha = 1f;
    }

    public void ResetToStartOutside()
    {
        StopAllHudCoroutines();
        rectTransform.anchoredPosition = startOutsidePosition;
        canvasGroup.alpha = 0f;
    }

    public void PlayIntro()
    {
        ResetToStartOutside();
        OpenFromOutside();
    }

    private void OpenFromOutside()
    {
        PlayMove(startOutsidePosition, visiblePosition, 0f, 1f);
        RestartAutoClose();
    }

    private void OpenFromHidden()
    {
        PlayMove(rectTransform.anchoredPosition, visiblePosition, canvasGroup.alpha, 1f);
        RestartAutoClose();
    }

    private void CloseToHidden()
    {
        PlayMove(rectTransform.anchoredPosition, hiddenPosition, canvasGroup.alpha, 1f);
    }

    private void RestartAutoClose()
    {
        if (autoCloseRoutine != null)
            StopCoroutine(autoCloseRoutine);

        autoCloseRoutine = StartCoroutine(AutoCloseAfterDelay());
    }

    private IEnumerator AutoCloseAfterDelay()
    {
        yield return new WaitForSeconds(autoCloseDelay);
        CloseToHidden();
    }

    private void PlayMove(Vector2 fromPos, Vector2 toPos, float fromAlpha, float toAlpha)
    {
        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        moveRoutine = StartCoroutine(MoveRoutine(fromPos, toPos, fromAlpha, toAlpha));
    }

    private IEnumerator MoveRoutine(Vector2 fromPos, Vector2 toPos, float fromAlpha, float toAlpha)
    {
        float time = 0f;

        while (time < slideDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / slideDuration);

            rectTransform.anchoredPosition = Vector2.Lerp(fromPos, toPos, t);

            float fadeT = Mathf.Clamp01(time / fadeDuration);
            canvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, fadeT);

            yield return null;
        }

        rectTransform.anchoredPosition = toPos;
        canvasGroup.alpha = toAlpha;
        moveRoutine = null;
    }

    private void StopAllHudCoroutines()
    {
        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
            moveRoutine = null;
        }

        if (autoCloseRoutine != null)
        {
            StopCoroutine(autoCloseRoutine);
            autoCloseRoutine = null;
        }
    }
}