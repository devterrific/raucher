using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(SnitchPatrolling))]
public class SnitchVision : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform spritesRoot;          // zieh hier dein Child "Sprites" rein
    [SerializeField] private SpriteRenderer spriteRenderer;  // optional (falls du nur flipX nutzt)
    [SerializeField] private Transform eyePoint;
    [SerializeField] private Transform suspicionPoint;

    [Header("Raycast Settings")]
    [SerializeField] private float instantDistance = 1.5f;
    [SerializeField] private float suspicionDistance = 5f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Suspicion Settings")]
    [SerializeField] private float timeToCatch = 3f;

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

        // Wenn nicht gesetzt, versuch automatisch zu finden
        if (spritesRoot == null)
        {
            Transform t = transform.Find("Sprites");
            if (t != null) spritesRoot = t;
        }

        if (spriteRenderer == null && spritesRoot != null)
            spriteRenderer = spritesRoot.GetComponentInChildren<SpriteRenderer>();

        if (eyePoint == null) Debug.LogError("[SnitchVision] eyePoint not assigned!");
        if (suspicionPoint == null) Debug.LogError("[SnitchVision] suspicionPoint not assigned!");
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void Update()
    {
        if (isRespawning) return;

        // 1) Suspicion läuft zuerst. Solange sie läuft, kein Instant-Catch checken.
        bool suspicionActiveOrCaught = DoSuspicionRay();

        bool instantCaught = false;
        if (!suspicionActiveOrCaught)
            instantCaught = DoInstantRay();

        if (snitchPatrolling != null)
            snitchPatrolling.canMove = !(instantCaught || suspicionActiveOrCaught);
    }

    /// <summary>
    /// Stabile Blickrichtung für 2D:
    /// - bevorzugt: spritesRoot scale.x (funktioniert bei scale.x = -1)
    /// - fallback: SpriteRenderer.flipX (funktioniert bei flipX-only)
    /// </summary>
    private Vector2 FacingDir()
    {
        if (spritesRoot != null)
        {
            // lossyScale berücksichtigt Parent-Scale usw.
            if (spritesRoot.lossyScale.x < 0f) return Vector2.left;
            if (spritesRoot.lossyScale.x > 0f) return Vector2.right;
        }

        if (spriteRenderer != null)
            return spriteRenderer.flipX ? Vector2.left : Vector2.right;

        // last resort
        return Vector2.right;
    }

    private bool DoInstantRay()
    {
        if (eyePoint == null) return false;

        Vector2 dir = FacingDir();
        if (drawDebug) Debug.DrawRay(eyePoint.position, dir * instantDistance, Color.red);

        RaycastHit2D hit = Physics2D.Raycast(eyePoint.position, dir, instantDistance, playerLayer);
        if (hit.collider == null) return false;

        PlayerMain player = hit.collider.GetComponent<PlayerMain>();
        if (player == null) return false;
        if (!player.Detectable) return false;

        Debug.Log("[SnitchVision] Player got caught (instant)!");
        StartRespawn(player);
        return true;
    }

    private bool DoSuspicionRay()
    {
        if (suspicionPoint == null) return false;

        Vector2 dir = FacingDir();
        if (drawDebug) Debug.DrawRay(suspicionPoint.position, dir * suspicionDistance, Color.yellow);

        RaycastHit2D hit = Physics2D.Raycast(suspicionPoint.position, dir, suspicionDistance, playerLayer);

        if (hit.collider == null)
        {
            suspicionTimer = 0f;
            return false;
        }

        PlayerMain player = hit.collider.GetComponent<PlayerMain>();
        if (player == null)
        {
            suspicionTimer = 0f;
            return false;
        }

        if (!player.Detectable)
        {
            suspicionTimer = 0f;
            return false;
        }

        suspicionTimer += Time.deltaTime;

        if (suspicionTimer >= timeToCatch)
        {
            Debug.Log("[SnitchVision] Player got caught after timed suspicion!");
            suspicionTimer = 0f;

            StartRespawn(player);
            return true;
        }

        // Verdacht läuft -> Snitch bleibt stehen
        return true;
    }

    private void StartRespawn(PlayerMain player)
    {
        if (isRespawning) return;

        isRespawning = true;
        playerToRespawn = player;

        // Player überlebt Scene-Load, damit wir ihn nach dem Laden repositionieren können
        DontDestroyOnLoad(player.gameObject);

        SceneManager.LoadScene(respawnSceneName, LoadSceneMode.Single);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!isRespawning) return;
        if (scene.name != respawnSceneName) return;

        Transform spawn = GameObject.Find(respawnPointName)?.transform;
        if (spawn == null)
        {
            Debug.LogError($"[SnitchVision] RespawnPoint '{respawnPointName}' not found in scene '{respawnSceneName}'!");
            isRespawning = false;
            return;
        }

        if (playerToRespawn != null)
        {
            if (resetVelocityOnRespawn)
            {
                Rigidbody2D rb = playerToRespawn.GetComponent<Rigidbody2D>();
                if (rb != null) rb.velocity = Vector2.zero;
            }

            playerToRespawn.transform.position = spawn.position;

            // zurück in die geladene Scene schieben (sauberer als dauerhaft DontDestroy)
            SceneManager.MoveGameObjectToScene(playerToRespawn.gameObject, scene);
        }

        playerToRespawn = null;
        isRespawning = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawDebug) return;

        Vector2 dir = Vector2.right;

        // OnDrawGizmosSelected läuft auch im Editor ohne Awake -> hier defensiv
        Transform sr = spritesRoot != null ? spritesRoot : transform.Find("Sprites");
        if (sr != null)
        {
            dir = sr.lossyScale.x < 0f ? Vector2.left : Vector2.right;
        }
        else if (spriteRenderer != null)
        {
            dir = spriteRenderer.flipX ? Vector2.left : Vector2.right;
        }

        if (eyePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(eyePoint.position, eyePoint.position + (Vector3)dir * instantDistance);
        }

        if (suspicionPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(suspicionPoint.position, suspicionPoint.position + (Vector3)dir * suspicionDistance);
        }
    }
}