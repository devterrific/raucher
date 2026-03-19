using UnityEngine;

[DisallowMultipleComponent]
public class PlayerVisuals : MonoBehaviour
{
    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    private static readonly int IsSprintingHash = Animator.StringToHash("IsSprinting");
    private static readonly int IsCrouchingHash = Animator.StringToHash("IsCrouching");

    private Vector3 baseScale;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Sprite spriteBeforeHide;

    public void Initialize(SpriteRenderer sr, Animator anim)
    {
        spriteRenderer = sr;
        animator = anim;
        baseScale = transform.localScale;
    }

    public void UpdateVisuals(float xInput, PlayerMovementState.MovementMode mode, bool crouchHeld)
    {
        UpdateFlip(xInput);
        UpdateAnimation(mode, crouchHeld);
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

    private void UpdateAnimation(PlayerMovementState.MovementMode mode, bool crouchHeld)
    {
        if (animator == null)
            return;

        bool isMoving = false;
        bool isSprinting = false;
        bool isCrouching = false;

        switch (mode)
        {
            case PlayerMovementState.MovementMode.Walk:
                isMoving = true;
                break;

            case PlayerMovementState.MovementMode.Sprint:
                isMoving = true;
                isSprinting = true;
                break;

            case PlayerMovementState.MovementMode.Sneak:
                isCrouching = true;
                break;
        }

        animator.SetBool(IsMovingHash, isMoving);
        animator.SetBool(IsSprintingHash, isSprinting);
        //animator.SetBool(IsCrouchingHash, isCrouching);
    }

    public void EnterHidezone(Sprite hideSprite)
    {
        //if (spriteRenderer == null)
        //    return;

        //spriteBeforeHide = spriteRenderer.sprite;

        //if (hideSprite != null)
        //    spriteRenderer.sprite = hideSprite;


        animator.SetBool("IsCrouching", true);
    }

    public void ExitHidezone()
    {
        //if (spriteRenderer != null && spriteBeforeHide != null)
        //    spriteRenderer.sprite = spriteBeforeHide;

        animator.SetBool("IsCrouching", false);
    }
}