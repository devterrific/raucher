using UnityEngine;

[DisallowMultipleComponent]
public class PlayerVisuals : MonoBehaviour
{
    private Vector3 baseScale;
    private SpriteRenderer spriteRenderer;
    private Sprite spriteBeforeHide;

    public void Initialize(SpriteRenderer sr)
    {
        spriteRenderer = sr;
        baseScale = transform.localScale;
    }

    public void UpdateFlip(float xInput)
    {
        if (xInput > 0.01f)
        {
            var scale = baseScale;
            scale.x = Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
        else if (xInput < -0.01f)
        {
            var scale = baseScale;
            scale.x = -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
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
