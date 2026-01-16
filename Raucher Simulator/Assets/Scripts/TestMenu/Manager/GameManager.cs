using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("TestMenu");
    }

    // Call this when player finishes level
    public void FinishLevel(int score)
    {
        string playerName = PlayerPrefs.GetString("PlayerName", "Player");
        HighscoreManager.AddHighscore(playerName, score);
        ReturnToMainMenu();
    }
}