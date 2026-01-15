using UnityEngine;

public static class ScoreService
{
    public static bool CanAddScore()
    {
        if (GameSessionManager.Instance == null)
        {
            return false;
        }

        if (GameSessionManager.Instance.IsSessionRunning == false)
        {
            return false;
        }

        return true;
    }

    public static void AddPoints(int pointsToAdd, string reasonForDebug = "")
    {
        if (CanAddScore() == false)
        {
            Debug.LogWarning("ScoreService: Keine aktive Session – Punkte wurden nicht hinzugefügt.");
            return;
        }

        if (pointsToAdd <= 0)
        {
            return;
        }

        GameSessionManager.Instance.AddScore(pointsToAdd);

        if (string.IsNullOrWhiteSpace(reasonForDebug) == false)
        {
            Debug.Log("ScoreService: +" + pointsToAdd + " Punkte. Grund: " + reasonForDebug);
        }
    }

    public static int GetCurrentScore()
    {
        if (GameSessionManager.Instance == null)
        {
            return 0;
        }

        return GameSessionManager.Instance.CurrentScore;
    }
}
