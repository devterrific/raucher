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
    [Tooltip("How far left/right from the collider edge the player may stand to interact.")]
    [FormerlySerializedAs("sideRange")]
    [SerializeField, Min(0f)] private float sideRange = 1.2f;

    [Tooltip("Base vertical size of the interaction window.")]
    [FormerlySerializedAs("sideHeight")]
    [SerializeField, Min(0f)] private float sideHeight = 2f;

    [Tooltip("Extra horizontal tolerance so interaction is less strict.")]
    [SerializeField, Min(0f)] private float horizontalBuffer = 0.5f;

    [Tooltip("Extra vertical tolerance so interaction is less strict.")]
    [SerializeField, Min(0f)] private float verticalBuffer = 0.35f;

    [Tooltip("Near the center of the hidezone, choose the nearer side only if needed.")]
    [SerializeField, Min(0f)] private float centerDeadzone = 0.05f;

    [Header("Interaction Safety")]
    [Tooltip("Minimum time between successful interactions to avoid spam toggling.")]
    [SerializeField, Min(0f)] private float interactionCooldown = 0.15f;

    [Header("Snitch Logic")]
    [SerializeField] private bool requireSnitchRule = true;
    [SerializeField] private string snitchTag = "Snitch";

    [Tooltip("Minimum X movement needed before direction updates.")]
    [SerializeField, Min(0f)] private float directionThreshold = 0.005f;

    [Tooltip("Keep the last valid snitch direction alive for a short time if movement becomes tiny.")]
    [SerializeField, Min(0f)] private float directionMemoryTime = 0.2f;

    [Tooltip("Ignore absurd X jumps like loop teleports.")]
    [SerializeField, Min(0f)] private float teleportThreshold = 10f;

    [Header("Fallback Behaviour")]
    [Tooltip("If snitch is missing, allow hiding instead of blocking everything.")]
    [SerializeField] private bool allowHideWhenSnitchMissing = true;

    [Tooltip("If snitch direction is unclear, allow hiding instead of blocking everything.")]
    [SerializeField] private bool allowHideWhenDirectionUnknown = true;

    [Header("Visuals")]
    [SerializeField] private Sprite crouchedSprite;

#if UNITY_EDITOR
    [Header("Debug")]
    [SerializeField] private bool logDebug = false;
    [SerializeField] private bool showInteractionGizmos = true;
#endif

    private BoxCollider2D box;
    private Transform snitchTransform;

    private float previousSnitchX;
    private bool hasSnitchSample;

    private HorizontalDirection lastStableDirection = HorizontalDirection.None;
    private float lastStableDirectionTime = float.NegativeInfinity;

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

        if (player.IsHiddenBy(this))
            return true;

        if (!TryGetValidSide(player.transform.position, out InteractionSide playerSide))
            return false;

        return CanEnterFromSide(playerSide);
    }

    public override void Interact(PlayerMain player)
    {
        if (!IsReady(player) || IsOnCooldown())
            return;

        if (player.IsHiddenBy(this))
        {
            player.ExitHidezone(this);
            lastInteractionTime = Time.time;
            return;
        }

        if (!TryGetValidSide(player.transform.position, out InteractionSide playerSide))
            return;

        if (!CanEnterFromSide(playerSide))
            return;

        player.EnterHidezone(this, crouchedSprite);
        lastInteractionTime = Time.time;
    }

    private bool IsReady(PlayerMain player)
    {
        if (player == null)
            return false;

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
            LogDebug("Snitch nicht gefunden.");
            return allowHideWhenSnitchMissing;
        }

        HorizontalDirection snitchDirection = GetStableSnitchDirection();

        if (snitchDirection == HorizontalDirection.None)
        {
            LogDebug("Snitch-Richtung unklar.");
            return allowHideWhenDirectionUnknown;
        }

        InteractionSide allowedSide = GetAllowedEnterSide(snitchDirection);

        if (allowedSide == InteractionSide.None)
        {
            LogDebug("Keine erlaubte Seite bestimmbar.");
            return false;
        }

        bool allowed = playerSide == allowedSide;
        LogDebug("PlayerSide: " + playerSide + " | SnitchDirection: " + snitchDirection + " | AllowedSide: " + allowedSide + " | Allowed: " + allowed);
        return allowed;
    }

    private InteractionSide GetAllowedEnterSide(HorizontalDirection snitchDirection)
    {
        switch (snitchDirection)
        {
            case HorizontalDirection.Left:
                // Snitch läuft nach links, schaut also nicht nach rechts.
                return InteractionSide.Right;

            case HorizontalDirection.Right:
                // Snitch läuft nach rechts, schaut also nicht nach links.
                return InteractionSide.Left;

            default:
                return InteractionSide.None;
        }
    }

    private HorizontalDirection GetStableSnitchDirection()
    {
        if (lastStableDirection == HorizontalDirection.None)
            return HorizontalDirection.None;

        if (Time.time <= lastStableDirectionTime + directionMemoryTime)
            return lastStableDirection;

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
        {
            hasSnitchSample = false;
            lastStableDirection = HorizontalDirection.None;
            return;
        }

        previousSnitchX = snitchTransform.position.x;
        hasSnitchSample = true;
        lastStableDirection = HorizontalDirection.None;
        lastStableDirectionTime = float.NegativeInfinity;
    }

    private void UpdateSnitchTracking()
    {
        Transform oldSnitch = snitchTransform;
        CacheSnitch();

        if (snitchTransform == null)
        {
            hasSnitchSample = false;
            lastStableDirection = HorizontalDirection.None;
            return;
        }

        if (oldSnitch != snitchTransform || !hasSnitchSample)
        {
            InitializeSnitchTracking();
            return;
        }

        float currentX = snitchTransform.position.x;
        float deltaX = currentX - previousSnitchX;

        if (Mathf.Abs(deltaX) >= teleportThreshold)
        {
            LogDebug("Snitch-Teleport/Loop erkannt, Delta ignoriert: " + deltaX);
            previousSnitchX = currentX;
            return;
        }

        if (deltaX > directionThreshold)
        {
            lastStableDirection = HorizontalDirection.Right;
            lastStableDirectionTime = Time.time;
        }
        else if (deltaX < -directionThreshold)
        {
            lastStableDirection = HorizontalDirection.Left;
            lastStableDirectionTime = Time.time;
        }

        previousSnitchX = currentX;
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
        {
            LogDebug("Player zu hoch oder zu tief.");
            return false;
        }

        float maxHorizontalDistance = sideRange + horizontalBuffer;

        float leftDistance = Mathf.Abs(playerPosition.x - bounds.min.x);
        float rightDistance = Mathf.Abs(playerPosition.x - bounds.max.x);
        float centerDelta = playerPosition.x - center.x;

        bool nearLeft = leftDistance <= maxHorizontalDistance;
        bool nearRight = rightDistance <= maxHorizontalDistance;

        if (!nearLeft && !nearRight)
        {
            LogDebug("Player nicht nah genug an einer Seite.");
            return false;
        }

        if (centerDelta < -centerDeadzone && nearLeft)
        {
            side = InteractionSide.Left;
            return true;
        }

        if (centerDelta > centerDeadzone && nearRight)
        {
            side = InteractionSide.Right;
            return true;
        }

        if (nearLeft && nearRight)
        {
            side = leftDistance <= rightDistance ? InteractionSide.Left : InteractionSide.Right;
            LogDebug("Player nahe Mitte -> nähere Seite genommen: " + side);
            return true;
        }

        side = nearLeft ? InteractionSide.Left : InteractionSide.Right;
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