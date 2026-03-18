using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHUDManager : MonoBehaviour
{
    public static PlayerHUDManager Instance { get; private set; }

    [Header("HUD")]
    [SerializeField] private GameObject hudRoot;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("UI")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private Image staminaImage;

    [Header("Stamina UI")]
    [SerializeField] private Sprite[] staminaSprites;

    [Header("Round Timer")]
    [SerializeField] private float roundDurationSeconds = 300f;

    [Header("Hint Bubble")]
    [SerializeField] private Button smallHintButton;
    [SerializeField] private GameObject largeHintPanel;
    [SerializeField] private TMP_Text largeHintText;

    [Header("Hint Scenes")]
    [SerializeField] private string[] allowedHintScenes = { "Buro", "Flur" };

    [Header("Hint Texts")]
    [TextArea(3, 8)]
    [SerializeField]
    private string buroHintText =
        "The boss's office is to the left, and the hallway is to the right—where are you going?";

    [TextArea(3, 8)]
    [SerializeField]
    private string flurHintText =
        "Hide behind objects so you can sneak past the Switch.";

    [TextArea(3, 8)]
    [SerializeField]
    private string defaultHintText =
        "In dieser Szene gibt es aktuell keine zusätzliche Erklärung.";

    private PlayerStamina playerStamina;

    public bool IsRoundRunning { get; private set; }
    public float TimeRemaining { get; private set; }

    private bool hudAllowed = false;
    private bool hudManuallyHidden = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        if (smallHintButton != null)
        {
            smallHintButton.onClick.AddListener(OnSmallHintClicked);
        }
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (smallHintButton != null)
        {
            smallHintButton.onClick.RemoveListener(OnSmallHintClicked);
        }
    }

    private void Start()
    {
        UpdateHudAvailability(SceneManager.GetActiveScene());
        ResetHudDisplay();
        FindPlayerStaminaInScene();
        RefreshStaminaDisplay();
        RefreshHintUI();
    }

    private void Update()
    {
        if (!hudAllowed || !IsRoundRunning)
        {
            RefreshStaminaDisplay();
            return;
        }

        if (GameOverManager.Instance != null && GameOverManager.Instance.HasGameOverOccurred)
        {
            RefreshStaminaDisplay();
            return;
        }

        if (TimeRemaining > 0f)
        {
            TimeRemaining -= Time.deltaTime;

            if (TimeRemaining <= 0f)
            {
                TimeRemaining = 0f;
                RefreshTimerDisplay();
                StopRound();

                if (GameOverManager.Instance != null)
                {
                    GameOverManager.Instance.TriggerGameOver();
                }

                RefreshStaminaDisplay();
                return;
            }

            RefreshTimerDisplay();
        }

        RefreshStaminaDisplay();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
    {
        UpdateHudAvailability(scene);
        FindPlayerStaminaInScene();

        if (!hudAllowed)
        {
            ResetHudDisplay();
        }
        else
        {
            RefreshPlayerName();
            RefreshTimerDisplay();
            RefreshStaminaDisplay();
        }

        CloseLargeHint();
        RefreshHintUI();
    }

    public void StartRound()
    {
        TimeRemaining = roundDurationSeconds;
        IsRoundRunning = true;

        RefreshPlayerName();
        RefreshTimerDisplay();
        RefreshStaminaDisplay();
        RefreshHintUI();
    }

    public void StopRound()
    {
        IsRoundRunning = false;
    }

    public void ResetHudDisplay()
    {
        StopRound();
        TimeRemaining = roundDurationSeconds;

        RefreshPlayerName();
        RefreshTimerDisplay();
        RefreshStaminaDisplay();
        ApplyHudVisibility();
        CloseLargeHint();
    }

    public void RefreshPlayerName()
    {
        if (playerNameText == null)
        {
            return;
        }

        if (GameSessionManager.Instance == null)
        {
            playerNameText.text = string.Empty;
            return;
        }

        playerNameText.text = GameSessionManager.Instance.PlayerName;
    }

    public void HideHud()
    {
        hudManuallyHidden = true;
        CloseLargeHint();
        ApplyHudVisibility();
    }

    public void ShowHud()
    {
        hudManuallyHidden = false;
        ApplyHudVisibility();
        RefreshHintUI();
    }

    public void OnLargeHintClicked()
    {
        if (!CanUseHintInCurrentScene())
        {
            CloseLargeHint();
            return;
        }

        if (GameSessionManager.Instance != null)
        {
            GameSessionManager.Instance.MarkSceneHintAsUsed(SceneManager.GetActiveScene().name);
        }

        CloseLargeHint();
        RefreshHintUI();
    }

    private void OnSmallHintClicked()
    {
        if (!CanUseHintInCurrentScene())
        {
            return;
        }

        OpenLargeHint();
    }

    private void UpdateHudAvailability(Scene activeScene)
    {
        hudAllowed = activeScene.name != mainMenuSceneName;
        ApplyHudVisibility();
    }

    private void ApplyHudVisibility()
    {
        if (hudRoot == null)
        {
            return;
        }

        bool shouldShowHud = hudAllowed && !hudManuallyHidden;

        if (GameOverManager.Instance != null && GameOverManager.Instance.HasGameOverOccurred)
        {
            shouldShowHud = false;
        }

        hudRoot.SetActive(shouldShowHud);
    }

    private void RefreshTimerDisplay()
    {
        if (timerText == null)
        {
            return;
        }

        int totalSeconds = Mathf.CeilToInt(TimeRemaining);

        if (totalSeconds < 0)
        {
            totalSeconds = 0;
        }

        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timerText.text = minutes.ToString("00") + ":" + seconds.ToString("00");
    }

    private void FindPlayerStaminaInScene()
    {
        playerStamina = FindObjectOfType<PlayerStamina>();
    }

    private void RefreshStaminaDisplay()
    {
        if (staminaImage == null)
        {
            return;
        }

        if (playerStamina == null)
        {
            playerStamina = FindObjectOfType<PlayerStamina>();

            if (playerStamina == null)
            {
                return;
            }
        }

        if (staminaSprites == null || staminaSprites.Length == 0)
        {
            return;
        }

        float maxStamina = GetMaxStamina();

        if (maxStamina <= 0f)
        {
            return;
        }

        float normalizedStamina = playerStamina.CurrentStamina / maxStamina;
        normalizedStamina = Mathf.Clamp01(normalizedStamina);

        int spriteIndex = Mathf.RoundToInt((staminaSprites.Length - 1) * normalizedStamina);
        spriteIndex = Mathf.Clamp(spriteIndex, 0, staminaSprites.Length - 1);

        staminaImage.sprite = staminaSprites[spriteIndex];
    }

    private float GetMaxStamina()
    {
        return playerStamina.MaxStamina;
    }

    private void RefreshHintUI()
    {
        if (smallHintButton == null)
        {
            return;
        }

        bool shouldShowSmallHint = hudAllowed
            && !hudManuallyHidden
            && CanUseHintInCurrentScene();

        if (GameOverManager.Instance != null && GameOverManager.Instance.HasGameOverOccurred)
        {
            shouldShowSmallHint = false;
        }

        smallHintButton.gameObject.SetActive(shouldShowSmallHint);

        if (largeHintPanel != null && !shouldShowSmallHint)
        {
            largeHintPanel.SetActive(false);
        }
    }

    private bool CanUseHintInCurrentScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        if (!IsHintAllowedInScene(currentSceneName))
        {
            return false;
        }

        if (GameSessionManager.Instance == null)
        {
            return false;
        }

        return !GameSessionManager.Instance.HasSceneHintBeenUsed(currentSceneName);
    }

    private bool IsHintAllowedInScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            return false;
        }

        if (allowedHintScenes == null || allowedHintScenes.Length == 0)
        {
            return false;
        }

        for (int i = 0; i < allowedHintScenes.Length; i++)
        {
            if (allowedHintScenes[i] == sceneName)
            {
                return true;
            }
        }

        return false;
    }

    private void OpenLargeHint()
    {
        if (largeHintPanel == null)
        {
            return;
        }

        if (largeHintText != null)
        {
            largeHintText.text = GetHintTextForCurrentScene();
        }

        largeHintPanel.SetActive(true);
    }

    private void CloseLargeHint()
    {
        if (largeHintPanel != null)
        {
            largeHintPanel.SetActive(false);
        }
    }

    private string GetHintTextForCurrentScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        switch (currentSceneName)
        {
            case "Buro":
                return buroHintText;

            case "Flur":
                return flurHintText;

            default:
                return defaultHintText;
        }
    }
}