using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MiniGameFlow : MonoBehaviour
{
    public enum State { Countdown, WaitPaperClick, WaitFilterClick, FilterAiming, WaitTobaccoClick, Assemble, Results }

    private enum PlacementRating { None, Okay, Good, Perfect, Fail }

    [Header("UI")]
    [SerializeField] private CanvasGroup countdownPanel;
    [SerializeField] private Text countdownText;
    [SerializeField] private Text topHintText;
    [SerializeField] private Text scoreText;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private Text resultText;

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

    [Header("Fade Settings (NEU)")]
    [SerializeField] private float zoneFadeInDuration = 0.35f;
    [SerializeField] private float zoneFadeOutDuration = 0.2f;
    [SerializeField] private float filterFadeInDuration = 0.25f;

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
    [SerializeField] private ScoreManager scoreManager;

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

    // -----------------------------
    // Unity Callbacks
    // -----------------------------

    private void Awake()
    {
        resultPanel.SetActive(false);

        paperPackBtn.interactable = false;
        filterPackBtn.interactable = false;
        tobaccoPackBtn.interactable = false;
        scoreText.gameObject.SetActive(false);

        paperPackBtn.onClick.AddListener(OnPaperClicked);
        filterPackBtn.onClick.AddListener(OnFilterPackClicked);
        tobaccoPackBtn.onClick.AddListener(OnTobaccoClicked);

        scoreManager.ResetRun();
        UpdateScoreUI();

        countdownPanel.alpha = 0f;
        countdownPanel.gameObject.SetActive(true);

        // Zonen initial wirklich unsichtbar (ohne Pop)
        ForceZonesHidden();

        paperPackOpened = false;
        tobaccoPackOpened = false;
        spawnedPaperPackOpen = null;
        spawnedTobaccoPackOpen = null;

        if (placementBanner != null)
            placementBanner.gameObject.SetActive(false);
    }

    private IEnumerator Start()
    {
        yield return StartCoroutine(RunCountdown());
        yield return StartCoroutine(PlayPackIntros());

        scoreText.gameObject.SetActive(true);
        EnterState(State.WaitPaperClick);
    }

    private void Update()
    {
        if (CurrentStateIsFilterAiming() && !filterEvaluated && Input.GetKeyDown(KeyCode.Space) && filterAimer != null)
        {
            filterEvaluated = true;

            // 1) Drop
            filterAimer.Drop();

            // 2) Scoring + evtl. FailSequence
            EvaluateFilter();

            // Wenn FailSequence läuft → Results Flow
            if (earnedPointsThisRun <= 0)
                return;

            // 2b) Zonen aus (FadeOut)
            SetZonesVisible(false);

            // 3) Weiter zum Tabak
            EnterState(State.WaitTobaccoClick);
        }
    }

    // -----------------------------
    // State / Flow
    // -----------------------------

    private IEnumerator RunCountdown()
    {
        SetHint("");
        countdownPanel.alpha = 1f;

        if (countdownAudioSource != null && countdown321GoClip != null)
        {
            countdownAudioSource.Stop();
            countdownAudioSource.clip = countdown321GoClip;
            countdownAudioSource.volume = countdownVolume;
            countdownAudioSource.Play();
        }

        float t = settings.countdownSeconds;
        while (t > 0f)
        {
            countdownText.text = Mathf.CeilToInt(t).ToString();
            t -= Time.unscaledDeltaTime;
            yield return null;
        }

        countdownText.text = "GO!";
        yield return new WaitForSecondsRealtime(0.5f);
        countdownPanel.alpha = 0f;
        countdownPanel.gameObject.SetActive(false);
    }

    private void EnterState(State s)
    {
        currentState = s;

        switch (s)
        {
            case State.WaitPaperClick:
                SetHint("Klicke die Papierpackung (rechts), um ein Blatt zu entnehmen.");
                paperPackBtn.interactable = true;
                break;

            case State.WaitFilterClick:
                SetHint("Klicke die Filterpackung oben, um einen Filter zu nehmen.");
                filterPackBtn.interactable = true;
                break;

            case State.FilterAiming:
                SetHint("Platziere den Filter: Drücke LEERTASTE im richtigen Moment.");
                StartFilterAiming();
                break;

            case State.WaitTobaccoClick:
                SetHint("Klicke den Tabakbeutel (links), um Tabak hinzuzufügen.");
                tobaccoPackBtn.interactable = true;
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
        if (!paperPackOpened)
        {
            paperPackOpened = true;

            if (uiSfxSource != null && paperOpenClip != null)
                uiSfxSource.PlayOneShot(paperOpenClip, uiSfxVolume);

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

        paperPackBtn.interactable = false;

        spawnedPaper = Instantiate(paperPrefab, paperAnchor);
        spawnedPaper.rectTransform.anchoredPosition = Vector2.zero;

        EnterState(State.WaitFilterClick);
    }

    private void OnFilterPackClicked()
    {
        if (filterEvaluated) return;
        if (isFilterClickLocked) return;

        isFilterClickLocked = true;
        filterPackBtn.interactable = false;

        // Zonen erstmal aus (FadeOut)
        SetZonesVisible(false);

        // Zone Reveal nach Delay (sanft)
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
            yield return new WaitForSecondsRealtime(wait);

        EnterState(State.FilterAiming);
        isFilterClickLocked = false;
    }

    private IEnumerator RevealZonesAfterDelay()
    {
        yield return new WaitForSecondsRealtime(zoneRevealDelay);

        if (currentState == State.FilterAiming && !filterEvaluated)
            SetZonesVisible(true); // Fade-In
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
            yield return new WaitForSecondsRealtime(wait);

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

    // -----------------------------
    // Gameplay-Logik
    // -----------------------------

    private void StartFilterAiming()
    {
        var filterImg = Instantiate(filterPrefab, filterAnchor);
        var rt = filterImg.rectTransform;

        rt.anchoredPosition = Vector2.zero;

        // ✅ Filter Fade-In (sanft auftauchen)
        var filterCg = filterImg.GetComponent<CanvasGroup>();
        if (filterCg == null) filterCg = filterImg.gameObject.AddComponent<CanvasGroup>();
        filterCg.alpha = 0f;
        StartCoroutine(FadeCanvasGroup(filterCg, 0f, 1f, filterFadeInDuration));

        filterAimer = filterImg.gameObject.AddComponent<FilterAimer>();

        float range = settings.movementRange;

        filterAimer.Init(
            filterAnchor,
            settings.filterSpeed,
            range,
            settings.rightToLeft,
            settings.loopMovement,
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
        scoreManager.Add(earnedPointsThisRun);
        UpdateScoreUI();
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
        int basePoints = settings.pointsPerfect;
        Sprite banner = bannerPerfect;

        if (oGood > bestOverlap)
        {
            bestOverlap = oGood;
            basePoints = settings.pointsGood;
            banner = bannerGood;
        }
        if (oOkay > bestOverlap)
        {
            bestOverlap = oOkay;
            basePoints = settings.pointsOkay;
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
        scoreManager.Add(earnedPointsThisRun);
        UpdateScoreUI();

        if (basePoints == settings.pointsPerfect) lastPlacement = PlacementRating.Perfect;
        else if (basePoints == settings.pointsGood) lastPlacement = PlacementRating.Good;
        else lastPlacement = PlacementRating.Okay;

        if (isMaxHit)
            StartCoroutine(ShowBanner(banner, bannerShowTime));
    }

    private IEnumerator FailSequence()
    {
        if (filterAimer != null)
            filterAimer.FailFall();

        if (bannerFail != null)
            yield return StartCoroutine(ShowBanner(bannerFail, bannerShowTime));

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
        yield return new WaitForSeconds(settings.assembleSeconds);
        EnterState(State.Results);
    }

    private void ShowResults()
    {
        resultPanel.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(ShowResultsRoutine());
    }

    private IEnumerator ShowResultsRoutine()
    {
        int runPoints = earnedPointsThisRun;
        int totalBefore = ScoreManager.GetTotal();

        int totalAfter = totalBefore;
        if (runPoints > 0)
        {
            scoreManager.AddRunToTotal();
            totalAfter = ScoreManager.GetTotal();
        }

        int displayedRun = 0;
        float t = 0f;

        while (t < countUpDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / countUpDuration);
            displayedRun = Mathf.RoundToInt(Mathf.Lerp(0, runPoints, k));

            resultText.text = runPoints > 0
                ? $"Ergebnis: {displayedRun} Punkte\nGesamt-Highscore: {totalBefore}"
                : $"Mission Fail!\nGesamt-Highscore: {totalBefore}";

            yield return null;
        }

        displayedRun = runPoints;
        resultText.text = runPoints > 0
            ? $"Ergebnis: {displayedRun} Punkte\nGesamt-Highscore: {totalBefore}"
            : $"Mission Fail!\nGesamt-Highscore: {totalBefore}";

        if (runPoints <= 0)
            yield break;

        int displayedTotal = totalBefore;
        t = 0f;

        while (t < totalCountUpDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / totalCountUpDuration);
            displayedTotal = Mathf.RoundToInt(Mathf.Lerp(totalBefore, totalAfter, k));

            resultText.text = $"Ergebnis: {runPoints} Punkte\nGesamt-Highscore: {displayedTotal}";
            yield return null;
        }

        displayedTotal = totalAfter;
        resultText.text = $"Ergebnis: {runPoints} Punkte\nGesamt-Highscore: {displayedTotal}";
    }

    // -----------------------------
    // UI / Helpers
    // -----------------------------

    private void SetHint(string msg) => topHintText.text = msg;

    /// <summary>
    /// ✅ Fade-In/Fade-Out für TargetZone + alle Zonen (kein „plopp“)
    /// </summary>
    private void SetZonesVisible(bool visible)
    {
        FadeZone(targetZone, visible);
        FadeZone(zoneOkay, visible);
        FadeZone(zoneGood, visible);
        FadeZone(zonePerfect, visible);
    }

    /// <summary>
    /// Für Awake(): sofort unsichtbar ohne Coroutine
    /// </summary>
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
            StartCoroutine(FadeCanvasGroup(cg, cg.alpha, 1f, zoneFadeInDuration));
        }
        else
        {
            // Fade out → dann deaktivieren
            StartCoroutine(FadeCanvasGroup(cg, cg.alpha, 0f, zoneFadeOutDuration, () =>
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
            yield return null;
    }

    public void OnBtnRetry() => SceneManager.LoadScene("SmokingMinigame");
    public void OnBtnWeiter() => SceneManager.LoadScene("Vorraum_Placeholder");

    private void UpdateScoreUI() => scoreText.text = $"Score: {scoreManager.RunScore}";
}
