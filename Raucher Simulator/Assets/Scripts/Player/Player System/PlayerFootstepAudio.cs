using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[DisallowMultipleComponent]
public class PlayerFootstepAudio : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInputReader input;
    [SerializeField] private PlayerMovementState movementState;
    [SerializeField] private PlayerMovementLockController movementLocks;

    [Header("Audio")]
    [SerializeField] private AudioClip footstepClip;

    [Header("Settings")]
    [SerializeField] private float walkPitch = 1f;
    [SerializeField] private float sprintPitch = 1.2f;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (input == null)
            input = GetComponent<PlayerInputReader>();

        if (movementState == null)
            movementState = GetComponent<PlayerMovementState>();

        if (movementLocks == null)
            movementLocks = GetComponent<PlayerMovementLockController>();

        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.clip = footstepClip;
    }

    private void Update()
    {
        HandleFootsteps();
    }

    private void HandleFootsteps()
    {
        if (ShouldBlockAudio())
        {
            StopFootsteps();
            return;
        }

        bool isMoving = Mathf.Abs(input.MoveX) > 0.01f;

        if (!isMoving)
        {
            StopFootsteps();
            return;
        }

        if (!audioSource.isPlaying)
            audioSource.Play();

        if (movementState.CurrentMode == PlayerMovementState.MovementMode.Sprint)
            audioSource.pitch = sprintPitch;
        else
            audioSource.pitch = walkPitch;
    }

    private bool ShouldBlockAudio()
    {
        if (Time.timeScale == 0f)
            return true;

        if (movementLocks != null && !movementLocks.CanMove)
            return true;

        if (movementState == null)
            return true;

        if (movementState.CurrentMode == PlayerMovementState.MovementMode.Locked)
            return true;

        return false;
    }

    private void StopFootsteps()
    {
        if (audioSource.isPlaying)
            audioSource.Stop();
    }
}