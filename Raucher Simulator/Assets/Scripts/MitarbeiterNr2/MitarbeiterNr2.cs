using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class MitarbeiterNr2 : MonoBehaviour
{
    private static readonly int MoneyHash = Animator.StringToHash("Money");

    private Animator animator;
    private AudioSource audioSource;

    [Header("Timing (in Minuten)")]
    [Min(0f)][SerializeField] private float minTimeMinutes = 0.5f;
    [Min(0f)][SerializeField] private float maxTimeMinutes = 2f;

    [Header("Active Phase")]
    [Min(0f)][SerializeField] private float activeDurationSeconds = 2f;

    [Header("Control")]
    [SerializeField] private bool enabledTrigger = true;

    [Header("Sound")]
    [Tooltip("Sound der abgespielt wird wenn Money startet")]
    [SerializeField] private AudioClip[] moneySounds;

    [Tooltip("Random Pitch für Variation")]
    [SerializeField] private Vector2 pitchRange = new Vector2(0.9f, 1.1f);

    [Tooltip("Lautstärke vom Sound")]
    [Range(0f, 10f)][SerializeField] private float volume = 1f;

    private float timer;
    private float targetTime;
    private float activeTimer;
    private bool isActive;

    private bool isPausedByMenu = false;
    private bool isGameOver = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        SetNewRandomTime();
    }

    private void OnEnable()
    {
        PauseMenuManager.OnPauseStateChanged += HandlePauseStateChanged;
        GameOverManager.OnGameOverStateChanged += HandleGameOverStateChanged;
    }

    private void OnDisable()
    {
        PauseMenuManager.OnPauseStateChanged -= HandlePauseStateChanged;
        GameOverManager.OnGameOverStateChanged -= HandleGameOverStateChanged;

        audioSource.Stop();
        animator.SetBool(MoneyHash, false);
    }

    private void Update()
    {
        if (isPausedByMenu || isGameOver)
            return;

        if (!enabledTrigger)
            return;

        if (!isActive)
        {
            timer += Time.deltaTime;

            if (timer >= targetTime)
            {
                Activate();
            }
        }
        else
        {
            activeTimer += Time.deltaTime;

            if (activeTimer >= activeDurationSeconds)
            {
                Deactivate();
                SetNewRandomTime();
            }
        }
    }

    private void Activate()
    {
        isActive = true;
        activeTimer = 0f;

        animator.SetBool(MoneyHash, true);
        PlaySoundOnce();
    }

    private void Deactivate()
    {
        isActive = false;
        animator.SetBool(MoneyHash, false);
    }

    private void SetNewRandomTime()
    {
        float min = minTimeMinutes * 60f;
        float max = maxTimeMinutes * 60f;

        targetTime = Random.Range(min, max);
        timer = 0f;
    }

    private void PlaySoundOnce()
    {
        if (moneySounds == null || moneySounds.Length == 0)
            return;

        audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);

        AudioClip clip = moneySounds[Random.Range(0, moneySounds.Length)];
        audioSource.PlayOneShot(clip, volume);
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
            isActive = false;
            animator.SetBool(MoneyHash, false);
        }
        else
        {
            audioSource.Stop();
            isActive = false;
            animator.SetBool(MoneyHash, false);
            SetNewRandomTime();
        }
    }
}