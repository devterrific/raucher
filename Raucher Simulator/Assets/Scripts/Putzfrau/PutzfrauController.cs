using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class PutzfrauController : MonoBehaviour
{
    private static readonly int RausholenHash = Animator.StringToHash("Rausholen");
    private static readonly int PzzPzzHash = Animator.StringToHash("PzzPzz");

    [Header("Start Delay")]
    [SerializeField] private float waitBeforeStart = 5f;

    [Header("Rausholen")]
    [SerializeField] private float rausholenDuration = 1f;

    [Header("Delay Before PzzPzz")]
    [SerializeField] private float delayBeforePzzPzz = 0f;

    [Header("PzzPzz Sound")]
    [SerializeField] private AudioClip pzzPzzSound;
    [SerializeField, Range(0f, 1f)] private float volume = 1f;

    [Header("Loop")]
    [SerializeField] private bool loop = true;
    [SerializeField] private float delayBetweenLoops = 3f;

    [Header("Control")]
    [SerializeField] private bool enabledTrigger = true;

    private Animator animator;
    private AudioSource audioSource;
    private Coroutine routine;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = volume;

        Debug.Log("[Putzfrau] Awake");
    }

    private void OnEnable()
    {
        Debug.Log("[Putzfrau] OnEnable");
        ResetStates();
        routine = StartCoroutine(MainLoop());
    }

    private IEnumerator MainLoop()
    {
        do
        {
            if (!enabledTrigger)
            {
                Debug.Log("[Putzfrau] Trigger disabled");
                ResetStates();
                yield return null;
                continue;
            }

            Debug.Log("[Putzfrau] WaitBeforeStart: " + waitBeforeStart);
            yield return new WaitForSeconds(waitBeforeStart);

            if (!enabledTrigger)
                continue;

            Debug.Log("[Putzfrau] Rausholen TRUE");
            animator.SetBool(RausholenHash, true);
            animator.SetBool(PzzPzzHash, false);

            yield return new WaitForSeconds(rausholenDuration);

            Debug.Log("[Putzfrau] Rausholen FALSE");
            animator.SetBool(RausholenHash, false);

            if (delayBeforePzzPzz > 0f)
            {
                Debug.Log("[Putzfrau] DelayBeforePzzPzz: " + delayBeforePzzPzz);
                yield return new WaitForSeconds(delayBeforePzzPzz);
            }

            if (!enabledTrigger)
                continue;

            Debug.Log("[Putzfrau] PzzPzz TRUE");
            animator.SetBool(PzzPzzHash, true);

            if (pzzPzzSound != null)
            {
                Debug.Log("[Putzfrau] Play sound: " + pzzPzzSound.name);
                audioSource.clip = pzzPzzSound;
                audioSource.volume = volume;
                audioSource.Play();

                yield return new WaitForSeconds(pzzPzzSound.length);
            }
            else
            {
                Debug.LogWarning("[Putzfrau] Kein Sound gesetzt");
                yield return null;
            }

            Debug.Log("[Putzfrau] PzzPzz FALSE");
            animator.SetBool(PzzPzzHash, false);

            if (delayBetweenLoops > 0f)
            {
                Debug.Log("[Putzfrau] DelayBetweenLoops: " + delayBetweenLoops);
                yield return new WaitForSeconds(delayBetweenLoops);
            }

        } while (loop);
    }

    private void ResetStates()
    {
        if (animator != null)
        {
            animator.SetBool(RausholenHash, false);
            animator.SetBool(PzzPzzHash, false);
        }

        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
    }

    private void OnDisable()
    {
        Debug.Log("[Putzfrau] OnDisable");

        if (routine != null)
            StopCoroutine(routine);

        ResetStates();
    }
}