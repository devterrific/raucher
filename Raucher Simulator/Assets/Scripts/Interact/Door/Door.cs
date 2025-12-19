using UnityEngine;
using UnityEngine.SceneManagement;

public class Door : Interactable
{
    public string targetSceneName;
    public bool requireInteract = true;

    public override void Interact(PlayerMain player)
    {
        if (!requireInteract) return;

        LoadScene();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (requireInteract) return;

        if (other.CompareTag("Player"))
        {
            LoadScene();
        }
    }

    private void LoadScene()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("Door: Keine Ziel-Scene gesetzt!");
            return;
        }

        SceneManager.LoadScene(targetSceneName);
    }
}
