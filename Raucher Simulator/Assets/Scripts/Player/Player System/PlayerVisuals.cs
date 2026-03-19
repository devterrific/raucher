using UnityEngine;

[DisallowMultipleComponent]
public class PlayerVisuals : MonoBehaviour
{
    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    private static readonly int IsSprintingHash = Animator.StringToHash("IsSprinting");
    private static readonly int IsCrouchingHash = Animator.StringToHash("IsCrouching");

    private Vector3 baseScale;
    private Animator animator;

    private bool isInHidezone;

    public void Initialize(SpriteRenderer sr, Animator anim)
    {
        animator = anim;
        baseScale = transform.localScale;
    }

    public void UpdateVisuals(float xInput, PlayerMovementState.MovementMode mode, bool crouchHeld)
    {
        UpdateFlip(xInput);
        UpdateAnimation(xInput, mode, crouchHeld);
    }

    private void UpdateFlip(float xInput)
    {
        if (xInput > 0.01f)
        {
            Vector3 scale = baseScale;
            scale.x = Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
        else if (xInput < -0.01f)
        {
            Vector3 scale = baseScale;
            scale.x = -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }

    private void UpdateAnimation(float xInput, PlayerMovementState.MovementMode mode, bool crouchHeld)
    {
        if (animator == null)
            return;

        bool isCrouching = crouchHeld || isInHidezone;
        bool isMoving = false;
        bool isSprinting = false;

        if (!isCrouching)
        {
            switch (mode)
            {
                case PlayerMovementState.MovementMode.Walk:
                    isMoving = Mathf.Abs(xInput) > 0.01f;
                    break;

                case PlayerMovementState.MovementMode.Sprint:
                    isMoving = Mathf.Abs(xInput) > 0.01f;
                    isSprinting = isMoving;
                    break;
            }
        }

        animator.SetBool(IsMovingHash, isMoving);
        animator.SetBool(IsSprintingHash, isSprinting);
        animator.SetBool(IsCrouchingHash, isCrouching);
    }

    public void EnterHidezone(Sprite hideSprite)
    {
        isInHidezone = true;
    }

    public void ExitHidezone()
    {
        isInHidezone = false;
    }
}