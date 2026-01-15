using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class MainMenuUIController : MonoBehaviour
{
    [Header("Hauptmenü Buttons")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button highscoresButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button quitButton;

    [Header("Global Gameplay UI Prefab")]
    [SerializeField] private GameObject globalGameplayUIPrefab;

    [Header("Panels")]
    [SerializeField] private GameObject newGamePanel;
    [SerializeField] private GameObject highscoresPanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject creditsPanel;

    [Header("New Game Panel UI")]
    [SerializeField] private TMP_InputField playerNameInputField;
    [SerializeField] private Button playButton;
    [SerializeField] private Button newGameBackButton;

    [Header("Options Panel UI")]
    [SerializeField] private Button optionsCloseButton;

    [Header("Credits Panel UI")]
    [SerializeField] private Button creditsCloseButton;

    [Header("Highscores Panel UI")]
    [SerializeField] private Button highscoresBackButton;
    [SerializeField] private HighscorePanelUI highscorePanelUI;

    [Header("Szene")]
    [SerializeField] private string firstGameSceneName = "GameScene01";

    private void Awake()
    {
        HideAllPanels();
        RegisterButtonEvents();

        EnsureSessionManagerExists();

 
        // Deaktiviert, weil der letzte Eintrag des Spielernames nicht gespeichert werden soll!!
        // PrefillLastPlayerName();
    }

    private void OnDestroy()
    {
        UnregisterButtonEvents();
    }

    private void RegisterButtonEvents()
    {
        if (newGameButton != null) newGameButton.onClick.AddListener(OpenNewGamePanel);
        if (highscoresButton != null) highscoresButton.onClick.AddListener(OpenHighscoresPanel);
        if (optionsButton != null) optionsButton.onClick.AddListener(OpenOptionsPanel);
        if (creditsButton != null) creditsButton.onClick.AddListener(OpenCreditsPanel);
        if (quitButton != null) quitButton.onClick.AddListener(QuitGame);

        if (playButton != null) playButton.onClick.AddListener(StartGameFromNewGamePanel);
        if (newGameBackButton != null) newGameBackButton.onClick.AddListener(CloseNewGamePanel);

        if (optionsCloseButton != null) optionsCloseButton.onClick.AddListener(CloseOptionsPanel);
        if (creditsCloseButton != null) creditsCloseButton.onClick.AddListener(CloseCreditsPanel);

        if (highscoresBackButton != null) highscoresBackButton.onClick.AddListener(CloseHighscoresPanel);
    }

    private void UnregisterButtonEvents()
    {
        if (newGameButton != null) newGameButton.onClick.RemoveListener(OpenNewGamePanel);
        if (highscoresButton != null) highscoresButton.onClick.RemoveListener(OpenHighscoresPanel);
        if (optionsButton != null) optionsButton.onClick.RemoveListener(OpenOptionsPanel);
        if (creditsButton != null) creditsButton.onClick.RemoveListener(OpenCreditsPanel);
        if (quitButton != null) quitButton.onClick.RemoveListener(QuitGame);

        if (playButton != null) playButton.onClick.RemoveListener(StartGameFromNewGamePanel);

        // BUGFIX: Hier muss CloseNewGamePanel entfernt werden (nicht HideAllPanels)
        if (newGameBackButton != null) newGameBackButton.onClick.RemoveListener(CloseNewGamePanel);

        if (optionsCloseButton != null) optionsCloseButton.onClick.RemoveListener(CloseOptionsPanel);
        if (creditsCloseButton != null) creditsCloseButton.onClick.RemoveListener(CloseCreditsPanel);

        if (highscoresBackButton != null) highscoresBackButton.onClick.RemoveListener(CloseHighscoresPanel);
    }

    private void OpenNewGamePanel()
    {
        HideAllPanels();

        if (newGamePanel != null)
        {
            newGamePanel.SetActive(true);
        }

        // Immer frisch starten
        ResetNewGameInputField();
        ClearSelectedUserInterfaceElement();

        if (playerNameInputField != null)
        {
            playerNameInputField.ActivateInputField();
        }
    }

    private void CloseNewGamePanel()
    {
        ResetNewGameInputField();

        if (newGamePanel != null)
        {
            newGamePanel.SetActive(false);
        }

        ClearSelectedUserInterfaceElement();
    }

    private void StartGameFromNewGamePanel()
    {
        string enteredName = "";

        if (playerNameInputField != null)
        {
            enteredName = playerNameInputField.text;
        }

        if (string.IsNullOrWhiteSpace(enteredName))
        {
            Debug.LogWarning("Bitte einen Spielernamen eingeben.");
            return;
        }

        if (GameSessionManager.Instance != null)
        {
            GameSessionManager.Instance.StartNewSession(enteredName);
        }

        ResetNewGameInputField();
        SpawnGlobalGameplayUIIfNeeded();
        SceneManager.LoadScene(firstGameSceneName);
    }

    private void OpenHighscoresPanel()
    {
        HideAllPanels();

        if (highscoresPanel != null)
        {
            highscoresPanel.SetActive(true);
        }

        if (highscorePanelUI != null)
        {
            highscorePanelUI.RefreshHighscores();
        }

        ClearSelectedUserInterfaceElement();
    }

    private void CloseHighscoresPanel()
    {
        if (highscoresPanel != null)
        {
            highscoresPanel.SetActive(false);
        }

        ClearSelectedUserInterfaceElement();
    }

    private void OpenOptionsPanel()
    {
        HideAllPanels();

        if (optionsPanel != null)
        {
            optionsPanel.SetActive(true);
        }

        ClearSelectedUserInterfaceElement();
    }

    private void CloseOptionsPanel()
    {
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }

        ClearSelectedUserInterfaceElement();
    }

    private void OpenCreditsPanel()
    {
        HideAllPanels();

        if (creditsPanel != null)
        {
            creditsPanel.SetActive(true);
        }

        ClearSelectedUserInterfaceElement();
    }

    private void CloseCreditsPanel()
    {
        if (creditsPanel != null)
        {
            creditsPanel.SetActive(false);
        }

        ClearSelectedUserInterfaceElement();
    }

    private void HideAllPanels()
    {
        if (newGamePanel != null) newGamePanel.SetActive(false);
        if (highscoresPanel != null) highscoresPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);

        ClearSelectedUserInterfaceElement();
    }

    private void ResetNewGameInputField()
    {
        if (playerNameInputField == null)
        {
            return;
        }

        playerNameInputField.text = string.Empty;
        playerNameInputField.DeactivateInputField();
    }

    private void ClearSelectedUserInterfaceElement()
    {
        if (EventSystem.current == null)
        {
            return;
        }

        EventSystem.current.SetSelectedGameObject(null);
    }

    private void EnsureSessionManagerExists()
    {
        if (GameSessionManager.Instance != null)
        {
            return;
        }

        GameObject sessionObject = new GameObject("GameSessionManager");
        sessionObject.AddComponent<GameSessionManager>();
    }

    private void PrefillLastPlayerName()
    {
        if (playerNameInputField == null)
        {
            return;
        }

        string lastName = PlayerPrefs.GetString("LastPlayerName", "");
        if (string.IsNullOrWhiteSpace(lastName) == false)
        {
            playerNameInputField.text = lastName;
        }
    }
    private void SpawnGlobalGameplayUIIfNeeded()
    {
        if (GlobalGameplayUIController.Instance != null)
        {
            return;
        }

        if (globalGameplayUIPrefab == null)
        {
            Debug.LogError("MainMenuUIController: globalGameplayUIPrefab ist nicht gesetzt (Inspector).");
            return;
        }

        Instantiate(globalGameplayUIPrefab);
    }

    private void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
