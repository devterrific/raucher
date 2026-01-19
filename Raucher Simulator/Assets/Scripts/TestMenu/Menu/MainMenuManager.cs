using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using System.IO;
using System.Collections.Generic;

public class MainMenuManager : MonoBehaviour
{
    [Header("Main Menu Elements")]
    [SerializeField] private GameObject buttonsPanel;  // Buttons Panel für Ausblenden
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button highscoreButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button quitButton;

    [Header("Panels")]
    [SerializeField] private GameObject newGamePanel;
    [SerializeField] private GameObject highscorePanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject creditsPanel;

    [Header("New Game Panel References")]
    [SerializeField] private TMP_InputField playerNameInputField;
    [SerializeField] private TextMeshProUGUI warningText;
    [SerializeField] private Button playButton;
    [SerializeField] private Button newGameBackButton;

    [Header("Highscore Panel References")]
    [SerializeField] private TextMeshProUGUI highscoreText;
    [SerializeField] private Button highscoreBackButton;

    [Header("Options Panel References")]
    [SerializeField] private Toggle soundToggle;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Button optionsBackButton;

    [Header("Credits Panel References")]
    [SerializeField] private TextMeshProUGUI creditsText;
    [SerializeField] private Button creditsBackButton;

    [Header("Settings")]
    [SerializeField] private string firstSceneName = "Level1";
    [SerializeField] private int maxHighscores = 20;

    private void Start()
    {
        // Initialisiere alle UI-Elemente
        DeactivateAllPanels();

        // Verstecke HUD falls im Main Menu vorhanden
        HideHUDInMainMenu();

        // Lade gespeicherte Einstellungen
        LoadSettings();

        // Lade Highscores
        LoadHighscores();

        // Setze Credits Text
        SetCreditsText();

        // Event Listener für Hauptmenü Buttons
        SetupMainMenuButtons();

        // Event Listener für Panel Buttons
        SetupPanelButtons();

        // Input Field Character Limit und Validation
        if (playerNameInputField != null)
        {
            playerNameInputField.characterLimit = 10;
            playerNameInputField.onValueChanged.AddListener(OnPlayerNameChanged);
        }
    }

    private void HideHUDInMainMenu()
    {
        // Deaktiviere HUD im Main Menu
        PlayerHUDManager hudManager = FindObjectOfType<PlayerHUDManager>();
        if (hudManager != null)
        {
            hudManager.gameObject.SetActive(false);
        }
    }

    private void SetupMainMenuButtons()
    {
        // New Game Button
        if (newGameButton != null)
        {
            newGameButton.onClick.AddListener(() =>
            {
                ActivatePanel(newGamePanel);
                if (buttonsPanel != null) buttonsPanel.SetActive(false);
            });
        }

        // Highscore Button
        if (highscoreButton != null)
        {
            highscoreButton.onClick.AddListener(() =>
            {
                LoadHighscores();
                ActivatePanel(highscorePanel);
                if (buttonsPanel != null) buttonsPanel.SetActive(false);
            });
        }

        // Options Button
        if (optionsButton != null)
        {
            optionsButton.onClick.AddListener(() =>
            {
                ActivatePanel(optionsPanel);
                if (buttonsPanel != null) buttonsPanel.SetActive(false);
            });
        }

        // Credits Button
        if (creditsButton != null)
        {
            creditsButton.onClick.AddListener(() =>
            {
                ActivatePanel(creditsPanel);
                if (buttonsPanel != null) buttonsPanel.SetActive(false);
            });
        }

        // Quit Button
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
        }
    }

    private void SetupPanelButtons()
    {
        // New Game Panel
        if (playButton != null)
        {
            playButton.onClick.AddListener(StartNewGame);
        }

        if (newGameBackButton != null)
        {
            newGameBackButton.onClick.AddListener(() =>
            {
                DeactivateAllPanels();
                if (buttonsPanel != null) buttonsPanel.SetActive(true);
            });
        }

        // Highscore Panel
        if (highscoreBackButton != null)
        {
            highscoreBackButton.onClick.AddListener(() =>
            {
                DeactivateAllPanels();
                if (buttonsPanel != null) buttonsPanel.SetActive(true);
            });
        }

        // Options Panel
        if (optionsBackButton != null)
        {
            optionsBackButton.onClick.AddListener(() =>
            {
                SaveSettings();
                DeactivateAllPanels();
                if (buttonsPanel != null) buttonsPanel.SetActive(true);
            });
        }

        // Credits Panel
        if (creditsBackButton != null)
        {
            creditsBackButton.onClick.AddListener(() =>
            {
                DeactivateAllPanels();
                if (buttonsPanel != null) buttonsPanel.SetActive(true);
            });
        }

        // Options - Sound Toggle
        if (soundToggle != null)
        {
            soundToggle.onValueChanged.AddListener((value) =>
            {
                AudioListener.volume = value ? volumeSlider.value : 0f;
            });
        }

        // Options - Volume Slider
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.AddListener((value) =>
            {
                if (soundToggle.isOn)
                {
                    AudioListener.volume = value;
                }
            });
        }
    }

    private void StartNewGame()
    {
        string playerName = playerNameInputField.text.Trim();

        if (string.IsNullOrEmpty(playerName))
        {
            ShowWarning("Please enter a player name");
            return;
        }

        if (playerName.Length > 10)
        {
            ShowWarning("Player name must be maximum 10 characters");
            return;
        }

        PlayerPrefs.SetString("PlayerName", playerName);
        PlayerPrefs.Save();
        Time.timeScale = 1f;

        // Aktiviere HUD
        PlayerHUDManager hudManager = FindObjectOfType<PlayerHUDManager>();
        if (hudManager != null)
        {
            hudManager.gameObject.SetActive(true);
            hudManager.ResetGame();
        }

        SceneManager.LoadScene(firstSceneName);
    }

    private void ShowWarning(string message)
    {
        if (warningText != null)
        {
            warningText.text = message;
            warningText.gameObject.SetActive(true);
            Invoke(nameof(HideWarning), 3f);
        }
    }

    private void HideWarning()
    {
        if (warningText != null)
        {
            warningText.gameObject.SetActive(false);
        }
    }

    private void OnPlayerNameChanged(string newText)
    {
        if (newText.Length > 10)
        {
            if (warningText != null && !warningText.gameObject.activeSelf)
            {
                warningText.text = "Maximum 10 characters";
                warningText.gameObject.SetActive(true);
            }
        }
        else if (warningText != null && warningText.gameObject.activeSelf &&
                 warningText.text.Contains("10 characters"))
        {
            warningText.gameObject.SetActive(false);
        }
    }

    private void LoadHighscores()
    {
        if (highscoreText == null) return;

        string highscoreDisplay = "HIGHSCORES:\n\n";

        try
        {
            string filePath = Path.Combine(Application.persistentDataPath, "highscores.txt");

            if (File.Exists(filePath))
            {
                string[] allLines = File.ReadAllLines(filePath);

                if (allLines.Length == 0)
                {
                    highscoreDisplay = "No highscores yet!\nBe the first to play!";
                }
                else
                {
                    List<string> sortedLines = new List<string>(allLines);
                    sortedLines.Sort((a, b) =>
                    {
                        try
                        {
                            int scoreA = int.Parse(a.Split('|')[2]);
                            int scoreB = int.Parse(b.Split('|')[2]);
                            return scoreB.CompareTo(scoreA);
                        }
                        catch
                        {
                            return 0;
                        }
                    });

                    int displayCount = Mathf.Min(sortedLines.Count, maxHighscores);

                    for (int i = 0; i < displayCount; i++)
                    {
                        string[] parts = sortedLines[i].Split('|');
                        if (parts.Length >= 3)
                        {
                            string date = parts[0];
                            string name = parts[1];
                            string score = parts[2];

                            string shortDate = date;
                            if (date.Length > 10)
                            {
                                shortDate = date.Substring(0, 10);
                            }

                            highscoreDisplay += $"{i + 1}. {name}: {score} points\n";
                            highscoreDisplay += $"   Date: {shortDate}\n\n";
                        }
                    }
                }
            }
            else
            {
                highscoreDisplay = "No highscores yet!\nBe the first to play!";
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading highscores: {e.Message}");
            highscoreDisplay = "Error loading highscores.\nPlease try again!";
        }

        highscoreText.text = highscoreDisplay;
    }

    private void LoadSettings()
    {
        bool soundEnabled = PlayerPrefs.GetInt("SoundEnabled", 1) == 1;
        float volume = PlayerPrefs.GetFloat("Volume", 0.5f);

        if (soundToggle != null)
        {
            soundToggle.isOn = soundEnabled;
        }

        if (volumeSlider != null)
        {
            volumeSlider.value = volume;
        }

        AudioListener.volume = soundEnabled ? volume : 0f;
    }

    private void SaveSettings()
    {
        if (soundToggle != null)
        {
            PlayerPrefs.SetInt("SoundEnabled", soundToggle.isOn ? 1 : 0);
        }

        if (volumeSlider != null)
        {
            PlayerPrefs.SetFloat("Volume", volumeSlider.value);
        }

        PlayerPrefs.Save();
    }

    private void SetCreditsText()
    {
        if (creditsText == null) return;

        creditsText.text = "SMOKING SIMULATOR\n\n" +
                          "Game Artist: Julia, Josephine, Marie, Nico & Julia\n\n" +
                          "Programming: Nour, Alim & Dennis\n\n" +

                          "Unity Community\n" +
                          "© 2025 Nicotine Studios";
    }

    private void ActivatePanel(GameObject panel)
    {
        DeactivateAllPanels();

        if (panel != null)
        {
            panel.SetActive(true);
        }
    }

    private void DeactivateAllPanels()
    {
        if (newGamePanel != null) newGamePanel.SetActive(false);
        if (highscorePanel != null) highscorePanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);

        HideWarning();
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    private void OnDestroy()
    {
        SaveSettings();
    }
}