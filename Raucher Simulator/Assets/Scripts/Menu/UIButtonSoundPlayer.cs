using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonSoundPlayer : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip clickSound;

    public void OnPointerEnter(PointerEventData eventData)
    {
        PlaySound(hoverSound);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        PlaySound(clickSound);
    }

    private void PlaySound(AudioClip clipToPlay)
    {
        if (audioSource == null || clipToPlay == null)
        {
            return;
        }

        audioSource.PlayOneShot(clipToPlay);
    }
}