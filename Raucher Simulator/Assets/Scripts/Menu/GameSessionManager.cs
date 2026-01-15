using UnityEngine;

public class GameSessionManager : MonoBehaviour
{
    public static GameSessionManager Instance { get; private set; }

    public string PlayerName { get; private set; }
    public int CurrentScore { get; private set; }

    public float RemainingTimeInSeconds { get; private set; }
    public bool IsSessionRunning { get; private set; }

    [SerializeField] private float startTimeInSeconds = 300f;

    private bool hasSavedThisSession;

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

    private void Update()
    {
        if (IsSessionRunning == false)
        {
            return;
        }

        RemainingTimeInSeconds -= Time.deltaTime;

        if (RemainingTimeInSeconds < 0f)
        {
            RemainingTimeInSeconds = 0f;
        }
    }

    public void StartNewSession(string playerName)
    {
        PlayerName = playerName;
        CurrentScore = 0;

        RemainingTimeInSeconds = startTimeInSeconds;
        IsSessionRunning = true;

        hasSavedThisSession = false;

        PlayerPrefs.SetString("LastPlayerName", PlayerName);
        PlayerPrefs.Save();
    }

    public void AddScore(int pointsToAdd)
    {
        if (IsSessionRunning == false)
        {
            return;
        }

        if (pointsToAdd < 0)
        {
            pointsToAdd = 0;
        }

        CurrentScore += pointsToAdd;
    }

    public void StopSessionTimer()
    {
        IsSessionRunning = false;
    }

    public void FinishSessionAndSaveToHighscores()
    {
        if (hasSavedThisSession == true)
        {
            return;
        }

        // Nur speichern, wenn Punkte > 0
        if (CurrentScore <= 0)
        {
            hasSavedThisSession = true;
            return;
        }

        if (string.IsNullOrWhiteSpace(PlayerName))
        {
            PlayerName = "Unknown";
        }

        HighscoreDatabase.AddHighscore(PlayerName, CurrentScore);
        hasSavedThisSession = true;
    }

    public void ResetSessionCompletely()
    {
        // Reset für neuen Run
        PlayerName = string.Empty;
        CurrentScore = 0;
        RemainingTimeInSeconds = 0f;
        IsSessionRunning = false;

        hasSavedThisSession = false;
    }

    private void OnApplicationQuit()
    {
        FinishSessionAndSaveToHighscores();
    }
}
