using UnityEngine;

[DisallowMultipleComponent]
public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private float interactRange = 1.5f;
    [SerializeField] private LayerMask interactLayer;

    public void TryInteract(PlayerMain player, bool interactPressed)
    {
        if (!interactPressed)
            return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactRange, interactLayer);

        Interactable best = null;
        float bestSqrDistance = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            Interactable candidate = hits[i].GetComponent<Interactable>();
            if (candidate == null || !candidate.CanInteract(player))
                continue;

            float sqrDist = (hits[i].transform.position - transform.position).sqrMagnitude;
            if (sqrDist < bestSqrDistance)
            {
                bestSqrDistance = sqrDist;
                best = candidate;
            }
        }

        if (best != null)
            best.Interact(player);
    }
}