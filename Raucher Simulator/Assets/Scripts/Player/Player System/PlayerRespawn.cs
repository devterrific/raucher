using UnityEngine;

[DisallowMultipleComponent]
public class PlayerRespawn : MonoBehaviour
{
    public void Respawn(Rigidbody2D rb, Transform targetTransform, Vector3 spawnPosition)
    {
        if (rb != null)
            rb.velocity = Vector2.zero;

        targetTransform.position = spawnPosition;
    }
}