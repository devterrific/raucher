using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class Door2 : Interactable
{
    private Animator animator;
    private AudioSource audioSource;

    private static readonly int OpenHash = Animator.StringToHash("Open");

    [Header("Door")]
    [SerializeField] private bool isOpen = false;

    [Header("Audio")]
    [SerializeField] private AudioClip openSound;
    [SerializeField, Min(0.1f)] private float soundPitch = 1f;
    [SerializeField, Min(0f)] private float soundVolume = 1f;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    public override void Interact(PlayerMain player)
    {
        if (isOpen)
            return;

        isOpen = true;
        animator.SetBool(OpenHash, true);

        PlayOpenSound();
    }

    private void PlayOpenSound()
    {
        if (openSound == null || audioSource == null)
            return;

        audioSource.pitch = soundPitch;
        audioSource.PlayOneShot(openSound, soundVolume);
    }
}