using UnityEngine;

[DisallowMultipleComponent]
public class PlayerMovementState : MonoBehaviour
{
    public enum MovementMode
    {
        Locked,
        Idle,
        Walk,
        Sprint,
        Sneak
    }

    [Header("Movement Speeds")]
    [Min(0f)][SerializeField] private float walkSpeed = 3.5f;
    [Min(0f)][SerializeField] private float sprintSpeed = 5.5f;
    [Min(0f)][SerializeField] private float sneakSpeed = 1.8f;

    public MovementMode CurrentMode { get; private set; } = MovementMode.Idle;
    public float TargetSpeed { get; private set; }
    public bool IsSneaking => CurrentMode == MovementMode.Sneak;

    public void Resolve(bool canMove, float xInput, bool sneakHeld, bool sprintActive)
    {
        if (!canMove)
        {
            CurrentMode = MovementMode.Locked;
            TargetSpeed = 0f;
            return;
        }

        if (sneakHeld)
        {
            CurrentMode = MovementMode.Sneak;
            TargetSpeed = sneakSpeed;
            return;
        }

        bool isMovingHorizontally = Mathf.Abs(xInput) > 0.01f;
        if (sprintActive && isMovingHorizontally)
        {
            CurrentMode = MovementMode.Sprint;
            TargetSpeed = sprintSpeed;
            return;
        }

        CurrentMode = isMovingHorizontally ? MovementMode.Walk : MovementMode.Idle;
        TargetSpeed = walkSpeed;
    }
}
