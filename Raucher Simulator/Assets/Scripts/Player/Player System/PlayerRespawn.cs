using UnityEngine;

[DisallowMultipleComponent]
public class PlayerRespawn : MonoBehaviour
{
    public void Respawn(Rigidbody2D rb, Transform targetTransform, Vector3 spawnPosition)
    {
        if (rb != null)
        {
            rb.velocity = Vector2.zero; // Fix: linearVelocity gibt es nicht, velocity verwenden
            rb.angularVelocity = 0f;
            rb.position = spawnPosition;
            return;
        }

        targetTransform.position = spawnPosition;
    }
}