using System.IO;
using UnityEngine;
using System.Linq;

public static class HighscoreDatabase
{
    private static string GetFilePath()
    {
        return Path.Combine(Application.persistentDataPath, "highscores.json");
    }

    public static HighscoreData LoadHighscores()
    {
        string filePath = GetFilePath();

        if (File.Exists(filePath) == false)
        {
            return new HighscoreData();
        }

        string jsonText = File.ReadAllText(filePath);
        HighscoreData loadedData = JsonUtility.FromJson<HighscoreData>(jsonText);

        if (loadedData == null)
        {
            return new HighscoreData();
        }

        if (loadedData.entries == null)
        {
            loadedData.entries = new System.Collections.Generic.List<HighscoreEntry>();
        }

        return loadedData;
    }

    public static void SaveHighscores(HighscoreData data)
    {
        string filePath = GetFilePath();
        string jsonText = JsonUtility.ToJson(data, true);
        File.WriteAllText(filePath, jsonText);
    }

    public static void AddHighscore(string playerName, int score)
    {
        HighscoreData data = LoadHighscores();

        data.entries.Add(new HighscoreEntry(playerName, score));

        // Sortierung: höchster Score zuerst
        data.entries = data.entries
            .OrderByDescending(entry => entry.score)
            .ToList();

        // Optional: nur Top 20 speichern
        if (data.entries.Count > 20)
        {
            data.entries.RemoveRange(20, data.entries.Count - 20);
        }

        SaveHighscores(data);
    }
}
