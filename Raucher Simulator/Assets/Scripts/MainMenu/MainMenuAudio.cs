using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MainMenuAudio : MonoBehaviour
{
    private const string SoundEnabledKey = "SoundEnabled";
    private const string MasterVolumeKey = "MasterVolume";

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        ApplySavedAudioSettings();
        PlayIfAllowed();
    }

    public void RefreshAudioFromSettings()
    {
        ApplySavedAudioSettings();

        bool isSoundEnabled = PlayerPrefs.GetInt(SoundEnabledKey, 1) == 1;

        if (!isSoundEnabled)
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            return;
        }

        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    private void ApplySavedAudioSettings()
    {
        bool isSoundEnabled = PlayerPrefs.GetInt(SoundEnabledKey, 1) == 1;
        float volume = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);

        audioSource.volume = isSoundEnabled ? volume : 0f;
    }

    private void PlayIfAllowed()
    {
        bool isSoundEnabled = PlayerPrefs.GetInt(SoundEnabledKey, 1) == 1;

        if (isSoundEnabled && audioSource.clip != null)
        {
            audioSource.Play();
        }
    }
}