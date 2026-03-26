using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject newGamePanel;
    [SerializeField] private GameObject highscorePanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject creditsPanel;

    [Header("New Game")]
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private GameObject errorTextPlayerName;
    [SerializeField] private string firstGameSceneName = "Buro";

    [Header("Highscore")]
    [SerializeField] private HighscoreManager highscoreManager;

    [Header("Options")]
    [SerializeField] private Image soundButtonImage;
    [SerializeField] private Sprite soundOnSprite;
    [SerializeField] private Sprite soundOffSprite;
    [SerializeField] private Slider sliderVolume;
    
    [Header("Mover")]
    [SerializeField] private BackgroundMover backgroundMover;

    [Header("Audio")]
    [SerializeField] private MainMenuAudio mainMenuAudio;

    private const string PlayerNameKey = "PlayerName";
    private const string SoundEnabledKey = "SoundEnabled";
    private const string MasterVolumeKey = "MasterVolume";

    private bool isSoundEnabled = true;

    private void Start()
    {
        ShowMainMenu();

        if (errorTextPlayerName != null)
        {
            errorTextPlayerName.SetActive(false);
        }

        LoadOptions();
        ApplyAudioSettings();
        UpdateSoundButtonVisual();

        if (mainMenuAudio != null)
        {
            mainMenuAudio.RefreshAudioFromSettings();
        }
    }

    public void ShowMainMenu()
    {
        if (newGamePanel != null && newGamePanel.activeSelf)
        {
            ResetNewGameInput();
        }

        SetOnlyOnePanelActive(mainMenuPanel);
        HideNameError();

        if (backgroundMover != null)
        {
            backgroundMover.MoveToMainMenu();
        }
    }

    public void ShowNewGamePanel()
    {
        SetOnlyOnePanelActive(newGamePanel);
        ResetNewGameInput();
    }

    public void ShowHighscorePanel()
    {
        SetOnlyOnePanelActive(highscorePanel);
        HideNameError();

        if (highscoreManager != null)
        {
            highscoreManager.ReloadAndRefresh();
        }

        if (backgroundMover != null)
        {
            backgroundMover.MoveToHighscore();
        }
    }

    public void ShowOptionsPanel()
    {
        SetOnlyOnePanelActive(optionsPanel);
        HideNameError();
        UpdateSoundButtonVisual();
    }

    public void ShowCreditsPanel()
    {
        SetOnlyOnePanelActive(creditsPanel);
        HideNameError();

        if (backgroundMover != null)
        {
            backgroundMover.MoveToCredits();
        }
    }

    public void StartNewGame()
    {
        string playerName = GetTrimmedPlayerName();

        if (string.IsNullOrEmpty(playerName) || !IsValidPlayerName(playerName))
        {
            ShowNameError();
            return;
        }

        HideNameError();
        PlayerPrefs.SetString(PlayerNameKey, playerName);
        PlayerPrefs.Save();

        SceneManager.LoadScene(firstGameSceneName);
    }

    public void ToggleSound()
    {
        isSoundEnabled = !isSoundEnabled;

        SaveOptions();
        ApplyAudioSettings();
        UpdateSoundButtonVisual();

        if (mainMenuAudio != null)
        {
            mainMenuAudio.RefreshAudioFromSettings();
        }
    }

    public void OnVolumeSliderChanged(float volume)
    {
        SaveOptions();
        ApplyAudioSettings();

        if (mainMenuAudio != null)
        {
            mainMenuAudio.RefreshAudioFromSettings();
        }
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

    private string GetTrimmedPlayerName()
    {
        if (nameInputField == null)
        {
            return string.Empty;
        }

        return nameInputField.text.Trim();
    }

    private bool IsValidPlayerName(string playerName)
    {
        for (int i = 0; i < playerName.Length; i++)
        {
            char currentCharacter = playerName[i];

            if (char.IsLetterOrDigit(currentCharacter) || currentCharacter == ' ')
            {
                continue;
            }

            return false;
        }

        return true;
    }

    private void ShowNameError()
    {
        if (errorTextPlayerName != null)
        {
            errorTextPlayerName.SetActive(true);
        }
    }

    private void HideNameError()
    {
        if (errorTextPlayerName != null)
        {
            errorTextPlayerName.SetActive(false);
        }
    }

    private void ResetNewGameInput()
    {
        if (nameInputField != null)
        {
            nameInputField.text = string.Empty;
        }

        HideNameError();
    }

    private void LoadOptions()
    {
        isSoundEnabled = PlayerPrefs.GetInt(SoundEnabledKey, 1) == 1;

        float masterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);

        if (sliderVolume != null)
        {
            sliderVolume.value = masterVolume;
        }
    }

    private void SaveOptions()
    {
        PlayerPrefs.SetInt(SoundEnabledKey, isSoundEnabled ? 1 : 0);

        if (sliderVolume != null)
        {
            PlayerPrefs.SetFloat(MasterVolumeKey, sliderVolume.value);
        }

        PlayerPrefs.Save();
    }

    private void ApplyAudioSettings()
    {
        float volume = sliderVolume != null ? sliderVolume.value : 1f;
        AudioListener.volume = isSoundEnabled ? volume : 0f;
    }

    private void UpdateSoundButtonVisual()
    {
        if (soundButtonImage == null)
        {
            Debug.LogWarning("MainMenuController: soundButtonImage ist nicht zugewiesen.");
            return;
        }

        soundButtonImage.sprite = isSoundEnabled ? soundOnSprite : soundOffSprite;
    }
}