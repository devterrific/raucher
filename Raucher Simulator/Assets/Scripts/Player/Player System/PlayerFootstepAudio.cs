using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[DisallowMultipleComponent]
public class PlayerFootstepAudio : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInputReader input;
    [SerializeField] private PlayerMovementState movementState;

    [Header("Audio")]
    [SerializeField] private AudioClip footstepClip;

    [Header("Settings")]
    [SerializeField] private float walkPitch = 1f;
    [SerializeField] private float sprintPitch = 1.3f;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (input == null)
            input = GetComponent<PlayerInputReader>();

        if (movementState == null)
            movementState = GetComponent<PlayerMovementState>();

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
        bool isMoving = Mathf.Abs(input.MoveX) > 0.01f;
        bool canMove = movementState.CurrentMode != PlayerMovementState.MovementMode.Locked;

        if (isMoving && canMove)
        {
            if (!audioSource.isPlaying)
                audioSource.Play();

            if (movementState.CurrentMode == PlayerMovementState.MovementMode.Sprint)
                audioSource.pitch = sprintPitch;
            else
                audioSource.pitch = walkPitch;
        }
        else
        {
            if (audioSource.isPlaying)
                audioSource.Stop();
        }
    }
}