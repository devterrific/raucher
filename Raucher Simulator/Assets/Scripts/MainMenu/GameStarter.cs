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
    }
}