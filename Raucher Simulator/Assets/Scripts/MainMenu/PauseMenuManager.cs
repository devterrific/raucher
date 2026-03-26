using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class PauseMenuManager : MonoBehaviour
{
    public static PauseMenuManager Instance { get; private set; }

    public static event Action<bool> OnPauseStateChanged;

    [Header("UI")]
    [SerializeField] private GameObject pauseMenuPanel;

    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Player")]
    [SerializeField] private string playerTag = "Player";

    [Header("Input")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

    [Header("Hint - Small Bubble")]
    [SerializeField] private Button smallHintButton;

    [Header("Hint - Large Bubble Root")]
    [SerializeField] private GameObject largeHintRoot;

    [Header("Hint - Navigation")]
    [SerializeField] private Button previousPageButton;
    [SerializeField] private Button nextPageButton;
    [SerializeField] private Button closeHintButton;

    [Header("Hint - Pages")]
    [SerializeField] private GameObject buroHintPage;
    [SerializeField] private GameObject flurHintPage;
    [SerializeField] private GameObject smokingMiniGameHintPage;
    [SerializeField] private GameObject bossRoomHintPage;

    [Header("Hint - Text")]
    [SerializeField] private TMP_Text buroHintTextUI;
    [SerializeField] private TMP_Text flurHintTextUI;
    [SerializeField] private TMP_Text smokingMiniGameHintTextUI;
    [SerializeField] private TMP_Text bossRoomHintTextUI;

    [Header("Hint - Images")]
    [SerializeField] private Image flurClosetImage;
    [SerializeField] private Image flurWaterDispenserImage;

    [Header("Hint - Content")]
    [TextArea(3, 8)]
    [SerializeField]
    private string buroHintText =
        "The boss's office is to the left, and the hallway is to the right—where are you going?\n\nInteract: press E";

    [TextArea(3, 8)]
    [SerializeField]
    private string flurHintText =
        "Hide behind objects so you can sneak past the Switch.";

    [TextArea(3, 8)]
    [SerializeField]
    private string smokingMiniGameHintText =
        "In the smoking mini game, keep calm and complete the task carefully.";

    [TextArea(3, 8)]
    [SerializeField]
    private string bossRoomHintText =
        "The boss room is dangerous. Watch the situation first and act at the right moment.";

    public bool IsPaused { get; private set; }

    private bool pauseAllowed = false;
    private int currentHintPageIndex = 0;
    private GameObject[] hintPages;

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

        if (previousPageButton != null)
        {
            previousPageButton.onClick.AddListener(ShowPreviousHintPage);
        }

        if (nextPageButton != null)
        {
            nextPageButton.onClick.AddListener(ShowNextHintPage);
        }

        if (closeHintButton != null)
        {
            closeHintButton.onClick.AddListener(CloseLargeHint);
        }
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (smallHintButton != null)
        {
            smallHintButton.onClick.RemoveListener(OnSmallHintClicked);
        }

        if (previousPageButton != null)
        {
            previousPageButton.onClick.RemoveListener(ShowPreviousHintPage);
        }

        if (nextPageButton != null)
        {
            nextPageButton.onClick.RemoveListener(ShowNextHintPage);
        }

        if (closeHintButton != null)
        {
            closeHintButton.onClick.RemoveListener(CloseLargeHint);
        }
    }

    private void Start()
    {
        EnsureEventSystemExists();
        UpdatePauseAvailability(SceneManager.GetActiveScene());
        SetupHintPages();
        ApplyHintTexts();
        ApplyPauseState(false);
        RefreshHintUI();
    }

    private void Update()
    {
        if (!CanUsePauseMenu())
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
        SetupHintPages();
        ApplyHintTexts();
        RefreshHintUI();

        if (!pauseAllowed)
        {
            ApplyPauseState(false);
        }
    }

    public void TogglePause()
    {
        if (!CanUsePauseMenu())
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

    private bool CanUsePauseMenu()
    {
        if (!pauseAllowed)
        {
            return false;
        }

        if (GameOverManager.Instance != null && GameOverManager.Instance.HasGameOverOccurred)
        {
            return false;
        }

        return true;
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

        RefreshHintUI();
        OnPauseStateChanged?.Invoke(IsPaused);
    }

    private void SetupHintPages()
    {
        hintPages = new GameObject[]
        {
            buroHintPage,
            flurHintPage,
            smokingMiniGameHintPage,
            bossRoomHintPage
        };
    }

    private void ApplyHintTexts()
    {
        if (buroHintTextUI != null)
        {
            buroHintTextUI.text = buroHintText;
        }

        if (flurHintTextUI != null)
        {
            flurHintTextUI.text = flurHintText;
        }

        if (smokingMiniGameHintTextUI != null)
        {
            smokingMiniGameHintTextUI.text = smokingMiniGameHintText;
        }

        if (bossRoomHintTextUI != null)
        {
            bossRoomHintTextUI.text = bossRoomHintText;
        }

        if (flurClosetImage != null)
        {
            flurClosetImage.gameObject.SetActive(true);
        }

        if (flurWaterDispenserImage != null)
        {
            flurWaterDispenserImage.gameObject.SetActive(true);
        }
    }

    private void OnSmallHintClicked()
    {
        if (!IsPaused)
        {
            return;
        }

        if (GameSessionManager.Instance == null)
        {
            return;
        }

        currentHintPageIndex = 0;

        OpenLargeHint();
    }

    private void OpenLargeHint()
    {
        if (GameSessionManager.Instance != null)
        {
            GameSessionManager.Instance.OpenLargePauseHint(currentHintPageIndex);
        }

        if (largeHintRoot != null)
        {
            largeHintRoot.SetActive(true);
        }

        ShowCurrentHintPage();
        RefreshHintUI();
    }

    private void CloseLargeHint()
    {
        if (GameSessionManager.Instance != null)
        {
            GameSessionManager.Instance.CloseLargePauseHint();
        }

        if (largeHintRoot != null)
        {
            largeHintRoot.SetActive(false);
        }

        HideAllHintPages();
        RefreshHintUI();
    }

    private void ShowCurrentHintPage()
    {
        HideAllHintPages();

        if (hintPages == null || hintPages.Length == 0)
        {
            return;
        }

        currentHintPageIndex = Mathf.Clamp(currentHintPageIndex, 0, hintPages.Length - 1);

        if (GameSessionManager.Instance != null)
        {
            GameSessionManager.Instance.SetCurrentPauseHintPage(currentHintPageIndex);
        }

        GameObject currentPage = hintPages[currentHintPageIndex];

        if (currentPage != null)
        {
            currentPage.SetActive(true);
        }

        RefreshPageButtons();
    }

    private void ShowNextHintPage()
    {
        if (hintPages == null || hintPages.Length == 0)
        {
            return;
        }

        if (currentHintPageIndex >= hintPages.Length - 1)
        {
            return;
        }

        currentHintPageIndex++;
        ShowCurrentHintPage();
    }

    private void ShowPreviousHintPage()
    {
        if (hintPages == null || hintPages.Length == 0)
        {
            return;
        }

        if (currentHintPageIndex <= 0)
        {
            return;
        }

        currentHintPageIndex--;
        ShowCurrentHintPage();
    }

    private void HideAllHintPages()
    {
        if (hintPages == null)
        {
            return;
        }

        for (int i = 0; i < hintPages.Length; i++)
        {
            if (hintPages[i] != null)
            {
                hintPages[i].SetActive(false);
            }
        }
    }

    private void RefreshPageButtons()
    {
        if (hintPages == null || hintPages.Length == 0)
        {
            if (previousPageButton != null)
            {
                previousPageButton.gameObject.SetActive(false);
            }

            if (nextPageButton != null)
            {
                nextPageButton.gameObject.SetActive(false);
            }

            return;
        }

        if (previousPageButton != null)
        {
            previousPageButton.gameObject.SetActive(currentHintPageIndex > 0);
        }

        if (nextPageButton != null)
        {
            nextPageButton.gameObject.SetActive(currentHintPageIndex < hintPages.Length - 1);
        }
    }

    private void RefreshHintUI()
    {
        bool showSmallHint = false;
        bool showLargeHint = false;

        if (IsPaused && pauseAllowed && GameSessionManager.Instance != null)
        {
            showSmallHint = GameSessionManager.Instance.IsSmallPauseHintVisible;
            showLargeHint = GameSessionManager.Instance.IsLargePauseHintOpen;
            currentHintPageIndex = GameSessionManager.Instance.CurrentPauseHintPageIndex;
        }

        if (GameOverManager.Instance != null && GameOverManager.Instance.HasGameOverOccurred)
        {
            showSmallHint = false;
            showLargeHint = false;
        }

        if (smallHintButton != null)
        {
            smallHintButton.gameObject.SetActive(showSmallHint);
        }

        if (largeHintRoot != null)
        {
            largeHintRoot.SetActive(showLargeHint);
        }

        if (showLargeHint)
        {
            ShowCurrentHintPage();
        }
        else
        {
            HideAllHintPages();
        }
    }
}