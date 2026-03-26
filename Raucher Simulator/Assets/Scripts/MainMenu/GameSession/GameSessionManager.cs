using UnityEngine;

public class GameSessionManager : MonoBehaviour
{
    public static GameSessionManager Instance { get; private set; }

    private const string PlayerNameKey = "PlayerName";

    public string PlayerName { get; private set; }
    public int CurrentScore { get; private set; }
    public bool IsSessionActive => sessionActive;

    // Pause Hint Zustand für die aktuelle Game Session
    public bool IsSmallPauseHintVisible { get; private set; }
    public bool IsLargePauseHintOpen { get; private set; }
    public int CurrentPauseHintPageIndex { get; private set; }

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

        ResetPauseHintState();
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

    public void OpenLargePauseHint(int pageIndex)
    {
        IsSmallPauseHintVisible = true;
        IsLargePauseHintOpen = true;
        CurrentPauseHintPageIndex = Mathf.Max(0, pageIndex);
    }

    public void CloseLargePauseHint()
    {
        IsSmallPauseHintVisible = true;
        IsLargePauseHintOpen = false;
    }

    public void SetCurrentPauseHintPage(int pageIndex)
    {
        CurrentPauseHintPageIndex = Mathf.Max(0, pageIndex);
    }

    public void HideSmallPauseHint()
    {
        IsSmallPauseHintVisible = false;
    }

    public void ShowSmallPauseHint()
    {
        IsSmallPauseHintVisible = true;
    }

    public void ResetPauseHintState()
    {
        IsSmallPauseHintVisible = true;
        IsLargePauseHintOpen = false;
        CurrentPauseHintPageIndex = 0;
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

        ResetPauseHintState();

        PlayerPrefs.DeleteKey(PlayerNameKey);
        PlayerPrefs.Save();
    }
}