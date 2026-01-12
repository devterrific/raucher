using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class MainMenuUIController : MonoBehaviour
{
    [Header("Buttons im Hauptmenü")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button quitButton;

    [Header("Panels")]
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject creditsPanel;

    [Header("Buttons in Panels")]
    [SerializeField] private Button optionsCloseButton;
    [SerializeField] private Button creditsCloseButton;

    [Header("Erste Spielszene")]
    [SerializeField] private string firstGameSceneName = "GameScene01";

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HideAllPanels();
        }
    }

    private void Awake()
    {
        HideAllPanels();
        RegisterButtonEvents();
    }

    private void OnDestroy()
    {
        UnregisterButtonEvents();
    }

    private void RegisterButtonEvents()
    {
        if (playButton != null)
            playButton.onClick.AddListener(LoadFirstGameScene);

        if (optionsButton != null)
            optionsButton.onClick.AddListener(ToggleOptionsPanel);

        if (creditsButton != null)
            creditsButton.onClick.AddListener(ToggleCreditsPanel);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);

        if (optionsCloseButton != null)
            optionsCloseButton.onClick.AddListener(HideAllPanels);

        if (creditsCloseButton != null)
            creditsCloseButton.onClick.AddListener(HideAllPanels);
    }

    private void UnregisterButtonEvents()
    {
        if (playButton != null)
            playButton.onClick.RemoveListener(LoadFirstGameScene);

        if (optionsButton != null)
            optionsButton.onClick.RemoveListener(ToggleOptionsPanel);

        if (creditsButton != null)
            creditsButton.onClick.RemoveListener(ToggleCreditsPanel);

        if (quitButton != null)
            quitButton.onClick.RemoveListener(QuitGame);

        if (optionsCloseButton != null)
            optionsCloseButton.onClick.RemoveListener(HideAllPanels);

        if (creditsCloseButton != null)
            creditsCloseButton.onClick.RemoveListener(HideAllPanels);
    }

    private void LoadFirstGameScene()
    {
        if (string.IsNullOrWhiteSpace(firstGameSceneName))
        {
            Debug.LogError("MainMenuUIController: firstGameSceneName ist leer. Bitte im Inspector setzen.");
            return;
        }

        SceneManager.LoadScene(firstGameSceneName);
    }

    private void ToggleOptionsPanel()
    {
        if (optionsPanel == null) return;

        bool shouldShow = !optionsPanel.activeSelf;
        HideAllPanels();

        optionsPanel.SetActive(shouldShow);
        ClearSelectedUserInterfaceElement();
    }

    private void ToggleCreditsPanel()
    {
        if (creditsPanel == null) return;

        bool shouldShow = !creditsPanel.activeSelf;
        HideAllPanels();

        creditsPanel.SetActive(shouldShow);
        ClearSelectedUserInterfaceElement();
    }


    private void HideAllPanels()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        if (creditsPanel != null)
            creditsPanel.SetActive(false);

        ClearSelectedUserInterfaceElement();
    }

    private void ClearSelectedUserInterfaceElement()
    {
        if (EventSystem.current == null)
        {
            return;
        }

        EventSystem.current.SetSelectedGameObject(null);
    }

    private void QuitGame()
    {
        Debug.Log("Spiel wird beendet...");

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
