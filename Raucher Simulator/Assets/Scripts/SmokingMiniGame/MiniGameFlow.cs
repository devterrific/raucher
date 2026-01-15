using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MiniGameFlow : MonoBehaviour
{
    public enum State { Countdown, WaitPaperClick, WaitFilterClick, FilterAiming, WaitTobaccoClick, Assemble, Results }

    private enum PlacementRating { None, Okay, Good, Perfect, Fail }

    [System.Serializable]
    private struct FadeSettings
    {
        [Header("Zones")]
        public float zoneFadeIn;
        public float zoneFadeOut;

        [Header("Filter")]
        public float filterFadeIn;

        [Header("Paper")]
        public float paperFadeIn;
    }

    [Header("Fade Settings")]
    [SerializeField]
    private FadeSettings fades = new FadeSettings
    {
        zoneFadeIn = 0.35f,
        zoneFadeOut = 0.2f,
        filterFadeIn = 0.25f,
        paperFadeIn = 0.25f
    };

    [Header("UI")]
    [SerializeField] private CanvasGroup countdownPanel;
    [SerializeField] private Text countdownText;
    [SerializeField] private Text topHintText;

    [SerializeField] private GameObject resultPanel;
    [SerializeField] private Text resultText;

    [Header("Result Panel Button (nur Weiter)")]
    [SerializeField] private Button continueButton;

    [Header("Scene Flow")]
    [SerializeField] private string continueSceneName = "Vorraum_Placeholder";

    [Header("Placement Banner (Sprites)")]
    [SerializeField] private Image placementBanner;
    [SerializeField] private Sprite bannerPerfect;
    [SerializeField] private Sprite bannerGood;
    [SerializeField] private Sprite bannerOkay;
    [SerializeField] private Sprite bannerFail;
    [SerializeField] private float bannerShowTime = 0.6f;

    [Header("Audio")]
    [SerializeField] private AudioSource countdownAudioSource;
    [SerializeField] private AudioClip countdown321GoClip;
    [SerializeField, Range(0f, 1f)] private float countdownVolume = 1f;

    [Header("UI Interaction Sounds")]
    [SerializeField] private AudioSource uiSfxSource;
    [SerializeField] private AudioClip paperOpenClip;
    [SerializeField] private AudioClip filterPickClip;
    [SerializeField] private AudioClip tobaccoOpenClip;
    [SerializeField, Range(0f, 1f)] private float uiSfxVolume = 0.9f;

    [Header("Results Count-Up")]
    [SerializeField] private float countUpDuration = 0.8f;
    [SerializeField] private float totalCountUpDuration = 0.8f;

    [Header("Zone Refs")]
    [SerializeField] private RectTransform targetZone;
    [SerializeField] private RectTransform zoneOkay;
    [SerializeField] private RectTransform zoneGood;
    [SerializeField] private RectTransform zonePerfect;

    [Header("Filter Timing")]
    [SerializeField] private float zoneRevealDelay = 5f;

    [Header("Packs & Anchors")]
    [SerializeField] private Button paperPackBtn;
    [SerializeField] private Button tobaccoPackBtn;
    [SerializeField] private Button filterPackBtn;
    [SerializeField] private RectTransform paperAnchor;
    [SerializeField] private RectTransform filterAnchor;

    [Header("Prefabs")]
    [SerializeField] private Image paperPrefab;
    [SerializeField] private Image filterPrefab;

    [Header("Tobacco Prefabs (Length by Rating)")]
    [SerializeField] private Image tobaccoPrefab;
    [SerializeField] private Image tobaccoPrefabOkay;
    [SerializeField] private Image tobaccoPrefabGood;
    [SerializeField] private Image tobaccoPrefabPerfect;

    [Header("Prefabs geöffnet")]
    [SerializeField] private Image paperPackOpenPrefab;
    [SerializeField] private Image tobaccoPackOpenPrefab;

    [Header("Config")]
    [SerializeField] private DifficultySettings settings;

    [Header("Pack Intros")]
    [SerializeField] private PackFlyIn paperPackIntro;
    [SerializeField] private PackFlyIn filterPackIntro;
    [SerializeField] private PackFlyIn tobaccoPackIntro;

    [Header("Tobacco Drag&Drop")]
    [SerializeField] private TobaccoDragAndDrop tobaccoDrag;
    [SerializeField] private Canvas mainCanvas;

    private Image spawnedPaper;
    private FilterAimer filterAimer;
    private bool filterEvaluated;
    private int earnedPointsThisRun;
    private State currentState;

    private bool paperPackOpened;
    private bool tobaccoPackOpened;
    private Image spawnedPaperPackOpen;
    private Image spawnedTobaccoPackOpen;

    private PlacementRating lastPlacement = PlacementRating.None;
    private bool isFilterClickLocked = false;
    private bool isTobaccoClickLocked = false;

    // Verhindert doppelte Punktevergabe (falls ShowResults mehrfach getriggert wird)
    private bool hasGivenScoreThisRun = false;

    private void Awake()
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }

        if (paperPackBtn != null) paperPackBtn.interactable = false;
        if (filterPackBtn != null) filterPackBtn.interactable = false;
        if (tobaccoPackBtn != null) tobaccoPackBtn.interactable = false;

        if (paperPackBtn != null) paperPackBtn.onClick.AddListener(OnPaperClicked);
        if (filterPackBtn != null) filterPackBtn.onClick.AddListener(OnFilterPackClicked);
        if (tobaccoPackBtn != null) tobaccoPackBtn.onClick.AddListener(OnTobaccoClicked);

        // Weiter-Button Listener per Code (Inspector-OnClick bitte leer lassen!)
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(ContinueAfterMiniGame);
        }

        earnedPointsThisRun = 0;
        hasGivenScoreThisRun = false;

        if (countdownPanel != null)
        {
            countdownPanel.alpha = 0f;
            countdownPanel.gameObject.SetActive(true);
        }

        // WICHTIG: Zonen initial wirklich unsichtbar
        ForceZonesHidden();

        paperPackOpened = false;
        tobaccoPackOpened = false;
        spawnedPaperPackOpen = null;
        spawnedTobaccoPackOpen = null;

        if (placementBanner != null)
        {
            placementBanner.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (paperPackBtn != null) paperPackBtn.onClick.RemoveListener(OnPaperClicked);
        if (filterPackBtn != null) filterPackBtn.onClick.RemoveListener(OnFilterPackClicked);
        if (tobaccoPackBtn != null) tobaccoPackBtn.onClick.RemoveListener(OnTobaccoClicked);

        if (continueButton != null) continueButton.onClick.RemoveListener(ContinueAfterMiniGame);
    }

    private IEnumerator Start()
    {
        yield return StartCoroutine(RunCountdown());

        // WICHTIG: Packs sind über PackFlyIn zunächst unsichtbar -> PlayPackIntros macht sie sichtbar.
        yield return StartCoroutine(PlayPackIntros());

        EnterState(State.WaitPaperClick);
    }

    private void Update()
    {
        if (CurrentStateIsFilterAiming() && !filterEvaluated && Input.GetKeyDown(KeyCode.Space) && filterAimer != null)
        {
            filterEvaluated = true;

            // Drop Animation
            filterAimer.Drop();

            // Punkte berechnen (oder Fail)
            EvaluateFilter();

            // Bei Fail geht es direkt in Results
            if (earnedPointsThisRun <= 0)
            {
                return;
            }

            // Zonen ausblenden
            SetZonesVisible(false);

            // Weiter zum Tabak
            EnterState(State.WaitTobaccoClick);
        }
    }

    private IEnumerator RunCountdown()
    {
        SetHint("");

        if (countdownPanel != null)
        {
            countdownPanel.alpha = 1f;
        }

        if (countdownAudioSource != null && countdown321GoClip != null)
        {
            countdownAudioSource.Stop();
            countdownAudioSource.clip = countdown321GoClip;
            countdownAudioSource.volume = countdownVolume;
            countdownAudioSource.Play();
        }

        float t = settings != null ? settings.countdownSeconds : 3f;

        while (t > 0f)
        {
            if (countdownText != null)
            {
                countdownText.text = Mathf.CeilToInt(t).ToString();
            }

            t -= Time.unscaledDeltaTime;
            yield return null;
        }

        if (countdownText != null)
        {
            countdownText.text = "GO!";
        }

        yield return new WaitForSecondsRealtime(0.5f);

        if (countdownPanel != null)
        {
            countdownPanel.alpha = 0f;
            countdownPanel.gameObject.SetActive(false);
        }
    }

    private void EnterState(State s)
    {
        currentState = s;

        switch (s)
        {
            case State.WaitPaperClick:
                SetHint("Klicke die Papierpackung (rechts), um ein Blatt zu entnehmen.");
                if (paperPackBtn != null) paperPackBtn.interactable = true;
                break;

            case State.WaitFilterClick:
                SetHint("Klicke die Filterpackung oben, um einen Filter zu nehmen.");
                if (filterPackBtn != null) filterPackBtn.interactable = true;
                break;

            case State.FilterAiming:
                SetHint("Platziere den Filter: Drücke LEERTASTE im richtigen Moment.");
                StartFilterAiming();
                break;

            case State.WaitTobaccoClick:
                SetHint("Klicke den Tabakbeutel (links), um Tabak hinzuzufügen.");
                if (tobaccoPackBtn != null) tobaccoPackBtn.interactable = true;
                break;

            case State.Assemble:
                SetHint("Zigarette wird fertiggestellt…");
                StartCoroutine(FinishAssemble());
                break;

            case State.Results:
                ShowResults();
                break;
        }
    }

    private void OnPaperClicked()
    {
        // STAGE 1: Packung öffnen
        if (!paperPackOpened)
        {
            paperPackOpened = true;

            var btnImg = paperPackBtn.GetComponent<Image>();
            if (btnImg != null) btnImg.enabled = false;

            if (spawnedPaperPackOpen == null && paperPackOpenPrefab != null)
            {
                spawnedPaperPackOpen = Instantiate(paperPackOpenPrefab, paperPackBtn.transform);

                var rt = spawnedPaperPackOpen.rectTransform;
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;

                spawnedPaperPackOpen.raycastTarget = false;
                paperPackBtn.targetGraphic = spawnedPaperPackOpen;
            }

            SetHint("Klicke die geöffnete Papierpackung erneut, um ein Blatt zu entnehmen.");
            return;
        }

        // STAGE 2: Paper nehmen (Sound → dann Paper)
        paperPackBtn.interactable = false;
        SetHint("Papier wird vorbereitet…");

        StartCoroutine(SpawnPaperAfterSound());
    }

    private void OnFilterPackClicked()
    {
        if (filterEvaluated) return;
        if (isFilterClickLocked) return;

        isFilterClickLocked = true;

        if (filterPackBtn != null)
        {
            filterPackBtn.interactable = false;
        }

        // Zonen erstmal aus
        SetZonesVisible(false);

        // Zone Reveal nach Delay
        StartCoroutine(RevealZonesAfterDelay());

        // Filter erst nach Sound spawnen
        StartCoroutine(FilterSpawnAfterSound());
    }

    private IEnumerator FilterSpawnAfterSound()
    {
        float wait = 0f;

        if (uiSfxSource != null && filterPickClip != null)
        {
            uiSfxSource.PlayOneShot(filterPickClip, uiSfxVolume);
            wait = filterPickClip.length;
        }

        if (wait > 0f)
        {
            yield return new WaitForSecondsRealtime(wait);
        }

        EnterState(State.FilterAiming);
        isFilterClickLocked = false;
    }

    private IEnumerator RevealZonesAfterDelay()
    {
        yield return new WaitForSecondsRealtime(zoneRevealDelay);

        if (currentState == State.FilterAiming && !filterEvaluated)
        {
            SetZonesVisible(true);
        }
    }

    private void OnTobaccoClicked()
    {
        if (!filterEvaluated) return;
        if (isTobaccoClickLocked) return;

        if (!tobaccoPackOpened)
        {
            tobaccoPackOpened = true;

            isTobaccoClickLocked = true;

            float wait = 0f;
            if (uiSfxSource != null && tobaccoOpenClip != null)
            {
                uiSfxSource.PlayOneShot(tobaccoOpenClip, uiSfxVolume);
                wait = tobaccoOpenClip.length;
            }

            StartCoroutine(OpenTobaccoAfterSound(wait));
            return;
        }

        tobaccoPackBtn.interactable = false;

        if (tobaccoDrag != null && spawnedPaper != null && mainCanvas != null)
        {
            Image chosenTobacco = GetTobaccoPrefabByRating();

            SetHint("Platziere den Tabak auf dem Papier (Linksklick zum Ablegen).");

            tobaccoDrag.BeginDrag(
                mainCanvas,
                spawnedPaper.rectTransform,
                chosenTobacco,
                () =>
                {
                    EnterState(State.Assemble);
                    SetZonesVisible(false);
                }
            );
        }
        else
        {
            Debug.LogWarning("TobaccoDrag/MainCanvas/spawnedPaper fehlt – fallback zu Assemble.");
            EnterState(State.Assemble);
        }
    }

    private IEnumerator OpenTobaccoAfterSound(float wait)
    {
        if (wait > 0f)
        {
            yield return new WaitForSecondsRealtime(wait);
        }

        var btnImg = tobaccoPackBtn.GetComponent<Image>();
        if (btnImg != null) btnImg.enabled = false;

        if (spawnedTobaccoPackOpen == null && tobaccoPackOpenPrefab != null)
        {
            spawnedTobaccoPackOpen = Instantiate(tobaccoPackOpenPrefab, tobaccoPackBtn.transform);

            var rt = spawnedTobaccoPackOpen.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;

            spawnedTobaccoPackOpen.raycastTarget = false;
            tobaccoPackBtn.targetGraphic = spawnedTobaccoPackOpen;
        }

        SetHint("Klicke den geöffneten Tabakbeutel erneut, um den Tabak zu platzieren.");
        isTobaccoClickLocked = false;
    }

    private bool CurrentStateIsFilterAiming()
    {
        return currentState == State.FilterAiming;
    }

    private void StartFilterAiming()
    {
        var filterImg = Instantiate(filterPrefab, filterAnchor);
        var rt = filterImg.rectTransform;
        rt.anchoredPosition = Vector2.zero;

        // Filter Fade-In
        var filterCg = filterImg.GetComponent<CanvasGroup>();
        if (filterCg == null) filterCg = filterImg.gameObject.AddComponent<CanvasGroup>();
        filterCg.alpha = 0f;
        StartCoroutine(FadeCanvasGroup(filterCg, 0f, 1f, fades.filterFadeIn));

        filterAimer = filterImg.gameObject.AddComponent<FilterAimer>();

        float range = settings != null ? settings.movementRange : 800f;

        filterAimer.Init(
            filterAnchor,
            settings != null ? settings.filterSpeed : 500f,
            range,
            settings != null && settings.rightToLeft,
            settings == null || settings.loopMovement,
            OnFilterPassedLimits
        );

        filterEvaluated = false;
        lastPlacement = PlacementRating.None;
    }

    private void OnFilterPassedLimits()
    {
        if (filterEvaluated) return;

        earnedPointsThisRun = 0;
        lastPlacement = PlacementRating.Fail;
        filterEvaluated = true;

        SetZonesVisible(false);
        StartCoroutine(FailSequence());
    }

    private void EvaluateFilter()
    {
        lastPlacement = PlacementRating.Fail;

        Rect filterRect = GetScreenRect(filterAimer.Rect);

        Rect perfectRect = GetScreenRect(zonePerfect);
        Rect goodRect = GetScreenRect(zoneGood);
        Rect okayRect = GetScreenRect(zoneOkay);

        float oPerfect = Overlap01(filterRect, perfectRect);
        float oGood = Overlap01(filterRect, goodRect);
        float oOkay = Overlap01(filterRect, okayRect);

        float bestOverlap = oPerfect;
        int basePoints = settings != null ? settings.pointsPerfect : 30;
        Sprite banner = bannerPerfect;

        if (oGood > bestOverlap)
        {
            bestOverlap = oGood;
            basePoints = settings != null ? settings.pointsGood : 20;
            banner = bannerGood;
        }
        if (oOkay > bestOverlap)
        {
            bestOverlap = oOkay;
            basePoints = settings != null ? settings.pointsOkay : 10;
            banner = bannerOkay;
        }

        if (bestOverlap <= 0f)
        {
            earnedPointsThisRun = 0;
            lastPlacement = PlacementRating.Fail;
            StartCoroutine(FailSequence());
            return;
        }

        int points = Mathf.RoundToInt(basePoints * bestOverlap);

        bool isMaxHit = bestOverlap >= 0.999f;
        if (isMaxHit) points = basePoints;

        earnedPointsThisRun = Mathf.Max(points, 0);

        if (basePoints == (settings != null ? settings.pointsPerfect : 30)) lastPlacement = PlacementRating.Perfect;
        else if (basePoints == (settings != null ? settings.pointsGood : 20)) lastPlacement = PlacementRating.Good;
        else lastPlacement = PlacementRating.Okay;

        if (isMaxHit)
        {
            StartCoroutine(ShowBanner(banner, bannerShowTime));
        }
    }

    private IEnumerator FailSequence()
    {
        if (filterAimer != null)
        {
            filterAimer.FailFall();
        }

        if (bannerFail != null)
        {
            yield return StartCoroutine(ShowBanner(bannerFail, bannerShowTime));
        }

        EnterState(State.Results);
    }

    private Image GetTobaccoPrefabByRating()
    {
        switch (lastPlacement)
        {
            case PlacementRating.Perfect:
                return tobaccoPrefabPerfect != null ? tobaccoPrefabPerfect : tobaccoPrefab;
            case PlacementRating.Good:
                return tobaccoPrefabGood != null ? tobaccoPrefabGood : tobaccoPrefab;
            case PlacementRating.Okay:
                return tobaccoPrefabOkay != null ? tobaccoPrefabOkay : tobaccoPrefab;
            default:
                return tobaccoPrefabOkay != null ? tobaccoPrefabOkay : tobaccoPrefab;
        }
    }

    private IEnumerator FinishAssemble()
    {
        float assembleSeconds = settings != null ? settings.assembleSeconds : 0.6f;
        yield return new WaitForSeconds(assembleSeconds);
        EnterState(State.Results);
    }

    private void ShowResults()
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
        }

        StopAllCoroutines();
        StartCoroutine(ShowResultsRoutine());
    }

    private IEnumerator ShowResultsRoutine()
    {
        int runPoints = earnedPointsThisRun;

        int totalBefore = ScoreService.GetCurrentScore();

        if (runPoints > 0 && hasGivenScoreThisRun == false)
        {
            ScoreService.AddPoints(runPoints, "SmokingMiniGame Ergebnis");
            hasGivenScoreThisRun = true;
        }

        int totalAfter = ScoreService.GetCurrentScore();

        // Run Count-Up
        int displayedRun = 0;
        float t = 0f;

        while (t < countUpDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / countUpDuration);
            displayedRun = Mathf.RoundToInt(Mathf.Lerp(0, runPoints, k));

            if (resultText != null)
            {
                resultText.text = runPoints > 0
                    ? "Ergebnis: " + displayedRun + " Punkte\nGesamt-Score: " + totalBefore
                    : "Mission Fail!\nGesamt-Score: " + totalBefore;
            }

            yield return null;
        }

        if (resultText != null)
        {
            resultText.text = runPoints > 0
                ? "Ergebnis: " + runPoints + " Punkte\nGesamt-Score: " + totalBefore
                : "Mission Fail!\nGesamt-Score: " + totalBefore;
        }

        if (runPoints <= 0)
        {
            yield break;
        }

        // Total Count-Up
        int displayedTotal = totalBefore;
        t = 0f;

        while (t < totalCountUpDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / totalCountUpDuration);
            displayedTotal = Mathf.RoundToInt(Mathf.Lerp(totalBefore, totalAfter, k));

            if (resultText != null)
            {
                resultText.text = "Ergebnis: " + runPoints + " Punkte\nGesamt-Score: " + displayedTotal;
            }

            yield return null;
        }

        if (resultText != null)
        {
            resultText.text = "Ergebnis: " + runPoints + " Punkte\nGesamt-Score: " + totalAfter;
        }
    }

    private void ContinueAfterMiniGame()
    {
        if (string.IsNullOrWhiteSpace(continueSceneName))
        {
            Debug.LogError("MiniGameFlow: continueSceneName ist nicht gesetzt.");
            return;
        }

        SceneManager.LoadScene(continueSceneName);
    }

    private void SetHint(string msg)
    {
        if (topHintText != null)
        {
            topHintText.text = msg;
        }
    }

    private void SetZonesVisible(bool visible)
    {
        FadeZone(targetZone, visible);
        FadeZone(zoneOkay, visible);
        FadeZone(zoneGood, visible);
        FadeZone(zonePerfect, visible);
    }

    private void ForceZonesHidden()
    {
        ForceZoneHidden(targetZone);
        ForceZoneHidden(zoneOkay);
        ForceZoneHidden(zoneGood);
        ForceZoneHidden(zonePerfect);
    }

    private void ForceZoneHidden(RectTransform zone)
    {
        if (zone == null) return;

        var cg = zone.GetComponent<CanvasGroup>();
        if (cg == null) cg = zone.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        zone.gameObject.SetActive(false);
    }

    private void FadeZone(RectTransform zone, bool show)
    {
        if (zone == null) return;

        var cg = zone.GetComponent<CanvasGroup>();
        if (cg == null) cg = zone.gameObject.AddComponent<CanvasGroup>();

        if (show)
        {
            zone.gameObject.SetActive(true);
            StartCoroutine(FadeCanvasGroup(cg, cg.alpha, 1f, fades.zoneFadeIn));
        }
        else
        {
            StartCoroutine(FadeCanvasGroup(cg, cg.alpha, 0f, fades.zoneFadeOut, () =>
            {
                zone.gameObject.SetActive(false);
            }));
        }
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration, System.Action onComplete = null)
    {
        if (cg == null) yield break;

        float t = 0f;
        cg.alpha = from;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duration);
            cg.alpha = Mathf.Lerp(from, to, k);
            yield return null;
        }

        cg.alpha = to;
        onComplete?.Invoke();
    }

    private Rect GetScreenRect(RectTransform rt)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);

        Vector2 min = RectTransformUtility.WorldToScreenPoint(null, corners[0]);
        Vector2 max = RectTransformUtility.WorldToScreenPoint(null, corners[2]);

        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    private float Overlap01(Rect a, Rect b)
    {
        float xMin = Mathf.Max(a.xMin, b.xMin);
        float yMin = Mathf.Max(a.yMin, b.yMin);
        float xMax = Mathf.Min(a.xMax, b.xMax);
        float yMax = Mathf.Min(a.yMax, b.yMax);

        float w = xMax - xMin;
        float h = yMax - yMin;

        if (w <= 0f || h <= 0f) return 0f;

        float interArea = w * h;
        float aArea = a.width * a.height;
        if (aArea <= 0f) return 0f;

        return Mathf.Clamp01(interArea / aArea);
    }

    private IEnumerator ShowBanner(Sprite sprite, float time)
    {
        if (placementBanner == null || sprite == null) yield break;

        placementBanner.sprite = sprite;
        placementBanner.gameObject.SetActive(true);

        yield return new WaitForSecondsRealtime(time);

        placementBanner.gameObject.SetActive(false);
    }

    private IEnumerator PlayPackIntros()
    {
        if (paperPackIntro != null) StartCoroutine(paperPackIntro.Play());
        if (filterPackIntro != null) StartCoroutine(filterPackIntro.Play());
        if (tobaccoPackIntro != null) StartCoroutine(tobaccoPackIntro.Play());

        bool AnyPlaying()
        {
            return (paperPackIntro != null && paperPackIntro.IsPlaying) ||
                   (filterPackIntro != null && filterPackIntro.IsPlaying) ||
                   (tobaccoPackIntro != null && tobaccoPackIntro.IsPlaying);
        }

        while (AnyPlaying())
        {
            yield return null;
        }
    }

    private IEnumerator SpawnPaperAfterSound()
    {
        float waitTime = 0f;

        if (uiSfxSource != null && paperOpenClip != null)
        {
            uiSfxSource.PlayOneShot(paperOpenClip, uiSfxVolume);
            waitTime = paperOpenClip.length;
        }

        if (waitTime > 0f)
        {
            yield return new WaitForSecondsRealtime(waitTime);
        }

        spawnedPaper = Instantiate(paperPrefab, paperAnchor);
        spawnedPaper.rectTransform.anchoredPosition = Vector2.zero;

        var cg = spawnedPaper.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = spawnedPaper.gameObject.AddComponent<CanvasGroup>();
        }

        cg.alpha = 0f;
        yield return StartCoroutine(FadeCanvasGroup(cg, 0f, 1f, fades.paperFadeIn));

        EnterState(State.WaitFilterClick);
    }
}