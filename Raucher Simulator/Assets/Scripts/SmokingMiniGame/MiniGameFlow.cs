using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

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

    //  NEU: 18.12. - Für die getrennte AudioSource 
    [Header("Audio")]
    [SerializeField] private AudioSource countdownAudioSource;
    [SerializeField] private AudioClip countdown321GoClip;
    [SerializeField, Range(0f, 1f)] private float countdownVolume = 1f;



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


    private void Awake()
    {
        resultPanel.SetActive(false);

        //  Startzustand: nur Papier-Pack klickbar
        paperPackBtn.interactable = false;
        filterPackBtn.interactable = false;
        tobaccoPackBtn.interactable = false;

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
    }

    private IEnumerator Start()
    {
        // 1) Countdown
        yield return StartCoroutine(RunCountdown());

        // 2) Einflug aller Packs
        yield return StartCoroutine(PlayPackIntros());

        // 3) Jetzt darf der Spieler Papier anklicken
        EnterState(State.WaitPaperClick);
    }


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

    private void OnPaperClicked()
    {
        // STAGE 1: Packung öffnen
        if (!paperPackOpened)
        {
            paperPackOpened = true;

            // Optional: geschlossenes Packungsbild ausblenden
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
            }

            // Hinweistext anpassen
            SetHint("Klicke die geöffnete Papierpackung erneut, um ein Blatt zu entnehmen.");
            // Button bleibt interactable
            return;
        }

        // STAGE 2: Papier entnehmen & platzieren (wie bisher)
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


    private void Update()
    {
        if (CurrentStateIsFilterAiming() && !filterEvaluated && Input.GetKeyDown(KeyCode.Space) && filterAimer != null)
        {
            filterEvaluated = true;

            // 1) Optischer Drop
            filterAimer.Drop();

            // 2) Scoring
            EvaluateFilter();

            // 2b)
            SetZonesVisible(false);

            // 3) Weiter zum Tabak
            EnterState(State.WaitTobaccoClick);
        }
    }
    private bool CurrentStateIsFilterAiming()
    {
        return currentState == State.FilterAiming;
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
        filterEvaluated = true;

        Vector2 filterCenterScreen = RectTransformUtility.WorldToScreenPoint(null, filterAimer.CenterWorld());
        bool inPerfect = RectTransformUtility.RectangleContainsScreenPoint(zonePerfect, filterCenterScreen);
        bool inGood = RectTransformUtility.RectangleContainsScreenPoint(zoneGood, filterCenterScreen);
        bool inOkay = RectTransformUtility.RectangleContainsScreenPoint(zoneOkay, filterCenterScreen);

        if (inPerfect)
            earnedPointsThisRun = settings.pointsPerfect;    // 30
        else if (inGood)
            earnedPointsThisRun = settings.pointsGood;       // 20
        else if (inOkay)
            earnedPointsThisRun = settings.pointsOkay;       // 10
        else
            earnedPointsThisRun = settings.pointsMiss;       // 0 (Mission Fail)

        scoreManager.Add(earnedPointsThisRun);
        UpdateScoreUI();
    }



    private void OnTobaccoClicked()
    {
        if (!filterEvaluated) return; // erst filtern

        // STAGE 1: Tabakbeutel öffnen
        if (!tobaccoPackOpened)
        {
            tobaccoPackOpened = true;

            // Optional: geschlossenes Beutelbild ausblenden
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
            }

            SetHint("Klicke den geöffneten Tabakbeutel erneut, um den Tabak zu platzieren.");
            // Button bleibt aktiv
            return;
        }

        // STAGE 2: Tabak platzieren (wie bisher)
        tobaccoPackBtn.interactable = false;

        // Simple Visual: Tabak auf dem Papier anzeigen
        var tob = Instantiate(tobaccoPrefab, paperAnchor);
        tob.rectTransform.anchoredPosition = new Vector2(0f, 0.5f);

        EnterState(State.Assemble);

        // Zonen sicherheitshalber aus
        SetZonesVisible(false);
    }


    private IEnumerator FinishAssemble()
    {
        yield return new WaitForSeconds(settings.assembleSeconds);
        EnterState(State.Results);
    }

    private void ShowResults()
    {
        resultPanel.SetActive(true);
        if (earnedPointsThisRun > 0)
        {
            // Erfolg ⇒ zum globalen Highscore addieren
            scoreManager.AddRunToTotal();
            resultText.text = $"Ergebnis: {earnedPointsThisRun} Punkte\n" +
                              $"Gesamt-Highscore: {ScoreManager.GetTotal()}";
        }
        else
        {
            resultText.text = "Mission Fail!\n" +
                              $"Gesamt-Highscore: {ScoreManager.GetTotal()}";
        }
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

    private void UpdateScoreUI() => scoreText.text = $"Score (Run): {scoreManager.RunScore}";
}