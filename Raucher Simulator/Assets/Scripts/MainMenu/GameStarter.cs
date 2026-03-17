using UnityEngine;

public class GameStarter : MonoBehaviour
{
    private void Start()
    {
        if (GameSessionManager.Instance == null)
        {
            return;
        }

        if (GameSessionManager.Instance.IsSessionActive)
        {
            return;
        }

        GameSessionManager.Instance.StartSession();

        if (PlayerHUDManager.Instance != null)
        {
            PlayerHUDManager.Instance.StartRound();
            PlayerHUDManager.Instance.ShowHud();
        }
    }
}