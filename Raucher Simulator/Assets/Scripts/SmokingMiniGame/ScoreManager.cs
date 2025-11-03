using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public int RunScore { get; private set; }

    public void Add(int p) => RunScore += p;
    public void ResetRun() => RunScore = 0;

    private const string TotalKey = "GlobalHighscore_Total";
    public void AddRunToTotal() => PlayerPrefs.SetInt(TotalKey, PlayerPrefs.GetInt(TotalKey, 0) + RunScore);
    public static int GetTotal() => PlayerPrefs.GetInt(TotalKey, 0);
}
