using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class StinkySleepTrigger : MonoBehaviour
{
    private static readonly int IsSleepHash = Animator.StringToHash("IsSleep");

    [Header("Timing")]
    [Tooltip("Wie lange gewartet wird, bis Stinky einschläft.")]
    [Min(0f)]
    [SerializeField] private float waitBeforeSleep = 5f;

    [Tooltip("Wie lange Stinky schläft.")]
    [Min(0f)]
    [SerializeField] private float sleepDuration = 10f;

    [Header("Audio")]
    [SerializeField] private AudioClip sleepSound;
    [Range(0f, 10f)]
    [SerializeField] private float volume = 1f;

    [Header("Control")]
    [SerializeField] private bool enabledTrigger = true;

    private Animator animator;
    private AudioSource audioSource;
    private Coroutine loopRoutine;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.clip = sleepSound;
        audioSource.volume = volume;
    }

    private void OnEnable()
    {
        loopRoutine = StartCoroutine(SleepLoop());
    }

    private IEnumerator SleepLoop()
    {
        while (true)
        {
            if (!enabledTrigger)
            {
                SetSleep(false);
                yield return null;
                continue;
            }

            yield return new WaitForSeconds(waitBeforeSleep);

            if (!enabledTrigger)
                continue;

            SetSleep(true);

            yield return new WaitForSeconds(sleepDuration);

            SetSleep(false);
        }
    }

    private void SetSleep(bool isSleeping)
    {
        animator.SetBool(IsSleepHash, isSleeping);

        if (isSleeping)
        {
            if (sleepSound != null)
            {
                audioSource.clip = sleepSound;
                audioSource.volume = volume;

                if (!audioSource.isPlaying)
                    audioSource.Play();
            }
        }
        else
        {
            if (audioSource.isPlaying)
                audioSource.Stop();
        }
    }

    private void OnDisable()
    {
        if (loopRoutine != null)
            StopCoroutine(loopRoutine);

        SetSleep(false);
    }
}