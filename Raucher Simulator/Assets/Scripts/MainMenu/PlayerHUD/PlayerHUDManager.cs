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
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        UpdateHudAvailability(SceneManager.GetActiveScene());
        ResetHudDisplay();
        FindPlayerStaminaInScene();
        RefreshStaminaDisplay();
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
    }

    public void StartRound()
    {
        TimeRemaining = roundDurationSeconds;
        IsRoundRunning = true;

        RefreshPlayerName();
        RefreshTimerDisplay();
        RefreshStaminaDisplay();
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
        ApplyHudVisibility();
    }

    public void ShowHud()
    {
        hudManuallyHidden = false;
        ApplyHudVisibility();
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
}