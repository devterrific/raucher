using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class HighscoreManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text highscoreListText;

    [Header("Settings")]
    [SerializeField] private int maxHighscoresToDisplay = 20;
    [SerializeField] private string fileName = "highscores.txt";

    //  ---     TEST    ---
    [ContextMenu("Add Test Highscore")]
    private void AddTestHighscore()
    {
        AddHighscore("TestPlayer", Random.Range(10, 5000));
    }

    [ContextMenu("Clear Highscores")]
    public void ClearHighscores()
    {
        if (File.Exists(FilePath))
        {
            File.Delete(FilePath);
        }

        highscores.Clear();
        RefreshHighscoreDisplay();

        Debug.Log("Highscores cleared.");
    }
    //  --------------------------------------------------------

    private string FilePath => Path.Combine(Application.persistentDataPath, fileName);

    private readonly List<HighscoreEntry> highscores = new List<HighscoreEntry>();

    private void Awake()
    {
        LoadHighscores();
        RefreshHighscoreDisplay();
    }

    public void AddHighscore(string playerName, int score)
    {
        if (string.IsNullOrWhiteSpace(playerName))
        {
            return;
        }

        highscores.Add(new HighscoreEntry(playerName.Trim(), score));
        SortHighscores();
        TrimHighscores();
        SaveHighscores();
        RefreshHighscoreDisplay();
    }

    public void ReloadAndRefresh()
    {
        LoadHighscores();
        RefreshHighscoreDisplay();
    }

    private void LoadHighscores()
    {
        highscores.Clear();

        if (!File.Exists(FilePath))
        {
            return;
        }

        string[] lines = File.ReadAllLines(FilePath);

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            string[] parts = line.Split('|');

            if (parts.Length != 2)
            {
                continue;
            }

            string playerName = parts[0].Trim();

            if (string.IsNullOrWhiteSpace(playerName))
            {
                continue;
            }

            if (int.TryParse(parts[1], out int score))
            {
                highscores.Add(new HighscoreEntry(playerName, score));
            }
        }

        SortHighscores();
        TrimHighscores();
    }

    private void SaveHighscores()
    {
        List<string> lines = new List<string>();

        foreach (HighscoreEntry entry in highscores)
        {
            lines.Add(entry.PlayerName + "|" + entry.Score);
        }

        File.WriteAllLines(FilePath, lines);
    }

    private void SortHighscores()
    {
        highscores.Sort((a, b) => b.Score.CompareTo(a.Score));
    }

    private void TrimHighscores()
    {
        if (highscores.Count > maxHighscoresToDisplay)
        {
            highscores.RemoveRange(maxHighscoresToDisplay, highscores.Count - maxHighscoresToDisplay);
        }
    }

    private void RefreshHighscoreDisplay()
    {
        if (highscoreListText == null)
        {
            Debug.LogWarning("HighscoreManager: Kein TMP-Text für die Highscore-Anzeige zugewiesen.");
            return;
        }

        if (highscores.Count == 0)
        {
            highscoreListText.text = "No highscores yet.";
            return;
        }

        List<string> lines = new List<string>();

        for (int i = 0; i < highscores.Count; i++)
        {
            HighscoreEntry entry = highscores[i];
            lines.Add((i + 1) + ". " + entry.PlayerName + " - " + entry.Score);
        }

        highscoreListText.text = string.Join("\n", lines);
    }

    [System.Serializable]
    private class HighscoreEntry
    {
        public string PlayerName { get; private set; }
        public int Score { get; private set; }

        public HighscoreEntry(string playerName, int score)
        {
            PlayerName = playerName;
            Score = score;
        }
    }
}