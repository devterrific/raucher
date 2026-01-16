using UnityEngine;
using System.Collections.Generic;

public static class HighscoreManager
{
    private const int MaxHighscores = 20;

    [System.Serializable]
    public struct HighscoreEntry
    {
        public string playerName;
        public int score;
    }

    public static List<HighscoreEntry> GetHighscores()
    {
        List<HighscoreEntry> highscores = new List<HighscoreEntry>();

        for (int i = 1; i <= MaxHighscores; i++)
        {
            if (PlayerPrefs.HasKey($"Highscore_{i}"))
            {
                HighscoreEntry entry = new HighscoreEntry
                {
                    playerName = PlayerPrefs.GetString($"HighscoreName_{i}", "Unknown"),
                    score = PlayerPrefs.GetInt($"Highscore_{i}", 0)
                };
                highscores.Add(entry);
            }
        }

        return highscores;
    }

    public static void AddHighscore(string playerName, int score)
    {
        List<HighscoreEntry> highscores = GetHighscores();

        // Füge neuen Score hinzu
        HighscoreEntry newEntry = new HighscoreEntry
        {
            playerName = playerName,
            score = score
        };
        highscores.Add(newEntry);

        // Sortiere absteigend nach Score
        highscores.Sort((a, b) => b.score.CompareTo(a.score));

        // Behalte nur die besten Scores
        if (highscores.Count > MaxHighscores)
        {
            highscores.RemoveRange(MaxHighscores, highscores.Count - MaxHighscores);
        }

        // Speichere zurück in PlayerPrefs
        for (int i = 0; i < highscores.Count; i++)
        {
            PlayerPrefs.SetString($"HighscoreName_{i + 1}", highscores[i].playerName);
            PlayerPrefs.SetInt($"Highscore_{i + 1}", highscores[i].score);
        }

        PlayerPrefs.Save();
    }

    public static void ClearHighscores()
    {
        for (int i = 1; i <= MaxHighscores; i++)
        {
            PlayerPrefs.DeleteKey($"HighscoreName_{i}");
            PlayerPrefs.DeleteKey($"Highscore_{i}");
        }
        PlayerPrefs.Save();
    }
}