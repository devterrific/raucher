using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneBannerManager : MonoBehaviour
{
    [System.Serializable]
    public class SceneBannerEntry
    {
        public string sceneName;
        public Sprite bannerSprite;
    }

    public static SceneBannerManager Instance;

    [Header("UI References")]
    [SerializeField] private Image bannerImage;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform bannerRectTransform;

    [Header("Scene Banner Mapping")]
    [SerializeField] private List<SceneBannerEntry> sceneBanners = new List<SceneBannerEntry>();

    [Header("Timing")]
    [SerializeField] private float fadeInDuration = 0.45f;
    [SerializeField] private float visibleDuration = 1.5f;
    [SerializeField] private float fadeOutDuration = 0.8f;

    [Header("Slide Settings")]
    [SerializeField] private float startOffsetX = -120f;
    [SerializeField] private float endOffsetX = 80f;

    private Coroutine currentRoutine;
    private readonly Dictionary<string, Sprite> bannerLookup = new Dictionary<string, Sprite>();

    private Vector2 originalAnchoredPosition;
    private bool isPaused;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildLookup();

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        if (bannerRectTransform != null)
            originalAnchoredPosition = bannerRectTransform.anchoredPosition;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        PauseMenuManager.OnPauseStateChanged += HandlePauseStateChanged;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        PauseMenuManager.OnPauseStateChanged -= HandlePauseStateChanged;
    }

    private void HandlePauseStateChanged(bool paused)
    {
        isPaused = paused;
    }

    private IEnumerator WaitWhilePaused()
    {
        while (isPaused)
        {
            yield return null;
        }
    }

    private void BuildLookup()
    {
        bannerLookup.Clear();

        foreach (SceneBannerEntry entry in sceneBanners)
        {
            if (string.IsNullOrWhiteSpace(entry.sceneName) || entry.bannerSprite == null)
                continue;

            if (!bannerLookup.ContainsKey(entry.sceneName))
                bannerLookup.Add(entry.sceneName, entry.bannerSprite);
            else
                Debug.LogWarning($"SceneBannerManager: Doppelte Szene im Mapping gefunden: {entry.sceneName}");
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ShowBannerForScene(scene.name);
    }

    public void ShowBannerForScene(string sceneName)
    {
        if (bannerImage == null || canvasGroup == null || bannerRectTransform == null)
        {
            Debug.LogWarning("SceneBannerManager: UI References fehlen.");
            return;
        }

        if (!bannerLookup.TryGetValue(sceneName, out Sprite sprite))
        {
            HideImmediately();
            return;
        }

        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
            currentRoutine = null;
        }

        currentRoutine = StartCoroutine(ShowBannerRoutine(sprite));
    }

    private IEnumerator ShowBannerRoutine(Sprite sprite)
    {
        bannerImage.sprite = sprite;
        yield return StartCoroutine(PlayBannerRoutine());
        currentRoutine = null;
    }

    private IEnumerator PlayBannerRoutine()
    {
        Vector2 startPosition = originalAnchoredPosition + new Vector2(startOffsetX, 0f);
        Vector2 centerPosition = originalAnchoredPosition;
        Vector2 endPosition = originalAnchoredPosition + new Vector2(endOffsetX, 0f);

        canvasGroup.alpha = 0f;
        bannerRectTransform.anchoredPosition = startPosition;

        float time = 0f;
        while (time < fadeInDuration)
        {
            if (isPaused)
            {
                yield return StartCoroutine(WaitWhilePaused());
            }

            time += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(time / fadeInDuration);
            float easedT = Mathf.SmoothStep(0f, 1f, t);

            canvasGroup.alpha = easedT;
            bannerRectTransform.anchoredPosition = Vector2.Lerp(startPosition, centerPosition, easedT);

            yield return null;
        }

        canvasGroup.alpha = 1f;
        bannerRectTransform.anchoredPosition = centerPosition;

        float visibleTime = 0f;
        while (visibleTime < visibleDuration)
        {
            if (isPaused)
            {
                yield return StartCoroutine(WaitWhilePaused());
            }

            visibleTime += Time.unscaledDeltaTime;
            yield return null;
        }

        time = 0f;
        while (time < fadeOutDuration)
        {
            if (isPaused)
            {
                yield return StartCoroutine(WaitWhilePaused());
            }

            time += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(time / fadeOutDuration);
            float easedT = Mathf.SmoothStep(0f, 1f, t);

            canvasGroup.alpha = 1f - easedT;
            bannerRectTransform.anchoredPosition = Vector2.Lerp(centerPosition, endPosition, easedT);

            yield return null;
        }

        canvasGroup.alpha = 0f;
        bannerRectTransform.anchoredPosition = originalAnchoredPosition;
    }

    public void HideImmediately()
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
            currentRoutine = null;
        }

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        if (bannerRectTransform != null)
            bannerRectTransform.anchoredPosition = originalAnchoredPosition;
    }
}