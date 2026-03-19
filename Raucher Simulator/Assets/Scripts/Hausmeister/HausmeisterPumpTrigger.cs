using UnityEngine;

[RequireComponent(typeof(Animator))]
public class HausmeisterPumpTrigger : MonoBehaviour
{
    private static readonly int PumpHash = Animator.StringToHash("Pump");

    [Header("Player Detection")]
    [Tooltip("Wenn der Player diesen Abstand oder weniger hat, wird Pump auf true gesetzt.")]
    [Min(0f)]
    [SerializeField] private float enterDistance = 2f;

    [Tooltip("Wenn der Player diesen Abstand oder mehr hat, wird Pump auf false gesetzt.")]
    [Min(0f)]
    [SerializeField] private float exitDistance = 3f;

    [Tooltip("Wenn deaktiviert, reagiert der Hausmeister nicht auf den Player.")]
    [SerializeField] private bool detectionEnabled = true;

    private Animator animator;
    private Transform player;
    private bool isPumping;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (!detectionEnabled)
            return;

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
                player = playerObject.transform;
            else
                return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        if (!isPumping && distance <= enterDistance)
        {
            isPumping = true;
            animator.SetBool(PumpHash, true);
        }
        else if (isPumping && distance >= exitDistance)
        {
            isPumping = false;
            animator.SetBool(PumpHash, false);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, enterDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, exitDistance);
    }
#endif
}