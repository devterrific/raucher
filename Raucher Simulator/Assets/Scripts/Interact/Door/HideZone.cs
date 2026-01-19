using UnityEngine;

public class Hidezone : Interactable
{
    [Header("Side Interact")]
    [SerializeField] private float sideRange = 0.5f;
    [SerializeField] private float sideHeight = 1.2f;

    [Header("Sprite")]
    [SerializeField] private Sprite crouchedSprite;

    private BoxCollider2D box;
    private Sprite normalSprite;
    private bool isHidden = false;

    void Awake()
    {
        box = GetComponent<BoxCollider2D>();
    }

    public override void Interact(PlayerMain player)
    {
        if (player == null) return;
        if (!box) return;

        if (!IsNextToObject(player.transform.position))
            return;

        SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
        if (!sr) return;

        if (normalSprite == null)
            normalSprite = sr.sprite;

        isHidden = !isHidden;

        // versteckt = nicht detectable
        player.SetDetectableExternal(!isHidden);

        // Sprite swap
        sr.sprite = isHidden ? crouchedSprite : normalSprite;
    }

    bool IsNextToObject(Vector2 playerPos)
    {
        Vector2 center = box.bounds.center;

        bool leftSide =
            playerPos.x < center.x &&
            Mathf.Abs(playerPos.x - box.bounds.min.x) < sideRange &&
            Mathf.Abs(playerPos.y - center.y) < sideHeight / 2f;

        bool rightSide =
            playerPos.x > center.x &&
            Mathf.Abs(playerPos.x - box.bounds.max.x) < sideRange &&
            Mathf.Abs(playerPos.y - center.y) < sideHeight / 2f;

        return leftSide || rightSide;
    }

    void OnDrawGizmosSelected()
    {
        BoxCollider2D b = GetComponent<BoxCollider2D>();
        if (!b) return;

        Gizmos.color = Color.green;
        Vector2 center = b.bounds.center;

        Gizmos.DrawWireCube(
            new Vector2(b.bounds.min.x - sideRange / 2f, center.y),
            new Vector2(sideRange, sideHeight)
        );

        Gizmos.DrawWireCube(
            new Vector2(b.bounds.max.x + sideRange / 2f, center.y),
            new Vector2(sideRange, sideHeight)
        );
    }
}
