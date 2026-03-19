using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(SnitchPatrolling))]
[RequireComponent(typeof(Animator))]
public class SnitchVision : MonoBehaviour
{
    private static readonly int IsSuspiciousHash = Animator.StringToHash("IsSuspicious");
    private static readonly int DoShockHash = Animator.StringToHash("DoShock");

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

    [Header("Catch Timing")]
    [SerializeField, Min(0f)] private float catchDelay = 1.0f;

    [Header("Respawn (Scene)")]
    [SerializeField] private string respawnSceneName = "Buro";
    [SerializeField] private string respawnPointName = "startBuro";
    [SerializeField] private bool resetVelocityOnRespawn = true;

    [Header("Snitch Audio")]
    [SerializeField] private SnitchAudio snitchAudio;

    private SnitchPatrolling snitchPatrolling;
    private Animator animator;

    private float suspicionTimer;
    private bool isCatching;
    private bool isRespawning;
    private PlayerMain playerToRespawn;

    private void Awake()
    {
        snitchPatrolling = GetComponent<SnitchPatrolling>();
        animator = GetComponent<Animator>();
        snitchAudio = GetComponent<SnitchAudio>();          // NEU: Für die Snitch-Audio
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void Update()
    {
        if (isRespawning || isCatching)
        {
            ApplyMovementStop(true);
            return;
        }

        TryTriggerCloseTurnReaction();

        bool seesSuspicion = CheckSuspicionVision(out PlayerMain suspiciousPlayer);
        bool seesInstant = CheckInstantVision(out PlayerMain instantPlayer);

        if (seesInstant)
        {
            StartCatch(instantPlayer);
            return;
        }

        if (seesSuspicion)
        {
            ApplyMovementStop(true);
            SetSuspicious(true);

            suspicionTimer += Time.deltaTime;
            if (suspicionTimer >= timeToCatch)
                StartCatch(suspiciousPlayer);

            return;
        }

        suspicionTimer = 0f;
        SetSuspicious(false);
        ApplyMovementStop(false);
    }

    private Vector2 FacingDirection => snitchPatrolling != null ? snitchPatrolling.FacingDirection : Vector2.right;

    private void TryTriggerCloseTurnReaction()
    {
        if (snitchPatrolling == null || closeTurnDistance <= 0f)
            return;

        Collider2D hit = Physics2D.OverlapCircle(transform.position, closeTurnDistance, playerLayer);
        if (hit == null || !hit.TryGetComponent(out PlayerMain player) || !player.Detectable)
            return;

        snitchPatrolling.TryStartCloseTurnTowards(player.transform.position, closeTurnDuration, closeTurnCooldown);
    }

    private bool CheckInstantVision(out PlayerMain player)
    {
        player = null;
        if (eyePoint == null)
            return false;

        return TryGetDetectablePlayer(eyePoint.position, FacingDirection, instantDistance, out player);
    }

    private bool CheckSuspicionVision(out PlayerMain player)
    {
        player = null;
        if (suspicionPoint == null)
            return false;

        return TryGetDetectablePlayer(suspicionPoint.position, FacingDirection, suspicionDistance, out player);
    }

    private bool TryGetDetectablePlayer(Vector2 origin, Vector2 direction, float distance, out PlayerMain player)
    {
        player = null;

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, distance, playerLayer);
        if (hit.collider == null)
            return false;

        if (!hit.collider.TryGetComponent(out PlayerMain foundPlayer) || !foundPlayer.Detectable)
            return false;

        player = foundPlayer;
        return true;
    }

    private void StartCatch(PlayerMain player)
    {
        if (player == null || isCatching || isRespawning)
            return;

        isCatching = true;
        playerToRespawn = player;
        suspicionTimer = 0f;

        SetSuspicious(false);
        ApplyMovementStop(true);

        if (animator != null)
            animator.SetTrigger(DoShockHash);

        //  Neu: Für die Snitch-Audio
        if (snitchAudio != null )
        {
            snitchAudio.PlayShockSound();
        }
        //  -------------------------------

        StartCoroutine(CatchRoutine());
    }

    private IEnumerator CatchRoutine()
    {
        yield return new WaitForSeconds(catchDelay);
        StartRespawn(playerToRespawn);
    }

    private void SetSuspicious(bool value)
    {
        if (animator != null)
            animator.SetBool(IsSuspiciousHash, value);
    }

    private void ApplyMovementStop(bool shouldStop)
    {
        if (snitchPatrolling != null)
            snitchPatrolling.canMove = !shouldStop;
    }

    private void StartRespawn(PlayerMain player)
    {
        if (isRespawning || player == null)
            return;

        isRespawning = true;
        DontDestroyOnLoad(player.gameObject);
        SceneManager.LoadScene(respawnSceneName, LoadSceneMode.Single);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!isRespawning || scene.name != respawnSceneName)
            return;

        GameObject spawnObject = GameObject.Find(respawnPointName);
        if (spawnObject == null)
        {
            CleanupState();
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

        CleanupState();
    }

    private void CleanupState()
    {
        suspicionTimer = 0f;
        isCatching = false;
        isRespawning = false;
        playerToRespawn = null;
        SetSuspicious(false);
        ApplyMovementStop(false);
    }
}