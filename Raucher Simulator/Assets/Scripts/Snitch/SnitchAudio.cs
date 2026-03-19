using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Animator))]
public class SnitchAudio : MonoBehaviour
{
    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");
    private static readonly int IsSuspiciousHash = Animator.StringToHash("IsSuspicious");

    [Header("References")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Animator animator;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip walkingLoopClip;
    [SerializeField] private AudioClip suspiciousClip;
    [SerializeField] private AudioClip shockClip;

    [Header("Walking Settings")]
    [SerializeField, Min(0f)] private float walkingVolume = 1f;
    [SerializeField, Min(0.1f)] private float walkingPitch = 1f;

    [Header("One Shot Settings")]
    [SerializeField, Min(0f)] private float suspiciousVolume = 1f;
    [SerializeField, Min(0f)] private float shockVolume = 1f;

    private bool wasWalking;
    private bool wasSuspicious;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (animator == null)
            animator = GetComponent<Animator>();

        SetupAudioSource();
    }

    private void Update()
    {
        UpdateWalkingAudio();
        UpdateSuspiciousAudio();
    }

    private void SetupAudioSource()
    {
        audioSource.playOnAwake = false;
        audioSource.loop = false;
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
        if (walkingLoopClip == null || audioSource == null)
            return;

        audioSource.clip = walkingLoopClip;
        audioSource.volume = walkingVolume;
        audioSource.pitch = walkingPitch;
        audioSource.loop = true;
        audioSource.Play();
    }

    private void StopWalkingLoop()
    {
        if (audioSource == null)
            return;

        if (audioSource.clip == walkingLoopClip)
            audioSource.Stop();

        audioSource.loop = false;
        audioSource.clip = null;
    }

    private void PlaySuspiciousSound()
    {
        if (suspiciousClip == null || audioSource == null)
            return;

        audioSource.PlayOneShot(suspiciousClip, suspiciousVolume);
    }

    public void PlayShockSound()
    {
        if (shockClip == null || audioSource == null)
            return;

        audioSource.PlayOneShot(shockClip, shockVolume);
    }
}