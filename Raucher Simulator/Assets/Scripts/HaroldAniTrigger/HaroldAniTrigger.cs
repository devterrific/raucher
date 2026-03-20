using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class HaroldAniTrigger : MonoBehaviour
{
    private static readonly int AniHash = Animator.StringToHash("Ani");

    [Header("Timing (in Minuten)")]
    [Min(0f)]
    [SerializeField] private float minTimeMinutes = 1f;

    [Min(0f)]
    [SerializeField] private float maxTimeMinutes = 5f;

    [Header("Fallback Dauer (wenn kein Sound)")]
    [Min(0f)]
    [SerializeField] private float activeDurationSeconds = 2f;

    [Header("Audio")]
    [SerializeField] private AudioClip sound;
    [Range(0f, 10f)]
    [SerializeField] private float volume = 1f;

    [Header("Control")]
    [SerializeField] private bool enabledTrigger = true;

    private Animator animator;
    private AudioSource audioSource;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
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

            if (sound != null)
            {
                audioSource.PlayOneShot(sound, volume);

                // Warten bis Sound fertig ist
                yield return new WaitForSeconds(sound.length);
            }
            else
            {
                // Fallback wenn kein Sound gesetzt
                yield return new WaitForSeconds(activeDurationSeconds);
            }

            animator.SetBool(AniHash, false);
        }
    }

    private void OnDisable()
    {
        animator.SetBool(AniHash, false);
        StopAllCoroutines();
    }
}