using System.Collections;
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

    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    private static readonly int IsSprintingHash = Animator.StringToHash("IsSprinting");

    [Header("Interaction Window")]
    [FormerlySerializedAs("sideRange")]
    [SerializeField, Min(0f)] private float sideRange = 0.5f;

    [FormerlySerializedAs("sideHeight")]
    [SerializeField, Min(0f)] private float sideHeight = 2f;

    [SerializeField, Min(0f)] private float horizontalBuffer = 0.35f;
    [SerializeField, Min(0f)] private float verticalBuffer = 0.2f;
    [SerializeField, Min(0f)] private float centerDeadzone = 0.05f;

    [Header("Interaction Safety")]
    [SerializeField, Min(0f)] private float interactionCooldown = 0.15f;

    [Header("Snitch Logic")]
    [SerializeField] private bool requireSnitchRule = true;
    [SerializeField] private string snitchTag = "Snitch";
    [SerializeField] private bool allowHideWhenSnitchMissing = true;
    [SerializeField, Min(0f)] private float snitchPlayerDeadzone = 0.05f;

    [Header("Hide Movement")]
    [SerializeField] private Transform leftHidePoint;
    [SerializeField] private Transform rightHidePoint;
    [SerializeField, Min(0.01f)] private float moveToHideSpeed = 4f;
    [SerializeField, Min(0.001f)] private float arriveDistance = 0.03f;

    [Header("Sorting")]
    [SerializeField] private int hiddenSortingOrder = 1;
    [SerializeField] private int normalSortingOrder = 10;

    [Header("Visuals")]
    [SerializeField] private Sprite crouchedSprite;

#if UNITY_EDITOR
    [SerializeField] private bool showInteractionGizmos = true;
#endif

    private BoxCollider2D box;
    private Transform snitchTransform;

    private float lastInteractionTime = float.NegativeInfinity;
    private bool isBusy;
    private Vector3 lastExitWorldPosition;

    private void Awake()
    {
        CacheCollider();
        CacheSnitch();
    }

    private void OnValidate()
    {
        CacheCollider();
    }

    public override bool CanInteract(PlayerMain player)
    {
        if (!IsReady(player))
            return false;

        if (isBusy)
            return false;

        if (IsOnCooldown())
            return false;

        if (player.IsHiddenBy(this))
            return true;

        if (!TryGetValidSide(player.transform.position, out InteractionSide playerSide))
            return false;

        if (!CanEnterFromSide(player, playerSide))
            return false;

        return GetHidePoint(playerSide) != null;
    }

    public override void Interact(PlayerMain player)
    {
        if (!IsReady(player))
            return;

        if (isBusy)
            return;

        if (IsOnCooldown())
            return;

        if (player.IsHiddenBy(this))
        {
            StartCoroutine(ExitRoutine(player));
            return;
        }

        if (!TryGetValidSide(player.transform.position, out InteractionSide playerSide))
            return;

        if (!CanEnterFromSide(player, playerSide))
            return;

        Transform targetHidePoint = GetHidePoint(playerSide);
        if (targetHidePoint == null)
            return;

        StartCoroutine(EnterRoutine(player, targetHidePoint));
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

    private bool CanEnterFromSide(PlayerMain player, InteractionSide playerSide)
    {
        if (!requireSnitchRule)
            return true;

        if (snitchTransform == null)
            return allowHideWhenSnitchMissing;

        if (!TryGetSnitchSideRelativeToPlayer(player, out InteractionSide snitchSide))
            return false;

        InteractionSide allowedSide = snitchSide == InteractionSide.Left
            ? InteractionSide.Right
            : InteractionSide.Left;

        return playerSide == allowedSide;
    }

    private bool TryGetSnitchSideRelativeToPlayer(PlayerMain player, out InteractionSide snitchSide)
    {
        snitchSide = InteractionSide.None;

        if (player == null || snitchTransform == null)
            return false;

        float deltaX = snitchTransform.position.x - player.transform.position.x;

        if (deltaX > snitchPlayerDeadzone)
        {
            snitchSide = InteractionSide.Right;
            return true;
        }

        if (deltaX < -snitchPlayerDeadzone)
        {
            snitchSide = InteractionSide.Left;
            return true;
        }

        return false;
    }

    private Transform GetHidePoint(InteractionSide side)
    {
        switch (side)
        {
            case InteractionSide.Left:
                return leftHidePoint;
            case InteractionSide.Right:
                return rightHidePoint;
            default:
                return null;
        }
    }

    private IEnumerator EnterRoutine(PlayerMain player, Transform targetHidePoint)
    {
        isBusy = true;
        lastExitWorldPosition = player.transform.position;

        player.SetExternalVisualControl(true);
        SetPlayerSorting(player, hiddenSortingOrder);

        Animator animator = player.GetAnimator();
        SetMoveAnimation(animator, true);

        yield return MovePlayerToPoint(player.transform, targetHidePoint.position, moveToHideSpeed);

        SetMoveAnimation(animator, false);

        player.EnterHidezone(this, crouchedSprite);
        player.SetExternalVisualControl(false);

        lastInteractionTime = Time.time;
        isBusy = false;
    }

    private IEnumerator ExitRoutine(PlayerMain player)
    {
        isBusy = true;

        player.ExitHidezone(this);
        SetPlayerSorting(player, normalSortingOrder);
        player.SetExternalVisualControl(true);

        Animator animator = player.GetAnimator();
        SetMoveAnimation(animator, true);

        yield return MovePlayerToPoint(player.transform, lastExitWorldPosition, moveToHideSpeed);

        SetMoveAnimation(animator, false);
        player.SetExternalVisualControl(false);

        lastInteractionTime = Time.time;
        isBusy = false;
    }

    private IEnumerator MovePlayerToPoint(Transform playerTransform, Vector3 targetPosition, float speed)
    {
        targetPosition.z = playerTransform.position.z;

        while (Vector2.Distance(playerTransform.position, targetPosition) > arriveDistance)
        {
            playerTransform.position = Vector3.MoveTowards(
                playerTransform.position,
                targetPosition,
                speed * Time.deltaTime
            );

            yield return null;
        }

        playerTransform.position = targetPosition;
    }

    private void SetMoveAnimation(Animator animator, bool isMoving)
    {
        if (animator == null)
            return;

        animator.SetBool(IsMovingHash, isMoving);
        animator.SetBool(IsSprintingHash, isMoving);
    }

    private void SetPlayerSorting(PlayerMain player, int order)
    {
        if (player == null)
            return;

        SpriteRenderer sr = player.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
            sr.sortingOrder = order;
    }

    private void CacheSnitch()
    {
        if (snitchTransform != null)
            return;

        GameObject snitchObject = GameObject.FindGameObjectWithTag(snitchTag);
        if (snitchObject != null)
            snitchTransform = snitchObject.transform;
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
        float centerDelta = playerPosition.x - center.x;

        bool nearLeft = leftDistance <= maxHorizontalDistance;
        bool nearRight = rightDistance <= maxHorizontalDistance;

        if (!nearLeft && !nearRight)
            return false;

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
            side = leftDistance <= rightDistance
                ? InteractionSide.Left
                : InteractionSide.Right;
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

        if (leftHidePoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(leftHidePoint.position, 0.08f);
        }

        if (rightHidePoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(rightHidePoint.position, 0.08f);
        }

        if (snitchTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, snitchTransform.position);
            Gizmos.DrawWireSphere(snitchTransform.position, 0.1f);
        }
    }
#endif
}