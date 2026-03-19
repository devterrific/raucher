using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance { get; private set; }

    public static event Action OnGameOverTriggered;

    [Header("UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button restartButton;

    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Canvas Order")]
    [SerializeField] private int gameOverSortingOrder = 1000;

    [Header("Player")]
    [SerializeField] private string playerTag = "Player";

    public bool HasGameOverOccurred { get; private set; }
    public bool IsGameplayInputBlocked => HasGameOverOccurred;

    private bool gameOverAllowed = false;
    private Canvas gameOverCanvas;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (gameOverPanel != null)
        {
            gameOverCanvas = gameOverPanel.GetComponentInParent<Canvas>();
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartToMainMenu);
        }
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(RestartToMainMenu);
        }
    }

    private void Start()
    {
        UpdateGameOverAvailability(SceneManager.GetActiveScene());
        HideGameOverPanel();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
    {
        UpdateGameOverAvailability(scene);

        if (!gameOverAllowed)
        {
            ResetGameOverState();
        }
    }

    public void TriggerGameOver()
    {
        if (HasGameOverOccurred || !gameOverAllowed)
        {
            return;
        }

        HasGameOverOccurred = true;
        Time.timeScale = 0f;

        if (GameSessionManager.Instance != null)
        {
            GameSessionManager.Instance.EndSession();
        }

        if (PlayerHUDManager.Instance != null)
        {
            PlayerHUDManager.Instance.HideHud();
        }

        if (PauseMenuManager.Instance != null && PauseMenuManager.Instance.IsPaused)
        {
            PauseMenuManager.Instance.ResumeGame();
        }

        ShowGameOverPanel();
        OnGameOverTriggered?.Invoke();
    }

    public void RestartToMainMenu()
    {
        Time.timeScale = 1f;
        ResetGameOverState();
        DestroyPersistentPlayerIfExists();
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void UpdateGameOverAvailability(Scene activeScene)
    {
        gameOverAllowed = activeScene.name != mainMenuSceneName;
    }

    private void ShowGameOverPanel()
    {
        if (gameOverPanel == null)
        {
            return;
        }

        if (gameOverCanvas == null)
        {
            gameOverCanvas = gameOverPanel.GetComponentInParent<Canvas>();
        }

        if (gameOverCanvas != null)
        {
            gameOverCanvas.overrideSorting = true;
            gameOverCanvas.sortingOrder = gameOverSortingOrder;
        }

        gameOverPanel.SetActive(true);
        gameOverPanel.transform.SetAsLastSibling();
    }

    private void HideGameOverPanel()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    private void ResetGameOverState()
    {
        HasGameOverOccurred = false;
        Time.timeScale = 1f;
        HideGameOverPanel();
    }

    private void DestroyPersistentPlayerIfExists()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);

        if (playerObject != null)
        {
            Destroy(playerObject);
        }
    }
}