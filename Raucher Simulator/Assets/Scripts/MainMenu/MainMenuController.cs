using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject newGamePanel;
    [SerializeField] private GameObject highscorePanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject creditsPanel;

    private void Start()
    {
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        SetOnlyOnePanelActive(mainMenuPanel);
    }

    public void ShowNewGamePanel()
    {
        SetOnlyOnePanelActive(newGamePanel);
    }

    public void ShowHighscorePanel()
    {
        SetOnlyOnePanelActive(highscorePanel);
    }

    public void ShowOptionsPanel()
    {
        SetOnlyOnePanelActive(optionsPanel);
    }

    public void ShowCreditsPanel()
    {
        SetOnlyOnePanelActive(creditsPanel);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void SetOnlyOnePanelActive(GameObject activePanel)
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (newGamePanel != null) newGamePanel.SetActive(false);
        if (highscorePanel != null) highscorePanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);

        if (activePanel != null) activePanel.SetActive(true);
    }
}