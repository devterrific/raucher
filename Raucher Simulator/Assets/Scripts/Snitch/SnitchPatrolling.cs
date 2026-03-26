using UnityEngine;

[RequireComponent(typeof(Animator))]
public class SnitchPatrolling : MonoBehaviour
{
    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");

    [Header("Patrol")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField, Min(0f)] private float moveSpeed = 2f;
    [SerializeField, Min(0f)] private float waitTimeAtPoint = 2.2f;
    [SerializeField, Min(0.001f)] private float arrivalThreshold = 0.05f;

    [Header("Patrol Variation")]
    [SerializeField, Min(0f)] private float patrolSpeedMultiplierMin = 0.8f;
    [SerializeField, Min(0f)] private float patrolSpeedMultiplierMax = 1.6f;

    [Header("Runtime")]
    [SerializeField] private int currentPointIndex;

    [HideInInspector] public bool canMove = true;
    public Vector2 FacingDirection => isFacingRight ? Vector2.right : Vector2.left;

    private Animator animator;
    private float waitTimer;
    private bool isWaiting;
    private bool isFacingRight = true;

    private float forcedFacingTimer;
    private float closeTurnCooldownTimer;

    private float currentMoveSpeed;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        currentPointIndex = Mathf.Clamp(currentPointIndex, 0, Mathf.Max(0, patrolPoints.Length - 1));
        isFacingRight = transform.rotation.eulerAngles.y < 90f || transform.rotation.eulerAngles.y > 270f;

        ApplyFacingRotation();
        SetWalking(false);
        SetRandomPatrolSpeed();
    }

    private void Update()
    {
        UpdateFacingOverrideTimers();

        if (!HasValidPatrolPath() || !canMove)
        {
            SetWalking(false);
            return;
        }

        if (isWaiting)
        {
            SetWalking(false);

            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                isWaiting = false;
                AdvanceToNextValidPoint();
                SetRandomPatrolSpeed();
            }

            return;
        }

        MoveTowardsCurrentPoint();
    }

    public bool TryStartCloseTurnTowards(Vector3 targetPosition, float duration, float cooldown)
    {
        if (forcedFacingTimer > 0f || closeTurnCooldownTimer > 0f)
            return false;

        float deltaX = targetPosition.x - transform.position.x;
        if (Mathf.Abs(deltaX) <= 0.001f)
            return false;

        isFacingRight = deltaX > 0f;
        ApplyFacingRotation();

        forcedFacingTimer = Mathf.Max(0f, duration);
        closeTurnCooldownTimer = forcedFacingTimer + Mathf.Max(0f, cooldown);
        return true;
    }

    private bool HasValidPatrolPath()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return false;

        if (patrolPoints[currentPointIndex] == null)
            AdvanceToNextValidPoint();

        return patrolPoints[currentPointIndex] != null;
    }

    private void MoveTowardsCurrentPoint()
    {
        Transform target = patrolPoints[currentPointIndex];
        Vector3 currentPosition = transform.position;
        Vector3 targetPosition = target.position;

        if (forcedFacingTimer <= 0f)
        {
            float horizontalDelta = targetPosition.x - currentPosition.x;
            UpdateFacing(horizontalDelta);
        }

        Vector3 newPosition = Vector2.MoveTowards(currentPosition, targetPosition, currentMoveSpeed * Time.deltaTime);
        transform.position = new Vector3(newPosition.x, newPosition.y, -9f);

        bool moved = (newPosition - currentPosition).sqrMagnitude > 0.000001f;
        SetWalking(moved);

        float remainingDistanceSqr = (targetPosition - newPosition).sqrMagnitude;
        if (remainingDistanceSqr <= arrivalThreshold * arrivalThreshold)
        {
            isWaiting = true;
            waitTimer = waitTimeAtPoint;
            SetWalking(false);
        }
    }

    private void AdvanceToNextValidPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        int startIndex = currentPointIndex;
        do
        {
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
            if (patrolPoints[currentPointIndex] != null)
                return;
        }
        while (currentPointIndex != startIndex);
    }

    private void UpdateFacing(float horizontalDelta)
    {
        if (Mathf.Abs(horizontalDelta) <= 0.001f)
            return;

        bool shouldFaceRight = horizontalDelta > 0f;
        if (shouldFaceRight == isFacingRight)
            return;

        isFacingRight = shouldFaceRight;
        ApplyFacingRotation();
    }

    private void ApplyFacingRotation()
    {
        transform.rotation = isFacingRight
            ? Quaternion.identity
            : Quaternion.Euler(0f, 180f, 0f);
    }

    private void UpdateFacingOverrideTimers()
    {
        if (forcedFacingTimer > 0f)
            forcedFacingTimer -= Time.deltaTime;

        if (closeTurnCooldownTimer > 0f)
            closeTurnCooldownTimer -= Time.deltaTime;
    }

    private void SetWalking(bool value)
    {
        if (animator != null)
            animator.SetBool(IsWalkingHash, value);
    }

    private void SetRandomPatrolSpeed()
    {
        float minMultiplier = Mathf.Max(0f, patrolSpeedMultiplierMin);
        float maxMultiplier = Mathf.Max(minMultiplier, patrolSpeedMultiplierMax);

        currentMoveSpeed = moveSpeed * Random.Range(minMultiplier, maxMultiplier);
    }
}