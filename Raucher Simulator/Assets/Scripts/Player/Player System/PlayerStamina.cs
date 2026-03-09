using UnityEngine;

[DisallowMultipleComponent]
public class PlayerStamina : MonoBehaviour
{
    [Header("Stamina")]
    [Min(0f)][SerializeField] private float maxStamina = 100f;
    [Min(0f)][SerializeField] private float drainPerSec = 20f;
    [Min(0f)][SerializeField] private float regenPerSec = 12f;
    [Min(0f)][SerializeField] private float minSprintThreshold = 5f;
    [Min(0f)][SerializeField] private float regenDelay = 0.6f;

    private float currentStamina;
    private float regenCooldown;

    public float CurrentStamina => currentStamina;
    public bool CanStartOrMaintainSprint => currentStamina > minSprintThreshold;

    public void Initialize()
    {
        currentStamina = maxStamina;
        regenCooldown = 0f;
    }

    public bool TickSprint(bool sprintRequested)
    {
        if (sprintRequested && CanStartOrMaintainSprint)
        {
            currentStamina = Mathf.Max(0f, currentStamina - drainPerSec * Time.deltaTime);
            regenCooldown = regenDelay;
            return true;
        }

        if (regenCooldown > 0f)
            regenCooldown -= Time.deltaTime;
        else
            currentStamina = Mathf.Min(maxStamina, currentStamina + regenPerSec * Time.deltaTime);

        return false;
    }
}