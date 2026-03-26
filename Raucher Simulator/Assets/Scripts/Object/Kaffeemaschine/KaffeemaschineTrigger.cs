using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class KaffeemaschineTrigger : MonoBehaviour
{
    private static readonly int ZZZHash = Animator.StringToHash("ZZZ");

    [Header("Player Detection")]
    [Tooltip("Wenn der Player diesen Abstand oder weniger hat, wird ZZZ einmalig auf true gesetzt.")]
    [Min(0f)]
    [SerializeField] private float triggerDistance = 2f;

    [Header("Sound")]
    [Tooltip("Diesen Sound kannst du später per Animation Event abspielen.")]
    [SerializeField] private AudioClip eventSound;

    private Animator animator;
    private AudioSource audioSource;
    private Transform player;
    private bool hasTriggered;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (hasTriggered)
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

        if (distance <= triggerDistance)
        {
            hasTriggered = true;
            animator.SetBool(ZZZHash, true);
        }
    }

    public void PlayEventSound()
    {
        if (eventSound == null || audioSource == null)
            return;

        audioSource.PlayOneShot(eventSound);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, triggerDistance);
    }
#endif
}