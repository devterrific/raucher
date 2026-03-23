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

    [Tooltip("Wie lange vom Clip abgespielt werden soll (Sekunden)")]
    [SerializeField] private float clipDuration = 0.25f;

    [Header("Pitch")]
    [SerializeField] private float walkPitch = 1f;
    [SerializeField] private float sprintPitch = 1.2f;

    private AudioSource audioSource;
    private Coroutine stopRoutine;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (input == null)
            input = GetComponent<PlayerInputReader>();

        if (movementState == null)
            movementState = GetComponent<PlayerMovementState>();

        if (movementLocks == null)
            movementLocks = GetComponent<PlayerMovementLockController>();

        audioSource.loop = false;
        audioSource.playOnAwake = false;
    }

    public void Footstep()
    {
        if (ShouldBlockAudio())
            return;

        bool isMoving = Mathf.Abs(input.MoveX) > 0.01f;

        if (!isMoving)
            return;

        PlayFootstep();
    }

    private void PlayFootstep()
    {
        if (footstepClip == null)
            return;

        if (movementState.CurrentMode == PlayerMovementState.MovementMode.Sprint)
            audioSource.pitch = sprintPitch;
        else
            audioSource.pitch = walkPitch;

        audioSource.clip = footstepClip;
        audioSource.time = 0f;

        audioSource.Play();

        if (stopRoutine != null)
            StopCoroutine(stopRoutine);

        stopRoutine = StartCoroutine(StopAfterTime());
    }

    private System.Collections.IEnumerator StopAfterTime()
    {
        yield return new WaitForSeconds(clipDuration);

        if (audioSource.isPlaying)
            audioSource.Stop();
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
}