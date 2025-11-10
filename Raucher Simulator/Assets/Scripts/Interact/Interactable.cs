using UnityEngine;

public class Interactable : MonoBehaviour
{
    [Tooltip("Wie nah der Spieler sein muss, um zu interagieren.")]
    public float interactRange = 1.5f;

    [Tooltip("Text für UI (optional).")]
    public string prompt = "Press E";

    // Wird aufgerufen, wenn der Spieler E drückt und nah genug ist
    public virtual void Interact(PlayerMain player)
    {
        Debug.Log($"{gameObject.name} wurde interagiert.");
    }

    // Hilfsmethode (optional)
    public bool IsInRange(Transform player)
    {
        return Vector2.Distance(player.position, transform.position) <= interactRange;
    }
}
