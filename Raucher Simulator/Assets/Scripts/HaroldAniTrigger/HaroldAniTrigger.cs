using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class HaroldAniTrigger : MonoBehaviour
{
    private static readonly int AniHash = Animator.StringToHash("Ani");

    [Header("Timing (in Minuten)")]
    [Tooltip("Minimale Zeit bis zum nächsten Trigger.")]
    [Min(0f)]
    [SerializeField] private float minTimeMinutes = 1f;

    [Tooltip("Maximale Zeit bis zum nächsten Trigger.")]
    [Min(0f)]
    [SerializeField] private float maxTimeMinutes = 5f;

    [Header("Active Phase")]
    [Tooltip("Wie lange Ani auf TRUE bleibt (in Sekunden).")]
    [Min(0f)]
    [SerializeField] private float activeDurationSeconds = 2f;

    [Header("Control")]
    [Tooltip("Wenn deaktiviert, passiert gar nichts.")]
    [SerializeField] private bool enabledTrigger = true;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        StartCoroutine(Loop());
    }

    private IEnumerator Loop()
    {
        while (true)
        {
            if (!enabledTrigger)
            {
                animator.SetBool(AniHash, false);
                yield return null;
                continue;
            }

            float waitTime = Random.Range(minTimeMinutes * 60f, maxTimeMinutes * 60f);
            yield return new WaitForSeconds(waitTime);

            if (!enabledTrigger)
                continue;

            animator.SetBool(AniHash, true);

            yield return new WaitForSeconds(activeDurationSeconds);

            animator.SetBool(AniHash, false);
        }
    }

    private void OnDisable()
    {
        animator.SetBool(AniHash, false);
        StopAllCoroutines();
    }
}