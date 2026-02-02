using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.IO;
using System.Collections.Generic;

public class PlayerHUDManager : MonoBehaviour
{
    [Header("HUD Elements")]
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject hudCanvas;

    [Header("Timer Settings")]
    [SerializeField] private float gameTimeInMinutes = 5f;
    [SerializeField] private bool countDown = true;

    [Header("Score Settings")]
    [SerializeField] private int initialScore = 0;

    [Header("Scene Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private float currentTimeInSeconds;
    private int currentScore;
    private string currentPlayerName;
    private bool isGameActive = false; // Standardmäßig false
    private static PlayerHUDManager instance;

    public int CurrentScore => currentScore;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            
            // HUD initial komplett deaktivieren
            if (hudCanvas != null)
            {
                hudCanvas.SetActive(false);
            }
            
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Überprüfe aktuelle Szene und passe HUD an
        CheckCurrentScene();
    }

    private void Update()
    {
        if (isGameActive)
        {
            UpdateTimer();
        }
    }

    private void CheckCurrentScene()
    {
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (currentScene == mainMenuSceneName)
        {
            // Im Main Menu: HUD deaktivieren
            DeactivateHUD();
        }
        else
        {
            // In Spiel-Szene: HUD aktivieren und initialisieren
            ActivateHUD();
        }
    }

    private void ActivateHUD()
    {
        if (hudCanvas != null)
        {
            hudCanvas.SetActive(true);
        }

        LoadPlayerName();
        ResetTimer();
        ResetScore();
        UpdateHUD();
        isGameActive = true;
    }

    private void DeactivateHUD()
    {
        if (hudCanvas != null)
        {
            hudCanvas.SetActive(false);
        }
        isGameActive = false;
    }

    private void LoadPlayerName()
    {
        currentPlayerName = PlayerPrefs.GetString("PlayerName", "Player");

        if (playerNameText != null)
        {
            playerNameText.text = $"Player: {currentPlayerName}";
        }
    }

    private void ResetTimer()
    {
        if (countDown)
        {
            currentTimeInSeconds = gameTimeInMinutes * 60f;
        }
        else
        {
            currentTimeInSeconds = 0f;
        }
        UpdateTimerDisplay();
    }

    private void UpdateTimer()
    {
        if (countDown)
        {
            currentTimeInSeconds -= Time.deltaTime;

            if (currentTimeInSeconds <= 0f)
            {
                currentTimeInSeconds = 0f;
                isGameActive = false;
                OnTimeExpired();
            }
        }
        else
        {
            currentTimeInSeconds += Time.deltaTime;
        }

        UpdateTimerDisplay();
    }

    private void UpdateTimerDisplay()
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(currentTimeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(currentTimeInSeconds % 60f);
        timerText.text = $"Time: {minutes:00}:{seconds:00}";
    }

    private void ResetScore()
    {
        currentScore = initialScore;
        UpdateScoreDisplay();
    }

    public void AddScore(int points)
    {
        if (!isGameActive) return;

        currentScore += points;
        if (currentScore < 0) currentScore = 0;
        UpdateScoreDisplay();
    }

    public void SetScore(int newScore)
    {
        if (!isGameActive) return;

        currentScore = newScore;
        if (currentScore < 0) currentScore = 0;
        UpdateScoreDisplay();
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {currentScore}";
        }
    }

    public void SaveHighscore()
    {
        if (string.IsNullOrEmpty(currentPlayerName) || currentScore <= 0) return;

        SaveHighscoreToFile(currentPlayerName, currentScore);
    }

    private void SaveHighscoreToFile(string playerName, int score)
    {
        try
        {
            string filePath = Path.Combine(Application.persistentDataPath, "highscores.txt");
            string entry = $"{DateTime.Now:yyyy-MM-dd HH:mm}|{playerName}|{score}";

            if (File.Exists(filePath))
            {
                List<string> entries = new List<string>(File.ReadAllLines(filePath));
                entries.Add(entry);

                entries.Sort((a, b) =>
                {
                    int scoreA = int.Parse(a.Split('|')[2]);
                    int scoreB = int.Parse(b.Split('|')[2]);
                    return scoreB.CompareTo(scoreA);
                });

                if (entries.Count > 20)
                {
                    entries = entries.GetRange(0, 20);
                }

                File.WriteAllLines(filePath, entries);
            }
            else
            {
                File.WriteAllText(filePath, entry);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving highscore: {e.Message}");
        }
    }

    private void OnTimeExpired()
    {
        SaveHighscore();
    }

    private void UpdateHUD()
    {
        LoadPlayerName();
        UpdateTimerDisplay();
        UpdateScoreDisplay();
    }

    public void SetGameActive(bool active)
    {
        isGameActive = active;
    }

    public void ResetGame()
    {
        ResetTimer();
        ResetScore();
        isGameActive = true;
        UpdateHUD();
    }

    public void SetHUDVisible(bool visible)
    {
        if (hudCanvas != null)
        {
            hudCanvas.SetActive(visible);
        }
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        CheckCurrentScene();
    }

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (currentScore > 0)
        {
            SaveHighscore();
        }
    }
}