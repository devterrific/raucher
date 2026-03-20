using UnityEngine;

public class Interactable : MonoBehaviour
{
    [Tooltip("Wie nah der Spieler sein muss, um zu interagieren (optional, falls du es pro Objekt brauchst).")]
    public float interactRange = 1.5f;

    [Tooltip("Text für UI (optional).")]
    public string prompt = "Press E";

    /// <summary>
    /// Kann überschrieben werden, um Bedingungen zu prüfen (z.B. Schlüssel, Queststate, Cooldown).
    /// </summary>
    public virtual bool CanInteract(PlayerMain player)
    {
        return player != null;
    }

    /// <summary>
    /// Wird aufgerufen, wenn der Spieler interagiert.
    /// </summary>
    public virtual void Interact(PlayerMain player)
    {
        Debug.Log($"{gameObject.name} wurde interagiert.");
    }

    /// <summary>
    /// Optionales Hilfs-Tool, falls du pro Interactable Distanz prüfen willst.
    /// playerPrefab nutzt aktuell OverlapCircle -> das ist okay.
    /// </summary>
    public bool IsInRange(Transform player)
    {
        return Vector2.Distance(player.position, transform.position) <= interactRange;
    }
}
