using UnityEngine;

public class Hidezone : Interactable
{
    [Header("Side Interact")]
    [SerializeField] private float sideRange = 0.5f;
    [SerializeField] private float sideHeight = 1.2f;

    [Header("Sprite")]
    [SerializeField] private Sprite crouchedSprite;

    private BoxCollider2D box;

    void Awake()
    {
        box = GetComponent<BoxCollider2D>();
    }

    public override bool CanInteract(PlayerMain player)
    {
        if (player == null) return false;
        if (!box) return false;

        return IsNextToObject(player.transform.position);
    }

    public override void Interact(PlayerMain player)
    {
        if (player == null) return;
        if (!box) return;

        if (!IsNextToObject(player.transform.position))
            return;

        // Toggle: wenn diese Hidezone bereits Hidden-Grund ist -> raus, sonst rein
        bool isHiddenHere = player.IsHiddenBy(this);

        if (!isHiddenHere)
        {
            player.EnterHidezone(this, crouchedSprite);
        }
        else
        {
            player.ExitHidezone(this);
        }
    }

    bool IsNextToObject(Vector2 playerPos)
    {
        Vector2 center = box.bounds.center;

        bool leftSide =
            playerPos.x < center.x &&
            Mathf.Abs(playerPos.x - (center.x - box.bounds.extents.x)) <= sideRange &&
            Mathf.Abs(playerPos.y - center.y) <= sideHeight * 0.5f;

        bool rightSide =
            playerPos.x > center.x &&
            Mathf.Abs(playerPos.x - (center.x + box.bounds.extents.x)) <= sideRange &&
            Mathf.Abs(playerPos.y - center.y) <= sideHeight * 0.5f;

        return leftSide || rightSide;
    }
}
