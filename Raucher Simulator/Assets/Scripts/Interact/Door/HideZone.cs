using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(BoxCollider2D))]
public class Hidezone : Interactable
{
    private enum InteractionSide
    {
        None = 0,
        Left = -1,
        Right = 1
    }

    [Header("Interaction Window")]
    [Tooltip("Distance from the collider's left/right edge that still allows interaction.")]
    [FormerlySerializedAs("sideRange")]
    [SerializeField, Min(0f)] private float sideRange = 0.5f;

    [Tooltip("Base vertical size of the interaction window.")]
    [FormerlySerializedAs("sideHeight")]
    [SerializeField, Min(0f)] private float sideHeight = 1.2f;

    [Tooltip("Additional horizontal tolerance to make interaction less strict.")]
    [SerializeField, Min(0f)] private float horizontalBuffer = 0.2f;

    [Tooltip("Additional vertical tolerance to make interaction less strict.")]
    [SerializeField, Min(0f)] private float verticalBuffer = 0.15f;

    [Header("Interaction Safety")]
    [Tooltip("Minimum time between successful interactions to avoid rapid toggle spam.")]
    [SerializeField, Min(0f)] private float interactionCooldown = 0.15f;

    [Header("Visuals")]
    [SerializeField] private Sprite crouchedSprite;

#if UNITY_EDITOR
    [Header("Debug")]
    [SerializeField] private bool showInteractionGizmos = true;
#endif

    private BoxCollider2D box;
    private float lastInteractionTime = float.NegativeInfinity;

    private void Awake()
    {
        CacheCollider();
    }

    private void OnValidate()
    {
        CacheCollider();
    }

    public override bool CanInteract(PlayerMain player)
    {
        if (!IsReady(player) || IsOnCooldown())
        {
            return false;
        }

        return TryGetValidSide(player.transform.position, out _);
    }

    public override void Interact(PlayerMain player)
    {
        if (!IsReady(player) || IsOnCooldown())
        {
            return;
        }

        if (!TryGetValidSide(player.transform.position, out _))
        {
            return;
        }

        if (player.IsHiddenBy(this))
        {
            player.ExitHidezone(this);
        }
        else
        {
            player.EnterHidezone(this, crouchedSprite);
        }

        lastInteractionTime = Time.time;
    }

    private bool IsReady(PlayerMain player)
    {
        if (player == null)
        {
            return false;
        }

        if (!box)
        {
            CacheCollider();
        }

        return box;
    }

    private bool IsOnCooldown()
    {
        return Time.time < lastInteractionTime + interactionCooldown;
    }

    private bool TryGetValidSide(Vector2 playerPosition, out InteractionSide side)
    {
        side = InteractionSide.None;

        Bounds bounds = box.bounds;
        Vector2 center = bounds.center;

        float maxVerticalDistance = sideHeight * 0.5f + verticalBuffer;
        if (Mathf.Abs(playerPosition.y - center.y) > maxVerticalDistance)
        {
            return false;
        }

        float maxHorizontalDistance = sideRange + horizontalBuffer;
        float leftDistance = Mathf.Abs(playerPosition.x - bounds.min.x);
        float rightDistance = Mathf.Abs(playerPosition.x - bounds.max.x);

        bool canUseLeft = playerPosition.x <= center.x && leftDistance <= maxHorizontalDistance;
        bool canUseRight = playerPosition.x >= center.x && rightDistance <= maxHorizontalDistance;

        if (!canUseLeft && !canUseRight)
        {
            return false;
        }

        if (canUseLeft && canUseRight)
        {
            side = leftDistance <= rightDistance ? InteractionSide.Left : InteractionSide.Right;
            return true;
        }

        side = canUseLeft ? InteractionSide.Left : InteractionSide.Right;
        return true;
    }

    private void CacheCollider()
    {
        if (!box)
        {
            box = GetComponent<BoxCollider2D>();
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!showInteractionGizmos)
        {
            return;
        }

        if (!box)
        {
            CacheCollider();
        }

        if (!box)
        {
            return;
        }

        Bounds bounds = box.bounds;
        float maxHorizontalDistance = sideRange + horizontalBuffer;
        float maxVerticalDistance = sideHeight * 0.5f + verticalBuffer;

        Vector3 zoneSize = new Vector3(maxHorizontalDistance * 2f, maxVerticalDistance * 2f, 0f);
        Vector3 leftCenter = new Vector3(bounds.min.x, bounds.center.y, 0f);
        Vector3 rightCenter = new Vector3(bounds.max.x, bounds.center.y, 0f);

        Gizmos.color = new Color(0.1f, 0.8f, 1f, 0.45f);
        Gizmos.DrawWireCube(leftCenter, zoneSize);
        Gizmos.DrawWireCube(rightCenter, zoneSize);
    }
#endif
}