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