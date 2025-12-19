using UnityEngine;

public class RightExitToSmoking : MonoBehaviour
{
    public string targetScene = "Smoking Mini Game";
    private bool used = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("[RightExitToSmoking] Trigger ENTER: " + other.name + " tag=" + other.tag);

        if (used) return;
        if (!other.CompareTag("Player")) return;

        used = true;

        Debug.Log("[RightExitToSmoking] LOAD -> " + targetScene);

        if (SpawnManager.Instance == null)
        {
            Debug.LogError("[RightExitToSmoking] SpawnManager.Instance ist NULL");
            return;
        }

        SpawnManager.Instance.LoadScene(targetScene);
    }
}
