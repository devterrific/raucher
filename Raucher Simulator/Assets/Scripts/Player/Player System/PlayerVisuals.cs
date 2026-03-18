using UnityEngine;

[DisallowMultipleComponent]
public class PlayerVisuals : MonoBehaviour
{
    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    private static readonly int IsSprintingHash = Animator.StringToHash("IsSprinting");

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

    public void UpdateFlip(float xInput)
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

    public void UpdateAnimation(PlayerMovementState.MovementMode mode)
    {
        if (animator == null)
            return;

        bool isMoving = mode == PlayerMovementState.MovementMode.Walk ||
                        mode == PlayerMovementState.MovementMode.Sprint;

        bool isSprinting = mode == PlayerMovementState.MovementMode.Sprint;

        animator.SetBool(IsMovingHash, isMoving);
        animator.SetBool(IsSprintingHash, isSprinting);
    }

    public void EnterHidezone(Sprite hideSprite)
    {
        if (spriteRenderer == null)
            return;

        spriteBeforeHide = spriteRenderer.sprite;

        if (hideSprite != null)
            spriteRenderer.sprite = hideSprite;
    }

    public void ExitHidezone()
    {
        if (spriteRenderer != null && spriteBeforeHide != null)
            spriteRenderer.sprite = spriteBeforeHide;
    }
}