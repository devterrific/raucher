using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SmokingMiniGameCharacterUI : MonoBehaviour
{
    [Header("Countdown Runner")]
    [SerializeField] private Image countdownRunnerImage;
    [SerializeField] private Sprite[] walkSprites;
    [SerializeField] private float walkFps = 10f;
    [SerializeField] private Vector2 runStartPosition = new Vector2(-900f, -250f);
    [SerializeField] private Vector2 runEndPosition = new Vector2(900f, -250f);

    [Header("Corner Character")]
    [SerializeField] private Image cornerCharacterImage;
    [SerializeField] private Sprite idleCornerSprite;

    [Header("Smoking Animation")]
    [SerializeField] private Sprite[] smokingSprites;
    [SerializeField] private float smokingFrameTime = 0.25f;
    [SerializeField] private bool loopSmoking = true;

    private Coroutine runCoroutine;
    private Coroutine smokingCoroutine;

    private void Awake()
    {
        HideRunner();

        if (cornerCharacterImage != null)
        {
            cornerCharacterImage.gameObject.SetActive(false);
        }
    }

    public void PlayCountdownRun(float duration)
    {
        if (countdownRunnerImage == null)
        {
            Debug.LogWarning("SmokingMiniGameCharacterUI: Countdown Runner Image fehlt.");
            return;
        }

        if (runCoroutine != null)
        {
            StopCoroutine(runCoroutine);
        }

        runCoroutine = StartCoroutine(CountdownRunRoutine(duration));
    }

    public void ShowCornerIdle()
    {
        StopSmokingAnimation();

        if (cornerCharacterImage == null)
        {
            Debug.LogWarning("SmokingMiniGameCharacterUI: Corner Character Image fehlt.");
            return;
        }

        cornerCharacterImage.gameObject.SetActive(true);

        if (idleCornerSprite != null)
        {
            cornerCharacterImage.sprite = idleCornerSprite;
            cornerCharacterImage.SetNativeSize();
        }
    }

    public void PlaySmoking()
    {
        if (cornerCharacterImage == null)
        {
            Debug.LogWarning("SmokingMiniGameCharacterUI: Corner Character Image fehlt.");
            return;
        }

        if (smokingSprites == null || smokingSprites.Length == 0)
        {
            Debug.LogWarning("SmokingMiniGameCharacterUI: Keine Smoking Sprites gesetzt.");
            return;
        }

        cornerCharacterImage.gameObject.SetActive(true);

        if (smokingCoroutine != null)
        {
            StopCoroutine(smokingCoroutine);
        }

        smokingCoroutine = StartCoroutine(SmokingRoutine());
    }

    public void HideAllCharacterUI()
    {
        if (runCoroutine != null)
        {
            StopCoroutine(runCoroutine);
            runCoroutine = null;
        }

        if (smokingCoroutine != null)
        {
            StopCoroutine(smokingCoroutine);
            smokingCoroutine = null;
        }

        HideRunner();

        if (cornerCharacterImage != null)
        {
            cornerCharacterImage.gameObject.SetActive(false);
        }
    }

    private IEnumerator CountdownRunRoutine(float duration)
    {
        countdownRunnerImage.gameObject.SetActive(true);

        RectTransform runnerRect = countdownRunnerImage.rectTransform;
        runnerRect.anchoredPosition = runStartPosition;

        float elapsed = 0f;
        float frameTimer = 0f;
        int frameIndex = 0;

        if (walkSprites != null && walkSprites.Length > 0)
        {
            countdownRunnerImage.sprite = walkSprites[0];
            countdownRunnerImage.SetNativeSize();
        }

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            frameTimer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / duration);
            runnerRect.anchoredPosition = Vector2.Lerp(runStartPosition, runEndPosition, t);

            if (walkSprites != null && walkSprites.Length > 0 && walkFps > 0f)
            {
                float frameDuration = 1f / walkFps;

                if (frameTimer >= frameDuration)
                {
                    frameTimer = 0f;
                    frameIndex++;
                    if (frameIndex >= walkSprites.Length)
                    {
                        frameIndex = 0;
                    }

                    countdownRunnerImage.sprite = walkSprites[frameIndex];
                }
            }

            yield return null;
        }

        runnerRect.anchoredPosition = runEndPosition;
        HideRunner();
        runCoroutine = null;
    }

    private IEnumerator SmokingRoutine()
    {
        if (idleCornerSprite != null && cornerCharacterImage.sprite == null)
        {
            cornerCharacterImage.sprite = idleCornerSprite;
        }

        do
        {
            for (int i = 0; i < smokingSprites.Length; i++)
            {
                cornerCharacterImage.sprite = smokingSprites[i];
                cornerCharacterImage.SetNativeSize();
                yield return new WaitForSecondsRealtime(smokingFrameTime);
            }
        }
        while (loopSmoking);

        smokingCoroutine = null;
    }

    private void StopSmokingAnimation()
    {
        if (smokingCoroutine != null)
        {
            StopCoroutine(smokingCoroutine);
            smokingCoroutine = null;
        }
    }

    private void HideRunner()
    {
        if (countdownRunnerImage != null)
        {
            countdownRunnerImage.gameObject.SetActive(false);
        }
    }
}