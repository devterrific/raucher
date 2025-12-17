using UnityEngine;

public class Hidezone : Interactable
{
    [Header("Side Interact")]
    [SerializeField] private float sideRange = 0.5f;   // wie weit links/rechts
    [SerializeField] private float sideHeight = 1.2f;  // Höhe vom Bereich

    [Header("Sprite")]
    [SerializeField] private Sprite crouchedSprite;

    private BoxCollider2D box;
    private Sprite normalSprite;

    void Awake()
    {
        box = GetComponent<BoxCollider2D>();
    }

    public override void Interact(PlayerMain player)
    {
        if (!IsNextToObject(player.transform.position))
            return;

        SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
        if (!sr) return;

        if (normalSprite == null)
            normalSprite = sr.sprite;

        player.Detectable = !player.Detectable;
        sr.sprite = player.Detectable ? normalSprite : crouchedSprite;
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
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (!box) return;

        Gizmos.color = Color.green;

        Vector2 center = box.bounds.center;

        // links
        Gizmos.DrawWireCube(
            new Vector2(box.bounds.min.x - sideRange / 2f, center.y),
            new Vector2(sideRange, sideHeight)
        );

        // rechts
        Gizmos.DrawWireCube(
            new Vector2(box.bounds.max.x + sideRange / 2f, center.y),
            new Vector2(sideRange, sideHeight)
        );
    }
}
