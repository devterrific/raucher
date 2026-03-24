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

    private enum HorizontalDirection
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

    [Header("Snitch Logic")]
    [SerializeField] private string snitchTag = "Snitch";
    [SerializeField, Min(0f)] private float directionEpsilon = 0.001f;
    [SerializeField] private bool requireSnitchRule = true;

    [Header("Visuals")]
    [SerializeField] private Sprite crouchedSprite;

#if UNITY_EDITOR
    [Header("Debug")]
    [SerializeField] private bool showInteractionGizmos = true;
    [SerializeField] private bool logDebug = false;
#endif

    private BoxCollider2D box;
    private Transform snitchTransform;

    private float previousSnitchX;
    private float currentSnitchX;
    private bool hasSnitchSamples;

    private float lastInteractionTime = float.NegativeInfinity;

    private void Awake()
    {
        CacheCollider();
        CacheSnitch();
        InitializeSnitchTracking();
    }

    private void Update()
    {
        UpdateSnitchTracking();
    }

    private void OnValidate()
    {
        CacheCollider();
    }

    public override bool CanInteract(PlayerMain player)
    {
        if (!IsReady(player) || IsOnCooldown())
            return false;

        if (!TryGetValidSide(player.transform.position, out InteractionSide playerSide))
            return false;

        if (player.IsHiddenBy(this))
            return true;

        return CanEnterFromSide(playerSide);
    }

    public override void Interact(PlayerMain player)
    {
        if (!IsReady(player) || IsOnCooldown())
            return;

        if (!TryGetValidSide(player.transform.position, out InteractionSide playerSide))
            return;

        if (player.IsHiddenBy(this))
        {
            player.ExitHidezone(this);
            lastInteractionTime = Time.time;
            return;
        }

        if (!CanEnterFromSide(playerSide))
            return;

        player.EnterHidezone(this, crouchedSprite);
        lastInteractionTime = Time.time;
    }

    private bool IsReady(PlayerMain player)
    {
        if (player == null)
            return false;

        if (!box)
            CacheCollider();

        CacheSnitch();

        return box != null;
    }

    private bool IsOnCooldown()
    {
        return Time.time < lastInteractionTime + interactionCooldown;
    }

    private bool CanEnterFromSide(InteractionSide playerSide)
    {
        if (!requireSnitchRule)
            return true;

        if (snitchTransform == null)
        {
            LogDebug("Snitch nicht gefunden -> fallback auf normales Verstecken.");
            return true;
        }

        HorizontalDirection snitchDirection = GetSnitchMoveDirection();
        if (snitchDirection == HorizontalDirection.None)
        {
            LogDebug("Snitch-Richtung unklar -> fallback auf normales Verstecken.");
            return true;
        }

        InteractionSide allowedSide = GetAllowedEnterSide(snitchTransform.position.x, snitchDirection);
        if (allowedSide == InteractionSide.None)
        {
            LogDebug("Snitch ist schon vorbei oder entfernt sich -> Enter blockiert.");
            return false;
        }

        bool allowed = playerSide == allowedSide;
        LogDebug("PlayerSide: " + playerSide + " | AllowedSide: " + allowedSide + " | Allowed: " + allowed);
        return allowed;
    }

    private InteractionSide GetAllowedEnterSide(float snitchX, HorizontalDirection snitchDirection)
    {
        if (!box)
            return InteractionSide.None;

        float hidezoneCenterX = box.bounds.center.x;

        bool snitchIsRightOfHidezone = snitchX > hidezoneCenterX;
        bool snitchIsLeftOfHidezone = snitchX < hidezoneCenterX;

        if (snitchIsRightOfHidezone && snitchDirection == HorizontalDirection.Left)
            return InteractionSide.Left;

        if (snitchIsLeftOfHidezone && snitchDirection == HorizontalDirection.Right)
            return InteractionSide.Right;

        return InteractionSide.None;
    }

    private HorizontalDirection GetSnitchMoveDirection()
    {
        if (snitchTransform == null || !hasSnitchSamples)
            return HorizontalDirection.None;

        float deltaX = currentSnitchX - previousSnitchX;

        if (deltaX > directionEpsilon)
            return HorizontalDirection.Right;

        if (deltaX < -directionEpsilon)
            return HorizontalDirection.Left;

        return HorizontalDirection.None;
    }

    private void CacheSnitch()
    {
        if (snitchTransform != null)
            return;

        GameObject snitchObject = GameObject.FindGameObjectWithTag(snitchTag);
        if (snitchObject != null)
            snitchTransform = snitchObject.transform;
    }

    private void InitializeSnitchTracking()
    {
        if (snitchTransform == null)
            return;

        float startX = snitchTransform.position.x;
        previousSnitchX = startX;
        currentSnitchX = startX;
        hasSnitchSamples = true;
    }

    private void UpdateSnitchTracking()
    {
        CacheSnitch();

        if (snitchTransform == null)
            return;

        float newX = snitchTransform.position.x;

        if (!hasSnitchSamples)
        {
            previousSnitchX = newX;
            currentSnitchX = newX;
            hasSnitchSamples = true;
            return;
        }

        previousSnitchX = currentSnitchX;
        currentSnitchX = newX;
    }

    private bool TryGetValidSide(Vector2 playerPosition, out InteractionSide side)
    {
        side = InteractionSide.None;

        if (!box)
            return false;

        Bounds bounds = box.bounds;
        Vector2 center = bounds.center;

        float maxVerticalDistance = sideHeight * 0.5f + verticalBuffer;
        if (Mathf.Abs(playerPosition.y - center.y) > maxVerticalDistance)
            return false;

        float maxHorizontalDistance = sideRange + horizontalBuffer;
        float leftDistance = Mathf.Abs(playerPosition.x - bounds.min.x);
        float rightDistance = Mathf.Abs(playerPosition.x - bounds.max.x);

        bool canUseLeft = playerPosition.x <= center.x && leftDistance <= maxHorizontalDistance;
        bool canUseRight = playerPosition.x >= center.x && rightDistance <= maxHorizontalDistance;

        if (!canUseLeft && !canUseRight)
            return false;

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
            box = GetComponent<BoxCollider2D>();
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void LogDebug(string message)
    {
#if UNITY_EDITOR
        if (logDebug)
            Debug.Log("[Hidezone] " + message, this);
#endif
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!showInteractionGizmos)
            return;

        if (!box)
            CacheCollider();

        if (!box)
            return;

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