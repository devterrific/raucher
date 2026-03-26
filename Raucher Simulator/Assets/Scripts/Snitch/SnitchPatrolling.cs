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
    [SerializeField, Min(0f)] private float patrolSpeedMultiplierMin = 0.9f;
    [SerializeField, Min(0f)] private float patrolSpeedMultiplierMax = 1.35f;
    [SerializeField, Range(0f, 1f)] private float turnAroundChanceAtPoint = 0.7f;

    [Header("Runtime")]
    [SerializeField] private int currentPointIndex;

    [HideInInspector] public bool canMove = true;
    public Vector2 FacingDirection => isFacingRight ? Vector2.right : Vector2.left;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private bool logStateChangesOnly = true;
    [SerializeField] private bool showGizmos = true;
    [SerializeField, Min(0f)] private float suspiciousTeleportDistance = 2f;

    private Animator animator;
    private float waitTimer;
    private bool isWaiting;
    private bool isFacingRight = true;

    private float forcedFacingTimer;
    private float closeTurnCooldownTimer;

    private float currentMoveSpeed;
    private float currentWaitDuration;
    private bool hasTurnedAroundDuringWait;

    private Vector3 previousPosition;
    private bool hadPreviousPosition;

    private bool lastLoggedCanMove;
    private bool lastLoggedIsWaiting;
    private bool lastLoggedFacingRight;
    private int lastLoggedPointIndex = -999;
    private bool lastLoggedHasValidPath;
    private bool lastLoggedWalking;
    private float lastLoggedForcedFacingTimer = -999f;
    private float lastLoggedCloseTurnCooldownTimer = -999f;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        currentPointIndex = Mathf.Clamp(currentPointIndex, 0, Mathf.Max(0, patrolPoints.Length - 1));
        isFacingRight = transform.rotation.eulerAngles.y < 90f || transform.rotation.eulerAngles.y > 270f;

        ApplyFacingRotation();
        SetWalking(false);
        SetRandomPatrolSpeed();

        previousPosition = transform.position;
        hadPreviousPosition = true;

        LogAlways("Awake");
        LogAlways("Start Facing: " + (isFacingRight ? "Right" : "Left"));
        LogAlways("Start Point Index: " + currentPointIndex);
        LogAlways("Patrol Point Count: " + (patrolPoints == null ? 0 : patrolPoints.Length));
        LogPatrolPoints();
    }

    private void Update()
    {
        UpdateFacingOverrideTimers();

        bool hasValidPath = HasValidPatrolPath();

        LogStateIfChanged(hasValidPath);

        if (!hasValidPath || !canMove)
        {
            SetWalking(false);

            if (!hasValidPath)
                LogState("No valid patrol path.");
            if (!canMove)
                LogState("canMove is FALSE.");

            TrackMovementDebug("Blocked");
            return;
        }

        if (isWaiting)
        {
            SetWalking(false);

            TryTurnAroundWhileWaiting();

            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                LogAlways("Wait finished at point " + currentPointIndex);

                isWaiting = false;
                AdvanceToNextValidPoint();
                SetRandomPatrolSpeed();

                LogAlways("Next point index: " + currentPointIndex);
                LogAlways("New patrol speed: " + currentMoveSpeed);
            }

            TrackMovementDebug("Waiting");
            return;
        }

        MoveTowardsCurrentPoint();
        TrackMovementDebug("Moving");
    }

    public bool TryStartCloseTurnTowards(Vector3 targetPosition, float duration, float cooldown)
    {
        LogAlways("TryStartCloseTurnTowards called. TargetX: " + targetPosition.x + " | MyX: " + transform.position.x);

        if (forcedFacingTimer > 0f || closeTurnCooldownTimer > 0f)
        {
            LogAlways("Close turn denied. forcedFacingTimer=" + forcedFacingTimer + " | closeTurnCooldownTimer=" + closeTurnCooldownTimer);
            return false;
        }

        float deltaX = targetPosition.x - transform.position.x;
        if (Mathf.Abs(deltaX) <= 0.001f)
        {
            LogAlways("Close turn denied. Target too centered.");
            return false;
        }

        isFacingRight = deltaX > 0f;
        ApplyFacingRotation();

        forcedFacingTimer = Mathf.Max(0f, duration);
        closeTurnCooldownTimer = forcedFacingTimer + Mathf.Max(0f, cooldown);

        LogAlways("Close turn accepted. Facing: " + (isFacingRight ? "Right" : "Left") +
                  " | forcedFacingTimer=" + forcedFacingTimer +
                  " | closeTurnCooldownTimer=" + closeTurnCooldownTimer);

        return true;
    }

    private bool HasValidPatrolPath()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            LogState("PatrolPoints missing or empty.");
            return false;
        }

        if (currentPointIndex < 0 || currentPointIndex >= patrolPoints.Length)
        {
            LogAlways("CurrentPointIndex out of range before clamp: " + currentPointIndex);
            currentPointIndex = Mathf.Clamp(currentPointIndex, 0, patrolPoints.Length - 1);
            LogAlways("CurrentPointIndex clamped to: " + currentPointIndex);
        }

        if (patrolPoints[currentPointIndex] == null)
        {
            LogAlways("Current patrol point is NULL at index " + currentPointIndex + ". Searching next valid point.");
            AdvanceToNextValidPoint();
        }

        bool valid = patrolPoints[currentPointIndex] != null;
        if (!valid)
            LogState("No valid patrol point found after search.");

        return valid;
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
        else
        {
            LogState("Facing locked by forcedFacingTimer: " + forcedFacingTimer);
        }

        Vector3 newPosition = Vector2.MoveTowards(currentPosition, targetPosition, currentMoveSpeed * Time.deltaTime);
        transform.position = new Vector3(newPosition.x, newPosition.y, -9f);

        bool moved = (newPosition - currentPosition).sqrMagnitude > 0.000001f;
        SetWalking(moved);

        float remainingDistanceSqr = (targetPosition - newPosition).sqrMagnitude;

        if (!moved)
        {
            LogState("MoveTowardsCurrentPoint: Did not move. Current=" + currentPosition + " | Target=" + targetPosition);
        }

        if (remainingDistanceSqr <= arrivalThreshold * arrivalThreshold)
        {
            isWaiting = true;
            waitTimer = waitTimeAtPoint;
            currentWaitDuration = waitTimeAtPoint;
            hasTurnedAroundDuringWait = false;
            SetWalking(false);

            LogAlways("Arrived at patrol point index " + currentPointIndex +
                      " | Target=" + target.name +
                      " | Wait=" + waitTimeAtPoint);
        }
    }

    private void AdvanceToNextValidPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            LogAlways("AdvanceToNextValidPoint failed: no patrol points.");
            return;
        }

        int startIndex = currentPointIndex;

        do
        {
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;

            LogAlways("Checking patrol point index " + currentPointIndex +
                      " -> " + (patrolPoints[currentPointIndex] != null ? patrolPoints[currentPointIndex].name : "NULL"));

            if (patrolPoints[currentPointIndex] != null)
            {
                LogAlways("Advanced to valid patrol point: " + currentPointIndex + " (" + patrolPoints[currentPointIndex].name + ")");
                return;
            }
        }
        while (currentPointIndex != startIndex);

        LogAlways("AdvanceToNextValidPoint: all patrol points are NULL.");
    }

    private void UpdateFacing(float horizontalDelta)
    {
        if (Mathf.Abs(horizontalDelta) <= 0.001f)
        {
            LogState("UpdateFacing skipped: horizontalDelta too small.");
            return;
        }

        bool shouldFaceRight = horizontalDelta > 0f;
        if (shouldFaceRight == isFacingRight)
            return;

        isFacingRight = shouldFaceRight;
        ApplyFacingRotation();

        LogAlways("Facing changed to: " + (isFacingRight ? "Right" : "Left") +
                  " | horizontalDelta=" + horizontalDelta);
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

        LogAlways("Random patrol speed set: " + currentMoveSpeed +
                  " | base=" + moveSpeed +
                  " | minMul=" + minMultiplier +
                  " | maxMul=" + maxMultiplier);
    }

    private void TryTurnAroundWhileWaiting()
    {
        if (hasTurnedAroundDuringWait)
        {
            LogState("Wait turn skipped: already turned this wait.");
            return;
        }

        if (forcedFacingTimer > 0f)
        {
            LogState("Wait turn skipped: forcedFacingTimer active.");
            return;
        }

        if (currentWaitDuration <= 0f)
        {
            LogState("Wait turn skipped: currentWaitDuration <= 0.");
            return;
        }

        if (waitTimer > currentWaitDuration * 0.5f)
            return;

        float randomValue = Random.value;
        if (randomValue > turnAroundChanceAtPoint)
        {
            LogAlways("Wait turn NOT triggered. Random=" + randomValue + " > Chance=" + turnAroundChanceAtPoint);
            hasTurnedAroundDuringWait = true;
            return;
        }

        isFacingRight = !isFacingRight;
        ApplyFacingRotation();
        hasTurnedAroundDuringWait = true;

        LogAlways("Wait turn triggered. New Facing: " + (isFacingRight ? "Right" : "Left") +
                  " | Random=" + randomValue +
                  " | Chance=" + turnAroundChanceAtPoint);
    }

    private void TrackMovementDebug(string stateLabel)
    {
        if (!hadPreviousPosition)
        {
            previousPosition = transform.position;
            hadPreviousPosition = true;
            return;
        }

        Vector3 currentPosition = transform.position;
        Vector3 delta = currentPosition - previousPosition;

        if (Mathf.Abs(delta.x) >= suspiciousTeleportDistance)
        {
            LogAlways("Suspicious X jump detected in state [" + stateLabel + "] | DeltaX=" + delta.x +
                      " | From=" + previousPosition +
                      " | To=" + currentPosition);
        }

        previousPosition = currentPosition;
    }

    private void LogPatrolPoints()
    {
        if (!enableDebugLogs)
            return;

        if (patrolPoints == null)
        {
            Debug.Log("[SnitchPatrolling] PatrolPoints array is NULL.", this);
            return;
        }

        for (int i = 0; i < patrolPoints.Length; i++)
        {
            string pointName = patrolPoints[i] != null ? patrolPoints[i].name : "NULL";
            string pointPos = patrolPoints[i] != null ? patrolPoints[i].position.ToString() : "no position";
            Debug.Log("[SnitchPatrolling] PatrolPoint[" + i + "] = " + pointName + " | Pos=" + pointPos, this);
        }
    }

    private void LogStateIfChanged(bool hasValidPath)
    {
        if (!enableDebugLogs || !logStateChangesOnly)
            return;

        bool walking = animator != null && animator.GetBool(IsWalkingHash);

        if (lastLoggedCanMove != canMove)
        {
            lastLoggedCanMove = canMove;
            LogAlways("canMove changed -> " + canMove);
        }

        if (lastLoggedIsWaiting != isWaiting)
        {
            lastLoggedIsWaiting = isWaiting;
            LogAlways("isWaiting changed -> " + isWaiting + " | waitTimer=" + waitTimer);
        }

        if (lastLoggedFacingRight != isFacingRight)
        {
            lastLoggedFacingRight = isFacingRight;
            LogAlways("isFacingRight changed -> " + isFacingRight);
        }

        if (lastLoggedPointIndex != currentPointIndex)
        {
            lastLoggedPointIndex = currentPointIndex;
            LogAlways("currentPointIndex changed -> " + currentPointIndex);
        }

        if (lastLoggedHasValidPath != hasValidPath)
        {
            lastLoggedHasValidPath = hasValidPath;
            LogAlways("hasValidPath changed -> " + hasValidPath);
        }

        if (lastLoggedWalking != walking)
        {
            lastLoggedWalking = walking;
            LogAlways("Animator IsWalking changed -> " + walking);
        }

        if (Mathf.Abs(lastLoggedForcedFacingTimer - forcedFacingTimer) > 0.25f)
        {
            lastLoggedForcedFacingTimer = forcedFacingTimer;
            if (forcedFacingTimer > 0f)
                LogAlways("forcedFacingTimer -> " + forcedFacingTimer);
        }

        if (Mathf.Abs(lastLoggedCloseTurnCooldownTimer - closeTurnCooldownTimer) > 0.25f)
        {
            lastLoggedCloseTurnCooldownTimer = closeTurnCooldownTimer;
            if (closeTurnCooldownTimer > 0f)
                LogAlways("closeTurnCooldownTimer -> " + closeTurnCooldownTimer);
        }
    }

    private void LogAlways(string message)
    {
        if (!enableDebugLogs)
            return;

        Debug.Log("[SnitchPatrolling] " + message, this);
    }

    private void LogState(string message)
    {
        if (!enableDebugLogs)
            return;

        if (logStateChangesOnly)
            return;

        Debug.Log("[SnitchPatrolling] " + message, this);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!showGizmos || patrolPoints == null || patrolPoints.Length == 0)
            return;

        Gizmos.color = Color.cyan;

        for (int i = 0; i < patrolPoints.Length; i++)
        {
            Transform point = patrolPoints[i];
            if (point == null)
                continue;

            Gizmos.DrawWireSphere(point.position, 0.12f);

            Transform nextPoint = patrolPoints[(i + 1) % patrolPoints.Length];
            if (nextPoint != null)
                Gizmos.DrawLine(point.position, nextPoint.position);
        }

        Gizmos.color = isFacingRight ? Color.green : Color.red;
        Gizmos.DrawLine(transform.position, transform.position + (isFacingRight ? Vector3.right : Vector3.left) * 0.75f);

        if (Application.isPlaying && patrolPoints != null && patrolPoints.Length > 0 &&
            currentPointIndex >= 0 && currentPointIndex < patrolPoints.Length &&
            patrolPoints[currentPointIndex] != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, patrolPoints[currentPointIndex].position);
        }
    }
#endif
}