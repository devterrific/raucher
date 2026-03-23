using UnityEngine;

[RequireComponent(typeof(Animator))]
public class SnitchAudio : MonoBehaviour
{
    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");
    private static readonly int IsSuspiciousHash = Animator.StringToHash("IsSuspicious");

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource walkingAudioSource;
    [SerializeField] private AudioSource effectAudioSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip walkingLoopClip;
    [SerializeField] private AudioClip suspiciousClip;
    [SerializeField] private AudioClip shockClip;

    [Header("Walking Settings")]
    [SerializeField, Min(0f)] private float walkingVolume = 1f;
    [SerializeField, Min(0.1f)] private float walkingPitch = 1f;

    [Header("Effect Settings")]
    [SerializeField, Min(0f)] private float suspiciousVolume = 1f;
    [SerializeField, Min(0f)] private float shockVolume = 1f;

    [Header("Suspicious Sound Delay")]
    [SerializeField, Min(0f)] private float suspiciousSoundCooldown = 1.5f;

    private bool wasWalking;
    private bool wasSuspicious;

    private bool walkingPausedByMenu;
    private bool effectPausedByMenu;

    private float nextSuspiciousSoundTime;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        SetupAudioSources();
    }

    private void Update()
    {
        if (IsGamePaused())
        {
            HandlePauseAudio();
            return;
        }

        HandleResumeAudio();
        UpdateWalkingAudio();
        UpdateSuspiciousAudio();
    }

    private void SetupAudioSources()
    {
        if (walkingAudioSource != null)
        {
            walkingAudioSource.playOnAwake = false;
            walkingAudioSource.loop = false;
        }

        if (effectAudioSource != null)
        {
            effectAudioSource.playOnAwake = false;
            effectAudioSource.loop = false;
        }
    }

    private bool IsGamePaused()
    {
        bool isPaused = PauseMenuManager.Instance != null && PauseMenuManager.Instance.IsPaused;
        bool isGameOver = GameOverManager.Instance != null && GameOverManager.Instance.HasGameOverOccurred;

        return isPaused || isGameOver;
    }

    private void HandlePauseAudio()
    {
        if (walkingAudioSource != null && walkingAudioSource.isPlaying)
        {
            walkingAudioSource.Pause();
            walkingPausedByMenu = true;
        }

        if (effectAudioSource != null && effectAudioSource.isPlaying)
        {
            effectAudioSource.Pause();
            effectPausedByMenu = true;
        }
    }

    private void HandleResumeAudio()
    {
        if (walkingPausedByMenu && walkingAudioSource != null)
        {
            walkingAudioSource.UnPause();
            walkingPausedByMenu = false;
        }

        if (effectPausedByMenu && effectAudioSource != null)
        {
            effectAudioSource.UnPause();
            effectPausedByMenu = false;
        }
    }

    private void UpdateWalkingAudio()
    {
        bool isWalking = animator != null && animator.GetBool(IsWalkingHash);

        if (isWalking && !wasWalking)
        {
            StartWalkingLoop();
        }
        else if (!isWalking && wasWalking)
        {
            StopWalkingLoop();
        }

        wasWalking = isWalking;
    }

    private void UpdateSuspiciousAudio()
    {
        bool isSuspicious = animator != null && animator.GetBool(IsSuspiciousHash);

        if (isSuspicious && !wasSuspicious)
        {
            PlaySuspiciousSound();
        }

        wasSuspicious = isSuspicious;
    }

    private void StartWalkingLoop()
    {
        if (walkingAudioSource == null || walkingLoopClip == null)
            return;

        walkingAudioSource.clip = walkingLoopClip;
        walkingAudioSource.volume = walkingVolume;
        walkingAudioSource.pitch = walkingPitch;
        walkingAudioSource.loop = true;

        if (!walkingAudioSource.isPlaying)
            walkingAudioSource.Play();
    }

    private void StopWalkingLoop()
    {
        if (walkingAudioSource == null)
            return;

        walkingAudioSource.Stop();
        walkingAudioSource.loop = false;
        walkingAudioSource.clip = null;
    }

    private void PlaySuspiciousSound()
    {
        if (IsGamePaused())
            return;

        if (effectAudioSource == null || suspiciousClip == null)
            return;

        if (Time.time < nextSuspiciousSoundTime)
            return;

        effectAudioSource.PlayOneShot(suspiciousClip, suspiciousVolume);
        nextSuspiciousSoundTime = Time.time + suspiciousSoundCooldown;
    }

    public void PlayShockSound()
    {
        if (IsGamePaused())
            return;

        if (effectAudioSource == null || shockClip == null)
            return;

        effectAudioSource.PlayOneShot(shockClip, shockVolume);
    }
}