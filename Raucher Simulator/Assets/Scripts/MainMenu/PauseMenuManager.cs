using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class PauseMenuManager : MonoBehaviour
{
    public static PauseMenuManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject pauseMenuPanel;

    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Player")]
    [SerializeField] private string playerTag = "Player";

    [Header("Input")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

    public bool IsPaused { get; private set; }

    private bool pauseAllowed = false;

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
        EnsureEventSystemExists();
        UpdatePauseAvailability(SceneManager.GetActiveScene());
        ApplyPauseState(false);
    }

    private void Update()
    {
        if (!pauseAllowed)
        {
            return;
        }

        if (Input.GetKeyDown(pauseKey))
        {
            TogglePause();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
    {
        EnsureEventSystemExists();
        UpdatePauseAvailability(scene);

        if (!pauseAllowed)
        {
            ApplyPauseState(false);
        }
    }

    public void TogglePause()
    {
        if (!pauseAllowed)
        {
            return;
        }

        ApplyPauseState(!IsPaused);
    }

    public void ResumeGame()
    {
        if (!pauseAllowed)
        {
            return;
        }

        ApplyPauseState(false);
    }

    public void LoadMainMenu()
    {
        EndCurrentSessionIfNeeded();
        ApplyPauseState(false);
        DestroyPersistentPlayerIfExists();
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        EndCurrentSessionIfNeeded();
        ApplyPauseState(false);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void EndCurrentSessionIfNeeded()
    {
        if (GameSessionManager.Instance != null)
        {
            GameSessionManager.Instance.EndSession();
        }
    }

    private void DestroyPersistentPlayerIfExists()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);

        if (playerObject != null)
        {
            Destroy(playerObject);
        }
    }

    private void EnsureEventSystemExists()
    {
        EventSystem existingEventSystem = FindFirstObjectByType<EventSystem>();

        if (existingEventSystem != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<StandaloneInputModule>();
    }

    private void UpdatePauseAvailability(Scene activeScene)
    {
        pauseAllowed = activeScene.name != mainMenuSceneName;

        if (!pauseAllowed && pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
    }

    private void ApplyPauseState(bool shouldPause)
    {
        IsPaused = shouldPause;
        Time.timeScale = IsPaused ? 0f : 1f;

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(IsPaused);
        }

        if (PlayerHUDManager.Instance != null)
        {
            if (IsPaused)
            {
                PlayerHUDManager.Instance.HideHud();
            }
            else
            {
                PlayerHUDManager.Instance.ShowHud();
            }
        }
    }
}