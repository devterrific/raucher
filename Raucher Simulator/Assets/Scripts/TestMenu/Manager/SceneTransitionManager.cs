using UnityEngine;

public class SceneTransitionManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerHUDManager hudManager;

    private void Start()
    {
        // Finde HUD Manager
        if (hudManager == null)
        {
            hudManager = FindObjectOfType<PlayerHUDManager>();
        }

        // Stelle sicher, dass HUD im Main Menu versteckt ist
        if (hudManager != null)
        {
            // Optional: HUD komplett deaktivieren
            hudManager.gameObject.SetActive(false);
        }
    }

    // Diese Methode aufrufen, wenn New Game gestartet wird
    public void StartGameWithHUD()
    {
        if (hudManager != null)
        {
            hudManager.gameObject.SetActive(true);
            hudManager.ResetGame();
        }
    }
}