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

    private bool isPausedByMenu = false;
    private bool isGameOver = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        PauseMenuManager.OnPauseStateChanged += HandlePauseStateChanged;
        GameOverManager.OnGameOverStateChanged += HandleGameOverStateChanged;

        StartCoroutine(Loop());
    }

    private void OnDisable()
    {
        PauseMenuManager.OnPauseStateChanged -= HandlePauseStateChanged;
        GameOverManager.OnGameOverStateChanged -= HandleGameOverStateChanged;

        animator.SetBool(AniHash, false);
        audioSource.Stop();
        StopAllCoroutines();
    }

    private IEnumerator Loop()
    {
        while (true)
        {
            if (isPausedByMenu || isGameOver)
            {
                yield return null;
                continue;
            }

            if (!enabledTrigger)
            {
                animator.SetBool(AniHash, false);
                yield return null;
                continue;
            }

            float waitTime = Random.Range(minTimeMinutes * 60f, maxTimeMinutes * 60f);
            yield return new WaitForSeconds(waitTime);

            if (!enabledTrigger || isPausedByMenu || isGameOver)
                continue;

            animator.SetBool(AniHash, true);

            if (sound != null)
            {
                audioSource.PlayOneShot(sound, volume);
                yield return new WaitForSeconds(sound.length);
            }
            else
            {
                yield return new WaitForSeconds(activeDurationSeconds);
            }

            if (!isPausedByMenu && !isGameOver)
            {
                animator.SetBool(AniHash, false);
            }
        }
    }

    private void HandlePauseStateChanged(bool isPaused)
    {
        isPausedByMenu = isPaused;

        if (isPausedByMenu)
        {
            audioSource.Pause();
        }
        else
        {
            if (!isGameOver)
            {
                audioSource.UnPause();
            }
        }
    }

    private void HandleGameOverStateChanged(bool gameOverActive)
    {
        isGameOver = gameOverActive;

        if (isGameOver)
        {
            audioSource.Stop();
            animator.SetBool(AniHash, false);
        }
        else
        {
            animator.SetBool(AniHash, false);
        }
    }
}