using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(SnitchPatrolling))]
public class SnitchVision : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform eyePoint;
    [SerializeField] private Transform suspicionPoint;

    [Header("Raycast Settings")]
    [SerializeField, Min(0f)] private float instantDistance = 1.5f;
    [SerializeField, Min(0f)] private float suspicionDistance = 5f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Suspicion Settings")]
    [SerializeField, Min(0f)] private float timeToCatch = 3f;

    [Header("Close Turn Reaction")]
    [SerializeField, Min(0f)] private float closeTurnDistance = 1.25f;
    [SerializeField, Min(0f)] private float closeTurnDuration = 1f;
    [SerializeField, Min(0f)] private float closeTurnCooldown = 1f;

    [Header("Respawn (Scene)")]
    [SerializeField] private string respawnSceneName = "Buro";
    [SerializeField] private string respawnPointName = "startBuro";
    [SerializeField] private bool resetVelocityOnRespawn = true;

    [Header("Debug")]
    [SerializeField] private bool drawDebug = true;

    private SnitchPatrolling snitchPatrolling;
    private float suspicionTimer;

    private bool isRespawning;
    private PlayerMain playerToRespawn;

    private void Awake()
    {
        snitchPatrolling = GetComponent<SnitchPatrolling>();
        if (eyePoint == null) Debug.LogWarning("[SnitchVision] eyePoint is missing. Instant catch is disabled.", this);
        if (suspicionPoint == null) Debug.LogWarning("[SnitchVision] suspicionPoint is missing. Suspicion is disabled.", this);
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void Update()
    {
        if (isRespawning)
        {
            ApplyMovementStop(true);
            return;
        }

        TryTriggerCloseTurnReaction();

        bool isSuspicious = CheckSuspicion();
        if (isRespawning)
        {
            ApplyMovementStop(true);
            return;
        }

        bool instantCaught = !isSuspicious && CheckInstantCatch();
        ApplyMovementStop(isSuspicious || instantCaught);
    }

    private Vector2 FacingDirection => snitchPatrolling != null ? snitchPatrolling.FacingDirection : Vector2.right;

    private void TryTriggerCloseTurnReaction()
    {
        if (snitchPatrolling == null || closeTurnDistance <= 0f)
        {
            return;
        }

        Collider2D hit = Physics2D.OverlapCircle(transform.position, closeTurnDistance, playerLayer);
        if (hit == null || !hit.TryGetComponent(out PlayerMain player) || !player.Detectable)
        {
            return;
        }

        snitchPatrolling.TryStartCloseTurnTowards(player.transform.position, closeTurnDuration, closeTurnCooldown);
    }

    private bool CheckInstantCatch()
    {
        if (eyePoint == null)
        {
            return false;
        }

        Vector2 facing = FacingDirection;
        if (drawDebug) Debug.DrawRay(eyePoint.position, facing * instantDistance, Color.red);

        if (!TryGetDetectablePlayer(eyePoint.position, facing, instantDistance, out PlayerMain player))
        {
            return false;
        }

        StartRespawn(player);
        return true;
    }

    private bool CheckSuspicion()
    {
        if (suspicionPoint == null)
        {
            suspicionTimer = 0f;
            return false;
        }

        Vector2 facing = FacingDirection;
        if (drawDebug) Debug.DrawRay(suspicionPoint.position, facing * suspicionDistance, Color.yellow);

        bool seesPlayer = TryGetDetectablePlayer(suspicionPoint.position, facing, suspicionDistance, out PlayerMain player);
        if (!seesPlayer)
        {
            suspicionTimer = 0f;
            return false;
        }

        suspicionTimer += Time.deltaTime;
        if (suspicionTimer < timeToCatch)
        {
            return true;
        }

        suspicionTimer = 0f;
        StartRespawn(player);
        return true;
    }

    private bool TryGetDetectablePlayer(Vector2 origin, Vector2 direction, float distance, out PlayerMain player)
    {
        player = null;

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, distance, playerLayer);
        if (hit.collider == null)
        {
            return false;
        }

        if (!hit.collider.TryGetComponent(out PlayerMain foundPlayer) || !foundPlayer.Detectable)
        {
            return false;
        }

        player = foundPlayer;
        return true;
    }

    private void ApplyMovementStop(bool shouldStop)
    {
        if (snitchPatrolling != null)
        {
            snitchPatrolling.canMove = !shouldStop;
        }
    }

    private void StartRespawn(PlayerMain player)
    {
        if (isRespawning || player == null)
        {
            return;
        }

        isRespawning = true;
        playerToRespawn = player;

        DontDestroyOnLoad(playerToRespawn.gameObject);
        SceneManager.LoadScene(respawnSceneName, LoadSceneMode.Single);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!isRespawning || scene.name != respawnSceneName)
        {
            return;
        }

        GameObject spawnObject = GameObject.Find(respawnPointName);
        if (spawnObject == null)
        {
            Debug.LogError($"[SnitchVision] Respawn point '{respawnPointName}' not found in scene '{respawnSceneName}'.", this);
            CleanupRespawnState();
            return;
        }

        if (playerToRespawn != null)
        {
            if (resetVelocityOnRespawn && playerToRespawn.TryGetComponent(out Rigidbody2D rb))
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }

            playerToRespawn.transform.position = spawnObject.transform.position;
            SceneManager.MoveGameObjectToScene(playerToRespawn.gameObject, scene);
        }

        CleanupRespawnState();
    }

    private void CleanupRespawnState()
    {
        suspicionTimer = 0f;
        playerToRespawn = null;
        isRespawning = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawDebug)
        {
            return;
        }

        Vector2 facing = Application.isPlaying && snitchPatrolling != null ? snitchPatrolling.FacingDirection : Vector2.right;

        if (eyePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(eyePoint.position, eyePoint.position + (Vector3)(facing * instantDistance));
        }

        if (suspicionPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(suspicionPoint.position, suspicionPoint.position + (Vector3)(facing * suspicionDistance));
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, closeTurnDistance);
    }
}