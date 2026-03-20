using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class Door2 : Interactable
{
    private static readonly int OpenHash = Animator.StringToHash("Open");
    private static readonly int CloseHash = Animator.StringToHash("Close");

    private Animator animator;
    private AudioSource audioSource;

    [Header("Door")]
    [SerializeField] private bool isOpen = false;

    [Tooltip("Wie lange die Tür offen bleibt, bevor sie wieder schließt")]
    [SerializeField, Min(0f)] private float autoCloseDelay = 15f;

    [Header("Audio")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;
    [SerializeField, Min(0.1f)] private float soundPitch = 1f;
    [SerializeField, Min(0f)] private float soundVolume = 1f;

    private float closeTimer = 0f;
    private bool isClosing = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (!isOpen || isClosing)
            return;

        closeTimer += Time.deltaTime;

        if (closeTimer >= autoCloseDelay)
        {
            CloseDoor();
        }
    }

    public override void Interact(PlayerMain player)
    {
        if (isOpen || isClosing)
            return;

        OpenDoor();
    }

    private void OpenDoor()
    {
        isOpen = true;
        isClosing = false;
        closeTimer = 0f;

        animator.SetBool(CloseHash, false);
        animator.SetBool(OpenHash, true);

        PlaySound(openSound);
    }

    private void CloseDoor()
    {
        isClosing = true;
        closeTimer = 0f;

        animator.SetBool(OpenHash, false);
        animator.SetBool(CloseHash, true);

        PlaySound(closeSound);

        float closeAnimLength = GetClipLength("TurClose");
        Invoke(nameof(ResetToNormal), closeAnimLength);
    }

    private void ResetToNormal()
    {
        animator.SetBool(CloseHash, false);

        isOpen = false;
        isClosing = false;
        closeTimer = 0f;
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip == null || audioSource == null)
            return;

        audioSource.pitch = soundPitch;
        audioSource.PlayOneShot(clip, soundVolume);
    }

    private float GetClipLength(string clipName)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return 0.1f;

        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == clipName)
                return clip.length;
        }

        return 0.1f;
    }
}