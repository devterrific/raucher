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

    [Header("Small Hint Bubble")]
    [SerializeField] private Button smallHintButton;

    [Header("Large Hint Panels")]
    [SerializeField] private GameObject buroHintPanel;
    [SerializeField] private GameObject flurHintPanel;

    [Header("Buro Hint")]
    [SerializeField] private TMP_Text buroHintTextUI;

    [TextArea(3, 8)]
    [SerializeField]
    private string buroHintText =
        "The boss's office is to the left, and the hallway is to the right—where are you going?\n\nInteract: press E";

    [Header("Flur Hint")]
    [SerializeField] private TMP_Text flurHintTextUI;
    [SerializeField] private Image flurClosetImage;
    [SerializeField] private Image flurWaterDispenserImage;

    [TextArea(3, 8)]
    [SerializeField]
    private string flurHintText =
        "Hide behind objects so you can sneak past the Switch.";

    [Header("Hint Scenes")]
    [SerializeField] private string[] allowedHintScenes = { "Buro", "Flur" };

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

        CloseAllHintPanels();
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
        CloseAllHintPanels();
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
        CloseAllHintPanels();
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
            CloseAllHintPanels();
            return;
        }

        if (GameSessionManager.Instance != null)
        {
            GameSessionManager.Instance.MarkSceneHintAsUsed(SceneManager.GetActiveScene().name);
        }

        CloseAllHintPanels();
        RefreshHintUI();
    }

    private void OnSmallHintClicked()
    {
        if (!CanUseHintInCurrentScene())
        {
            return;
        }

        OpenHintForCurrentScene();
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

        if (!shouldShowSmallHint)
        {
            CloseAllHintPanels();
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

    private void OpenHintForCurrentScene()
    {
        CloseAllHintPanels();

        string currentSceneName = SceneManager.GetActiveScene().name;

        switch (currentSceneName)
        {
            case "Buro":
                OpenBuroHint();
                break;

            case "Flur":
                OpenFlurHint();
                break;
        }
    }

    private void OpenBuroHint()
    {
        if (buroHintPanel == null)
        {
            return;
        }

        if (buroHintTextUI != null)
        {
            buroHintTextUI.text = buroHintText;
        }

        buroHintPanel.SetActive(true);
    }

    private void OpenFlurHint()
    {
        if (flurHintPanel == null)
        {
            return;
        }

        if (flurHintTextUI != null)
        {
            flurHintTextUI.text = flurHintText;
        }

        if (flurClosetImage != null)
        {
            flurClosetImage.gameObject.SetActive(true);
        }

        if (flurWaterDispenserImage != null)
        {
            flurWaterDispenserImage.gameObject.SetActive(true);
        }

        flurHintPanel.SetActive(true);
    }

    private void CloseAllHintPanels()
    {
        if (buroHintPanel != null)
        {
            buroHintPanel.SetActive(false);
        }

        if (flurHintPanel != null)
        {
            flurHintPanel.SetActive(false);
        }
    }
}