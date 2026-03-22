using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AmbientLoop : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioClip clip;
    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;

    [Header("Options")]
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool loop = true;
    [SerializeField] private bool dontDestroyOnLoad = false;

    [Header("Delay")]
    [Min(0f)]
    [SerializeField] private float startDelay = 0f;

    private AudioSource source;
    private bool pausedByMenu;

    private void Awake()
    {
        source = GetComponent<AudioSource>();

        source.playOnAwake = false;
        source.loop = loop;
        source.spatialBlend = 0f;
        source.volume = volume;

        if (clip != null)
            source.clip = clip;

        if (dontDestroyOnLoad)
            DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (playOnStart && clip != null)
        {
            if (startDelay > 0f)
                Invoke(nameof(Play), startDelay);
            else
                Play();
        }
    }

    private void Update()
    {
        if (IsGamePaused())
        {
            PauseAudio();
            return;
        }

        ResumeAudio();
    }

    public void Play()
    {
        if (source.clip == null)
            return;

        if (!source.isPlaying)
            source.Play();
    }

    public void Stop()
    {
        if (source.isPlaying)
            source.Stop();

        pausedByMenu = false;
    }

    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        source.volume = volume;
    }

    private bool IsGamePaused()
    {
        return PauseMenuManager.Instance != null && PauseMenuManager.Instance.IsPaused;
    }

    private void PauseAudio()
    {
        if (source != null && source.isPlaying)
        {
            source.Pause();
            pausedByMenu = true;
        }
    }

    private void ResumeAudio()
    {
        if (pausedByMenu && source != null)
        {
            source.UnPause();
            pausedByMenu = false;
        }
    }
}