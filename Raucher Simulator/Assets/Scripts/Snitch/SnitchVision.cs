using UnityEngine;

public class SnitchVision : MonoBehaviour
{
    [Header("Raycast Points")]
    [SerializeField] private Transform eyePoint;
    [SerializeField] private Transform suspicionPoint;

    [Header("Raycast Settings")]
    [SerializeField] private float viewDistance = 5f;
    [SerializeField] private LayerMask playerLayer; // NUR Player (nicht PlayerHidden)

    [Header("Suspicion Settings")]
    [SerializeField] private float timeToCatch = 3f;

    private float suspicionTimer = 0f;
    private SnitchPatrolling snitchPatrolling;

    void Awake()
    {
        snitchPatrolling = GetComponent<SnitchPatrolling>();
    }

    void Update()
    {
        bool instantHit = InstantRaycast();
        bool suspicionHit = TimedRaycast();

        if (snitchPatrolling != null)
            snitchPatrolling.canMove = !(instantHit || suspicionHit);
    }

    bool InstantRaycast()
    {
        if (eyePoint == null) return false;

        RaycastHit2D hit = Physics2D.Raycast(
            eyePoint.position,
            transform.right,
            viewDistance,
            playerLayer
        );

        if (hit.collider == null) return false;

        // extra safety: falls Layer mal falsch ist
        PlayerMain player = hit.collider.GetComponent<PlayerMain>();
        if (player != null && !player.Detectable)
            return false;

        Debug.Log("Player got caught!");
        return true;
    }

    bool TimedRaycast()
    {
        if (suspicionPoint == null) return false;

        RaycastHit2D hit = Physics2D.Raycast(
            suspicionPoint.position,
            transform.right,
            viewDistance,
            playerLayer
        );

        if (hit.collider == null)
        {
            suspicionTimer = 0f;
            return false;
        }

        // extra safety: falls Layer mal falsch ist
        PlayerMain player = hit.collider.GetComponent<PlayerMain>();
        if (player != null && !player.Detectable)
        {
            suspicionTimer = 0f;
            return false;
        }

        suspicionTimer += Time.deltaTime;

        if (suspicionTimer >= timeToCatch)
        {
            Debug.Log("Player got caught after timed suspicion!");
            return true;
        }

        return true; // Snitch bleibt stehen während Verdacht läuft
    }

    void OnDrawGizmosSelected()
    {
        if (eyePoint == null || suspicionPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(eyePoint.position, eyePoint.position + transform.right * viewDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(suspicionPoint.position, suspicionPoint.position + transform.right * viewDistance);
    }
}
