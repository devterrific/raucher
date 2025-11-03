using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class MiniGameFlow : MonoBehaviour
{
    public enum State { Countdown, WaitPaperClick, FilterAiming, WaitTobaccoClick, Assemble, Results }
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

    private void Awake()
    {
        resultPanel.SetActive(false);
        tobaccoPackBtn.interactable = false;
        paperPackBtn.onClick.AddListener(OnPaperClicked);
        tobaccoPackBtn.onClick.AddListener(OnTobaccoClicked);
        scoreManager.ResetRun();
        UpdateScoreUI();
        countdownPanel.alpha = 0f; countdownPanel.gameObject.SetActive(true);
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
        switch (s)
        {
            case State.WaitPaperClick:
                SetHint("Klicke die Papierpackung (rechts), um ein Blatt zu entnehmen.");
                paperPackBtn.interactable = true;
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

    private void OnPaperClicked()
    {
        paperPackBtn.interactable = false;
        spawnedPaper = Instantiate(paperPrefab, paperAnchor);
        spawnedPaper.rectTransform.anchoredPosition = Vector2.zero;
        EnterState(State.FilterAiming);
    }

    private void StartFilterAiming()
    {
        var filterImg = Instantiate(filterPrefab, filterAnchor);
        var rt = filterImg.rectTransform;
        rt.anchoredPosition = Vector2.zero;

        filterAimer = filterImg.gameObject.AddComponent<FilterAimer>();
        // Range = Breite der TargetZone; du kannst auch fix 1000 px nehmen
        float range = targetZone.rect.width;
        filterAimer.Init(filterAnchor, settings.filterSpeed, settings.rightToLeft, range, OnFilterPassedLimits);

        filterEvaluated = false;
    }

    private void Update()
    {
        // Space = Drop & Score
        if (!filterEvaluated && Input.GetKeyDown(KeyCode.Space) && filterAimer != null)
        {
            filterAimer.Freeze();
            EvaluateFilter();
            EnterState(State.WaitTobaccoClick);
        }
    }

    private void OnFilterPassedLimits()
    {
        if (filterEvaluated) return;
        // Spieler hat nicht rechtzeitig gedrückt → Auswertung "Miss"
        earnedPointsThisRun = 0;
        scoreManager.Add(earnedPointsThisRun);
        UpdateScoreUI();
        filterEvaluated = true;
        EnterState(State.WaitTobaccoClick);
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
        tob.rectTransform.anchoredPosition = new Vector2(0f, 30f);

        EnterState(State.Assemble);
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
