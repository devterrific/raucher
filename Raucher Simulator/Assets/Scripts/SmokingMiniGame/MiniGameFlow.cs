using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using Unity.PlasticSCM.Editor.WebApi;

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

    [Header("Config")]
    [SerializeField] private DifficultySettings settings;
    [SerializeField] private ScoreManager scoreManager;

    private Image spawnedPaper;
    private FilterAimer filterAimer;
    private bool filterEvaluated;
    private int earnedPointsThisRun;
    private State currentState;

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
    }

    private IEnumerator Start()
    {
        yield return StartCoroutine(RunCountdown());
        EnterState(State.WaitPaperClick);
    }

    private IEnumerator RunCountdown()
    {
        SetHint("");
        countdownPanel.alpha = 1f;
        float t = settings.countdownSeconds;
        while (t > 0f)
        {
            countdownText.text = Mathf.CeilToInt(t).ToString();
            t -= Time.unscaledDeltaTime;
            yield return null;
        }
        countdownText.text = "Start!";
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
        tobaccoPackBtn.interactable = false;

        // Simple Visual: Tabak auf dem Papier anzeigen
        var tob = Instantiate(tobaccoPrefab, paperAnchor);
        tob.rectTransform.anchoredPosition = new Vector2(0f, 0.5f);

        EnterState(State.Assemble);
        
        //  Zonen wieder verstecken 
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

    public void OnBtnRetry() => SceneManager.LoadScene("SmokingMinigame");
    public void OnBtnWeiter() => SceneManager.LoadScene("Vorraum_Placeholder");

    private void UpdateScoreUI() => scoreText.text = $"Score (Run): {scoreManager.RunScore}";
}
