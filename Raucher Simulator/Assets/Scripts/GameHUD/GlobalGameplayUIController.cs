using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GlobalGameplayUIController : MonoBehaviour
{
    public static GlobalGameplayUIController Instance { get; private set; }

    [Header("Welche Szene ist das Main Menu?")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("HUD Texte")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text playerNameText;

    [Header("Game Over Panel")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button backToMenuButton;

    private bool gameIsOver;
    private int lastShownScore = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;

        gameIsOver = false;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        RegisterButtonEvents();
        UpdateAllTexts(forceScoreUpdate: true);
        ApplyVisibilityForScene(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        UnregisterButtonEvents();

        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        if (GameSessionManager.Instance == null)
        {
            return;
        }

        if (SceneManager.GetActiveScene().name == mainMenuSceneName)
        {
            return;
        }

        if (gameIsOver == true)
        {
            return;
        }

        UpdateAllTexts(forceScoreUpdate: false);

        if (GameSessionManager.Instance.IsSessionRunning == true &&
            GameSessionManager.Instance.RemainingTimeInSeconds <= 0f)
        {
            TriggerGameOver();
        }
    }

    private void UpdateAllTexts(bool forceScoreUpdate)
    {
        UpdateTimerText();
        UpdateScoreText(forceScoreUpdate);
        UpdatePlayerNameText();
    }

    private void UpdateTimerText()
    {
        if (timerText == null || GameSessionManager.Instance == null)
        {
            return;
        }

        float remaining = GameSessionManager.Instance.RemainingTimeInSeconds;

        int totalSeconds = Mathf.CeilToInt(remaining);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timerText.text = minutes.ToString("00") + ":" + seconds.ToString("00");
    }

    private void UpdateScoreText(bool forceScoreUpdate)
    {
        if (scoreText == null || GameSessionManager.Instance == null)
        {
            return;
        }

        int currentScore = GameSessionManager.Instance.CurrentScore;

        if (forceScoreUpdate == true || currentScore != lastShownScore)
        {
            lastShownScore = currentScore;
            scoreText.text = "Score: " + currentScore.ToString();
        }
    }

    private void TriggerGameOver()
    {
        gameIsOver = true;

        if (GameSessionManager.Instance != null)
        {
            GameSessionManager.Instance.StopSessionTimer();
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        Time.timeScale = 0f;
    }

    private void RegisterButtonEvents()
    {
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGameAndReset);
        }

        if (backToMenuButton != null)
        {
            backToMenuButton.onClick.AddListener(BackToMenuAndReset);
        }
    }

    private void UnregisterButtonEvents()
    {
        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(QuitGameAndReset);
        }

        if (backToMenuButton != null)
        {
            backToMenuButton.onClick.RemoveListener(BackToMenuAndReset);
        }
    }

    private void QuitGameAndReset()
    {
        SaveScoreIfNeeded();
        ResetRunState();

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void BackToMenuAndReset()
    {
        SaveScoreIfNeeded();
        ResetRunState();

        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void SaveScoreIfNeeded()
    {
        if (GameSessionManager.Instance != null)
        {
            GameSessionManager.Instance.FinishSessionAndSaveToHighscores();
        }
    }

    private void ResetRunState()
    {
        Time.timeScale = 1f;

        gameIsOver = false;
        lastShownScore = -1;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (GameSessionManager.Instance != null)
        {
            GameSessionManager.Instance.ResetSessionCompletely();
        }

        PersistentGameplayCleaner.DestroyAllPersistentGameplayObjectsExceptSession();

        Destroy(gameObject);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyVisibilityForScene(scene.name);
        UpdateAllTexts(forceScoreUpdate: true);
    }

    private void ApplyVisibilityForScene(string sceneName)
    {
        bool isInMainMenu = sceneName == mainMenuSceneName;

        if (timerText != null)
        {
            timerText.gameObject.SetActive(isInMainMenu == false);
        }

        if (scoreText != null)
        {
            scoreText.gameObject.SetActive(isInMainMenu == false);
        }

        if (isInMainMenu == true && gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (playerNameText)
        {
            playerNameText.gameObject.SetActive(isInMainMenu == false);
        }
    }
    private void UpdatePlayerNameText()
    {
        if (playerNameText == null || GameSessionManager.Instance == null)
        {
            return;
        }

        string playerName = GameSessionManager.Instance.PlayerName;

        if (string.IsNullOrWhiteSpace(playerName))
        {
            playerName = "---";
        }

        playerNameText.text = "Player: " + playerName;
    }
}
