using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public static class HighscoreFileUtility
{
    private const string FileName = "highscores.txt";
    private const int MaxHighscores = 20;

    private static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

    public static void AddHighscore(string playerName, int score)
    {
        if (string.IsNullOrWhiteSpace(playerName))
        {
            return;
        }

        List<HighscoreEntry> entries = LoadHighscores();

        entries.Add(new HighscoreEntry(playerName.Trim(), score));

        entries = entries
            .OrderByDescending(entry => entry.Score)
            .Take(MaxHighscores)
            .ToList();

        SaveHighscores(entries);
    }

    private static List<HighscoreEntry> LoadHighscores()
    {
        List<HighscoreEntry> entries = new List<HighscoreEntry>();

        if (!File.Exists(FilePath))
        {
            return entries;
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
                entries.Add(new HighscoreEntry(playerName, score));
            }
        }

        return entries;
    }

    private static void SaveHighscores(List<HighscoreEntry> entries)
    {
        List<string> lines = new List<string>();

        foreach (HighscoreEntry entry in entries)
        {
            lines.Add(entry.PlayerName + "|" + entry.Score);
        }

        File.WriteAllLines(FilePath, lines);
    }

    private class HighscoreEntry
    {
        public string PlayerName { get; }
        public int Score { get; }

        public HighscoreEntry(string playerName, int score)
        {
            PlayerName = playerName;
            Score = score;
        }
    }
}