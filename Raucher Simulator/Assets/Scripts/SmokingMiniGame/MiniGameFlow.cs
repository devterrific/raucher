using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class MiniGameFlow : MonoBehaviour
{
    public enum State { Countdown, WaitPaperClick, WaitFilterClick, FilterAiming, WaitTobaccoClick, Assemble, Results }

    [Header("UI")]
    [SerializeField] private CanvasGroup countdownPanel;
    [SerializeField] private Text countdownText;
    [SerializeField] private Text topHintText;
    [SerializeField] private Text scoreText;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private Text resultText;

    //  NEU: 18.12. - Für die Einbindung der einzelnen Bilder, beim richtigen Filterdrop
    [Header("Placement Banner (Sprites)")]
    [SerializeField] private Image placementBanner; // UI Image in Canvas
    [SerializeField] private Sprite bannerPerfect;
    [SerializeField] private Sprite bannerGood;
    [SerializeField] private Sprite bannerOkay;
    [SerializeField] private Sprite bannerFail;
    [SerializeField] private float bannerShowTime = 0.6f;

    //  NEU: 18.12. - Für die getrennte AudioSource 
    [Header("Audio")]
    [SerializeField] private AudioSource countdownAudioSource;
    [SerializeField] private AudioClip countdown321GoClip;
    [SerializeField, Range(0f, 1f)] private float countdownVolume = 1f;

    //  NEU: 18.12 - Für das "Hochzählen" im ResultPannel
    [Header("Results Count-Up")]
    [SerializeField] private float countUpDuration = 0.8f;  // Dauer der Zähl-Animation
    [SerializeField] private float totalCountUpDuration = 0.8f; // optional

    [Header("Zone Refs")]
    [SerializeField] private RectTransform targetZone;     // gesamtes Feld
    [SerializeField] private RectTransform zoneOkay;       // äußerer Bereich
    [SerializeField] private RectTransform zoneGood;       // mittlerer Bereich
    [SerializeField] private RectTransform zonePerfect;    // Kernbereich

    [Header("Packs & Anchors")]
    [SerializeField] private Button paperPackBtn;          // rechts
    [SerializeField] private Button tobaccoPackBtn;        // links
    [SerializeField] private Button filterPackBtn;         // oben
    [SerializeField] private RectTransform paperAnchor;    // unten Mitte
    [SerializeField] private RectTransform filterAnchor;   // über Papier

    [Header("Prefabs")]
    [SerializeField] private Image paperPrefab;
    [SerializeField] private Image filterPrefab;
    [SerializeField] private Image tobaccoPrefab;

    // NEU => 24.11
    [Header("Prefabs geöffnet")]
    [SerializeField] private Image paperPackOpenPrefab;
    [SerializeField] private Image tobaccoPackOpenPrefab;

    [Header("Config")]
    [SerializeField] private DifficultySettings settings;
    [SerializeField] private ScoreManager scoreManager;

    // NEU => 24.11
    [Header("Pack Intros")]
    [SerializeField] private PackFlyIn paperPackIntro;
    [SerializeField] private PackFlyIn filterPackIntro;
    [SerializeField] private PackFlyIn tobaccoPackIntro;

    private Image spawnedPaper;
    private FilterAimer filterAimer;
    private bool filterEvaluated;
    private int earnedPointsThisRun;
    private State currentState;

    // NEU => 24.11
    private bool paperPackOpened;
    private bool tobaccoPackOpened;
    private Image spawnedPaperPackOpen;
    private Image spawnedTobaccoPackOpen;


    // =================================================================================================
    //  1) Unity-Callbacks
    // =================================================================================================

    private void Awake()
    {
        resultPanel.SetActive(false);

        //  Startzustand: nur Papier-Pack klickbar
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

        SetZonesVisible(false);

        // NEU => 24.11
        paperPackOpened = false;
        tobaccoPackOpened = false;
        spawnedPaperPackOpen = null;
        spawnedTobaccoPackOpen = null;

        //  NEU: 18.12. - Damit der Banner nicht die ganzezeit Angezeigt wird
        if (placementBanner != null)
            placementBanner.gameObject.SetActive(false);
    }

    private IEnumerator Start()
    {
        // 1) Countdown
        yield return StartCoroutine(RunCountdown());

        // 2) Einflug aller Packs
        yield return StartCoroutine(PlayPackIntros());

        // 3) Score jetzt sichtbar machen
        scoreText.gameObject.SetActive(true);

        // 4) Jetzt darf der Spieler Papier anklicken
        EnterState(State.WaitPaperClick);
    }

    private void Update()
    {
        if (CurrentStateIsFilterAiming() && !filterEvaluated && Input.GetKeyDown(KeyCode.Space) && filterAimer != null)
        {
            filterEvaluated = true;

            // 1) Optischer Drop
            filterAimer.Drop();

            // 2) Scoring
            EvaluateFilter();

            // Wenn FailSequence läuft, gehen wir NICHT zum Tabak
            if (earnedPointsThisRun <= 0)
                return;

            // 2b) Zonen aus
            SetZonesVisible(false);

            // 3) Weiter zum Tabak
            EnterState(State.WaitTobaccoClick);
        }
    }


    private bool CurrentStateIsFilterAiming()
    {
        return currentState == State.FilterAiming;
    }


    // =================================================================================================
    //  2) State / Flow
    // =================================================================================================

    private IEnumerator RunCountdown()
    {
        SetHint("");
        countdownPanel.alpha = 1f;

        //  NEU: 18.12. - Countdown spielt nun über countdownAudioSource
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
        // STAGE 1: Packung öffnen
        if (!paperPackOpened)
        {
            paperPackOpened = true;

            // geschlossenes Packungsbild ausblenden (optional)
            var btnImg = paperPackBtn.GetComponent<Image>();
            if (btnImg != null) btnImg.enabled = false;

            // Offene Packung als Prefab über dem Button anzeigen
            if (spawnedPaperPackOpen == null && paperPackOpenPrefab != null)
            {
                spawnedPaperPackOpen = Instantiate(paperPackOpenPrefab, paperPackBtn.transform);

                var rt = spawnedPaperPackOpen.rectTransform;
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;

                // Wichtig: Open-Image soll keine Klicks blockieren
                spawnedPaperPackOpen.raycastTarget = false;

                // Open-Image wird TargetGraphic → Disabled-Tint funktioniert später
                paperPackBtn.targetGraphic = spawnedPaperPackOpen;
            }

            SetHint("Klicke die geöffnete Papierpackung erneut, um ein Blatt zu entnehmen.");
            return;
        }

        // STAGE 2: Papier entnehmen & platzieren
        paperPackBtn.interactable = false;

        spawnedPaper = Instantiate(paperPrefab, paperAnchor);
        spawnedPaper.rectTransform.anchoredPosition = Vector2.zero;

        EnterState(State.WaitFilterClick);
    }

    private void OnFilterPackClicked()
    {
        if (filterEvaluated) return;

        filterPackBtn.interactable = false;

        SetZonesVisible(true);

        EnterState(State.FilterAiming);
    }

    private void OnTobaccoClicked()
    {
        if (!filterEvaluated) return; // erst filtern

        // STAGE 1: Tabakbeutel öffnen
        if (!tobaccoPackOpened)
        {
            tobaccoPackOpened = true;

            // geschlossenes Beutelbild ausblenden (optional)
            var btnImg = tobaccoPackBtn.GetComponent<Image>();
            if (btnImg != null) btnImg.enabled = false;

            // Offener Tabakbeutel an gleicher Stelle anzeigen
            if (spawnedTobaccoPackOpen == null && tobaccoPackOpenPrefab != null)
            {
                spawnedTobaccoPackOpen = Instantiate(tobaccoPackOpenPrefab, tobaccoPackBtn.transform);

                var rt = spawnedTobaccoPackOpen.rectTransform;
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;

                // Wichtig: Open-Image soll keine Klicks blockieren
                spawnedTobaccoPackOpen.raycastTarget = false;

                // ✅ Open-Image wird TargetGraphic → Disabled-Tint funktioniert später
                tobaccoPackBtn.targetGraphic = spawnedTobaccoPackOpen;
            }

            SetHint("Klicke den geöffneten Tabakbeutel erneut, um den Tabak zu platzieren.");
            return;
        }

        // STAGE 2: Tabak platzieren
        tobaccoPackBtn.interactable = false;

        var tob = Instantiate(tobaccoPrefab, paperAnchor);
        tob.rectTransform.anchoredPosition = new Vector2(0f, 0.5f);

        EnterState(State.Assemble);

        // Zonen sicherheitshalber aus
        SetZonesVisible(false);
    }


    // =================================================================================================
    //  3) Gameplay-Logik
    // =================================================================================================

    private void StartFilterAiming()
    {
        var filterImg = Instantiate(filterPrefab, filterAnchor);
        var rt = filterImg.rectTransform;
        rt.anchoredPosition = Vector2.zero;

        filterAimer = filterImg.gameObject.AddComponent<FilterAimer>();

        float range = settings.movementRange; // Breite der Zielzone als Bewegungsbereich

        filterAimer.Init(
            filterAnchor,
            settings.filterSpeed,
            range,
            settings.rightToLeft,
            settings.loopMovement,
            OnFilterPassedLimits
        );

        filterEvaluated = false;
    }

    private void OnFilterPassedLimits()
    {
        if (filterEvaluated) return;

        earnedPointsThisRun = 0;
        scoreManager.Add(earnedPointsThisRun);
        UpdateScoreUI();
        filterEvaluated = true;

        // direkt zu Tabak
        EnterState(State.WaitTobaccoClick);

        // Zonen wieder verstecken
        SetZonesVisible(false);
    }

    private void EvaluateFilter()
    {
        // Filter Rect (Screen)
        Rect filterRect = GetScreenRect(filterAimer.Rect);

        // Zone Rects (Screen)
        Rect perfectRect = GetScreenRect(zonePerfect);
        Rect goodRect = GetScreenRect(zoneGood);
        Rect okayRect = GetScreenRect(zoneOkay);

        // Overlap (0..1) bezogen auf Filterfläche
        float oPerfect = Overlap01(filterRect, perfectRect);
        float oGood = Overlap01(filterRect, goodRect);
        float oOkay = Overlap01(filterRect, okayRect);

        // Zone mit höchstem Overlap gewinnt
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

        // Kein Overlap → Fail
        if (bestOverlap <= 0f)
        {
            earnedPointsThisRun = 0;
            StartCoroutine(FailSequence());
            return;
        }

        // Punkte: nur volle Punkte, wenn 100% in Zone (Overlap == 1)
        int points = Mathf.RoundToInt(basePoints * bestOverlap);

        bool isMaxHit = bestOverlap >= 0.9999f;
        if (isMaxHit)
            points = basePoints;

        earnedPointsThisRun = Mathf.Max(points, 0);
        scoreManager.Add(earnedPointsThisRun);
        UpdateScoreUI();

        // Banner nur zeigen, wenn Max-Punkte der Zone erreicht wurden
        if (isMaxHit)
            StartCoroutine(ShowBanner(banner, bannerShowTime));
    }

    private IEnumerator FailSequence()
    {
        filterEvaluated = true;

        // Zonen aus
        SetZonesVisible(false);

        // Filter fällt weit nach unten
        if (filterAimer != null)
            filterAimer.DropToVoid(900f, 0.35f);

        // Fail Banner
        yield return StartCoroutine(ShowBanner(bannerFail, bannerShowTime));

        // direkt Results (kein Tabak)
        EnterState(State.Results);
    }

    // =================================================================================================
    //  4) UI / Helpers
    // =================================================================================================

    private void SetHint(string msg) => topHintText.text = msg;

    private void SetZonesVisible(bool visible)
    {
        // TargetZone selbst (falls die auch eine Hintergrundfarbe hat)
        var tzImg = targetZone.GetComponent<Image>();
        if (tzImg != null) tzImg.enabled = visible;

        var okImg = zoneOkay.GetComponent<Image>();
        if (okImg != null) okImg.enabled = visible;

        var goodImg = zoneGood.GetComponent<Image>();
        if (goodImg != null) goodImg.enabled = visible;

        var perfImg = zonePerfect.GetComponent<Image>();
        if (perfImg != null) perfImg.enabled = visible;
    }

    //  NEU: 18.12. - Helper funktion für EvaluateFilter()
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


    // =================================================================================================
    //  Weitere bestehende Logik (Results / Pack Intros / Scene Buttons)
    // =================================================================================================

    private IEnumerator FinishAssemble()
    {
        yield return new WaitForSeconds(settings.assembleSeconds);
        EnterState(State.Results);
    }

    private void ShowResults()
    {
        resultPanel.SetActive(true);
        StopAllCoroutines(); // optional: verhindert doppelte CountUps
        StartCoroutine(ShowResultsRoutine());
    }

    private IEnumerator ShowResultsRoutine()
    {
        // Sicherheitswerte
        int runPoints = earnedPointsThisRun;
        int totalBefore = ScoreManager.GetTotal();

        // Erfolg: Total wird erhöht
        int totalAfter = totalBefore;
        if (runPoints > 0)
        {
            scoreManager.AddRunToTotal();
            totalAfter = ScoreManager.GetTotal();
        }

        // 1) Run Score hochzählen
        int displayedRun = 0;
        float t = 0f;

        while (t < countUpDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / countUpDuration);
            displayedRun = Mathf.RoundToInt(Mathf.Lerp(0, runPoints, k));

            // Text live updaten (Run)
            resultText.text = runPoints > 0
                ? $"Ergebnis: {displayedRun} Punkte\nGesamt-Highscore: {totalBefore}"
                : $"Mission Fail!\nGesamt-Highscore: {totalBefore}";

            yield return null;
        }

        // final sicher setzen
        displayedRun = runPoints;
        resultText.text = runPoints > 0
            ? $"Ergebnis: {displayedRun} Punkte\nGesamt-Highscore: {totalBefore}"
            : $"Mission Fail!\nGesamt-Highscore: {totalBefore}";

        // Wenn Mission Fail, keine Total-Animation nötig
        if (runPoints <= 0)
            yield break;

        // 2) Optional: Total hochzählen
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

        // final sicher setzen
        displayedTotal = totalAfter;
        resultText.text = $"Ergebnis: {runPoints} Punkte\nGesamt-Highscore: {displayedTotal}";
    }

    private IEnumerator PlayPackIntros()
    {
        // Alle drei gleichzeitig einfliegen lassen
        Coroutine c1 = null, c2 = null, c3 = null;

        if (paperPackIntro != null)
            c1 = StartCoroutine(paperPackIntro.Play());
        if (filterPackIntro != null)
            c2 = StartCoroutine(filterPackIntro.Play());
        if (tobaccoPackIntro != null)
            c3 = StartCoroutine(tobaccoPackIntro.Play());

        // Warten, bis alle fertig sind
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
