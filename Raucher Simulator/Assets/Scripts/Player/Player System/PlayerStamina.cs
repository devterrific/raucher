using UnityEngine;

[DisallowMultipleComponent]
public class PlayerStamina : MonoBehaviour
{
    [Header("Stamina")]
    [Min(0f)][SerializeField] private float maxStamina = 100f;
    [Min(0f)][SerializeField] private float drainPerSec = 20f;
    [Min(0f)][SerializeField] private float regenPerSec = 12f;

    [Header("Sprint Thresholds")]
    [Min(0f)][SerializeField] private float minSprintThreshold = 5f;
    [Min(0f)][SerializeField] private float sprintResumeThreshold = 40f;

    [Min(0f)][SerializeField] private float regenDelay = 0.6f;

    private float currentStamina;
    private float regenCooldown;
    private bool sprintLocked;

    public float CurrentStamina => currentStamina;
    public float MaxStamina => maxStamina;

    public void Initialize()
    {
        currentStamina = maxStamina;
        regenCooldown = 0f;
        sprintLocked = false;
    }

    public bool TickSprint(bool sprintRequested)
    {
        // Entsperren erst ab 40 Stamina
        if (sprintLocked && currentStamina >= sprintResumeThreshold)
            sprintLocked = false;

        // Sprinten nur erlauben, wenn nicht gesperrt und genug Stamina da ist
        if (sprintRequested && !sprintLocked && currentStamina > minSprintThreshold)
        {
            currentStamina = Mathf.Max(0f, currentStamina - drainPerSec * Time.deltaTime);
            regenCooldown = regenDelay;

            // Sobald Stamina unter/gleich Minimum fällt: Sprint sperren
            if (currentStamina <= minSprintThreshold)
                sprintLocked = true;

            return true;
        }

        if (regenCooldown > 0f)
        {
            regenCooldown -= Time.deltaTime;
        }
        else
        {
            currentStamina = Mathf.Min(maxStamina, currentStamina + regenPerSec * Time.deltaTime);
        }

        return false;
    }
}