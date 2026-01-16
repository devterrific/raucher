using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PauseMenuManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private TextMeshProUGUI pauseText;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button quitButton;

    [Header("Settings")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

    private bool isPaused = false;
    private static PauseMenuManager instance;
    private PlayerHUDManager hudManager;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (pauseText != null)
            pauseText.text = "BREAK";

        hudManager = FindObjectOfType<PlayerHUDManager>();

        if (continueButton != null)
            continueButton.onClick.AddListener(ResumeGame);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGameWithSave);
    }

    private void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu")
            return;

        isPaused = true;
        Time.timeScale = 0f;

        if (hudManager != null)
            hudManager.SetGameActive(false);

        if (pausePanel != null)
            pausePanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (hudManager != null)
            hudManager.SetGameActive(true);

        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    private void QuitGameWithSave()
    {
        SaveHighscore();
        QuitGame();
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    private void SaveHighscore()
    {
        if (hudManager != null)
        {
            hudManager.SaveHighscore();
        }
    }

    public bool IsGamePaused()
    {
        return isPaused;
    }
}