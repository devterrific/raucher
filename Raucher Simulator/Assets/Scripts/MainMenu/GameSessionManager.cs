using System.Collections.Generic;
using UnityEngine;

public class GameSessionManager : MonoBehaviour
{
    public static GameSessionManager Instance { get; private set; }

    private const string PlayerNameKey = "PlayerName";

    public string PlayerName { get; private set; }
    public int CurrentScore { get; private set; }
    public bool IsSessionActive => sessionActive;

    private bool sessionActive = false;
    private bool scoreSaved = false;

    // Dient für das Merken, in welcher Szene die SpeechBubble schon geladen wurde
    private readonly HashSet<string> usedHintScenes = new HashSet<string>();

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

    public void StartSession()
    {
        PlayerName = PlayerPrefs.GetString(PlayerNameKey, string.Empty);
        CurrentScore = 0;

        sessionActive = true;
        scoreSaved = false;

        // Neue Game Session = Hinweise wieder freigeben
        usedHintScenes.Clear();
    }

    public void AddScore(int points)
    {
        if (!sessionActive)
        {
            return;
        }

        CurrentScore += points;
    }

    public void EndSession()
    {
        if (!sessionActive)
        {
            return;
        }

        sessionActive = false;

        if (!scoreSaved)
        {
            SaveHighscore();
            scoreSaved = true;
        }

        ResetSessionData();
    }

    public bool HasSceneHintBeenUsed(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            return false;
        }

        return usedHintScenes.Contains(sceneName);
    }

    public void MarkSceneHintAsUsed(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            return;
        }

        usedHintScenes.Add(sceneName);
    }

    private void SaveHighscore()
    {
        if (CurrentScore <= 0)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(PlayerName))
        {
            return;
        }

        HighscoreFileUtility.AddHighscore(PlayerName, CurrentScore);
    }

    private void ResetSessionData()
    {
        PlayerName = string.Empty;
        CurrentScore = 0;

        PlayerPrefs.DeleteKey(PlayerNameKey);
        PlayerPrefs.Save();
    }
}