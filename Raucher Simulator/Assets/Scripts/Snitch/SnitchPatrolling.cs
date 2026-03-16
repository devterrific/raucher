using UnityEngine;

public class SnitchPatrolling : MonoBehaviour
{
    [Header("Patrol")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField, Min(0f)] private float moveSpeed = 2f;
    [SerializeField, Min(0f)] private float waitTimeAtPoint = 2.2f;
    [SerializeField, Min(0.001f)] private float arrivalThreshold = 0.05f;

    [Header("Runtime")]
    [SerializeField] private int currentPointIndex;

    [HideInInspector] public bool canMove = true;
    public Vector2 FacingDirection => isFacingRight ? Vector2.right : Vector2.left;

    private float waitTimer;
    private bool isWaiting;
    private bool isFacingRight = true;

    private void Awake()
    {
        currentPointIndex = Mathf.Clamp(currentPointIndex, 0, Mathf.Max(0, patrolPoints.Length - 1));
        isFacingRight = transform.rotation.eulerAngles.y < 90f || transform.rotation.eulerAngles.y > 270f;
    }

    private void Update()
    {
        if (!HasValidPatrolPath() || !canMove)
        {
            return;
        }

        if (isWaiting)
        {
            UpdateWaitTimer();
            return;
        }

        MoveTowardsCurrentPoint();
    }

    private bool HasValidPatrolPath()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            return false;
        }

        if (patrolPoints[currentPointIndex] == null)
        {
            AdvanceToNextValidPoint();
        }

        return patrolPoints[currentPointIndex] != null;
    }

    private void UpdateWaitTimer()
    {
        waitTimer -= Time.deltaTime;
        if (waitTimer > 0f)
        {
            return;
        }

        isWaiting = false;
        AdvanceToNextValidPoint();
    }

    private void MoveTowardsCurrentPoint()
    {
        Transform target = patrolPoints[currentPointIndex];
        Vector3 currentPosition = transform.position;
        Vector3 targetPosition = target.position;

        float horizontalDelta = targetPosition.x - currentPosition.x;
        UpdateFacing(horizontalDelta);

        Vector3 newPosition = Vector2.MoveTowards(currentPosition, targetPosition, moveSpeed * Time.deltaTime);
        transform.position = newPosition;

        float remainingDistanceSqr = (targetPosition - newPosition).sqrMagnitude;
        if (remainingDistanceSqr <= arrivalThreshold * arrivalThreshold)
        {
            StartWaiting();
        }
    }

    private void StartWaiting()
    {
        if (isWaiting)
        {
            return;
        }

        isWaiting = true;
        waitTimer = waitTimeAtPoint;
    }

    private void AdvanceToNextValidPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            return;
        }

        int startIndex = currentPointIndex;

        do
        {
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;

            if (patrolPoints[currentPointIndex] != null)
            {
                return;
            }
        }
        while (currentPointIndex != startIndex);
    }

    private void UpdateFacing(float horizontalDelta)
    {
        const float deadZone = 0.001f;
        if (Mathf.Abs(horizontalDelta) <= deadZone)
        {
            return;
        }

        bool shouldFaceRight = horizontalDelta > 0f;
        if (shouldFaceRight == isFacingRight)
        {
            return;
        }

        isFacingRight = shouldFaceRight;
        transform.rotation = isFacingRight ? Quaternion.identity : Quaternion.Euler(0f, 180f, 0f);
    }
}