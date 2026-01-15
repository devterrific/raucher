using UnityEngine;
using TMPro;
using System.Text;

public class HighscorePanelUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text highscoreText;
    [SerializeField] private int maxEntriesToShow = 10;

    public void RefreshHighscores()
    {
        if (highscoreText == null)
        {
            return;
        }

        var data = HighscoreDatabase.LoadHighscores();

        if (data.entries.Count == 0)
        {
            highscoreText.text = "Noch keine Highscores vorhanden.";
            return;
        }

        StringBuilder builder = new StringBuilder();
        int shownEntries = Mathf.Min(maxEntriesToShow, data.entries.Count);

        for (int i = 0; i < shownEntries; i++)
        {
            var entry = data.entries[i];
            builder.AppendLine((i + 1) + ". " + entry.playerName + " - " + entry.score + " Punkte (" + entry.dateTime + ")");
        }

        highscoreText.text = builder.ToString();
    }
}
