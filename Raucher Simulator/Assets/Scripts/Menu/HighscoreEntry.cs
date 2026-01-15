using System;

[Serializable]
public class HighscoreEntry
{
    public string playerName;
    public int score;
    public string dateTime;

    public HighscoreEntry(string playerName, int score)
    {
        this.playerName = playerName;
        this.score = score;
        this.dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
    }
}