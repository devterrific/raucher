using UnityEngine;

public class GameStarter : MonoBehaviour
{
    private void Start()
    {
        if (GameSessionManager.Instance != null)
        {
            GameSessionManager.Instance.StartSession();
        }
    }
}