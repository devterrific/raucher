using UnityEngine;
using UnityEngine.UI;

public class CharacterSpeechBubbleUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject bubbleRoot;
    [SerializeField] private Text bubbleText;

    [Header("Optional")]
    [SerializeField] private bool hideWhenEmpty = true;

    private void Awake()
    {
        Clear();
    }

    public void ShowMessage(string message)
    {
        if (bubbleText != null)
        {
            bubbleText.text = message;
        }

        if (bubbleRoot != null)
        {
            bool shouldShow = hideWhenEmpty == false || string.IsNullOrWhiteSpace(message) == false;
            bubbleRoot.SetActive(shouldShow);
        }
    }

    public void Clear()
    {
        if (bubbleText != null)
        {
            bubbleText.text = "";
        }

        if (bubbleRoot != null && hideWhenEmpty)
        {
            bubbleRoot.SetActive(false);
        }
    }
}