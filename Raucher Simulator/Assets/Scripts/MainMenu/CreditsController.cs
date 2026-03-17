using UnityEngine;
using TMPro;

public class CreditsController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI creditsText;
    [SerializeField] private RectTransform creditsTextRect;
    [SerializeField] private RectTransform viewportRect;

    [Header("Content")]
    [TextArea(10, 30)]
    [SerializeField]
    private string creditsContent =
@"GAME TITLE

Developed by:


Programming:
[Your Name]

Game Design:
[Your Name]

Art & Assets:
[Your Name / Sources]

Sound & Music:
[Your Name / Sources]

Special Thanks:
- Friends & Supporters
- Unity Engine

Thank you for playing!";

    [Header("Scroll")]
    [SerializeField] private float scrollSpeed = 40f;
    [SerializeField] private float startOffset = 40f;
    [SerializeField] private float endOffset = 40f;

    private void Start()
    {
        ApplyCredits();
        Canvas.ForceUpdateCanvases();
        ResetScrollPosition();
    }

    private void Update()
    {
        ScrollCredits();
    }

    private void ApplyCredits()
    {
        if (creditsText == null)
        {
            Debug.LogWarning("CreditsController: No Text assigned.");
            return;
        }

        creditsText.text = creditsContent;
    }

    private void ResetScrollPosition()
    {
        if (creditsTextRect == null || viewportRect == null)
        {
            return;
        }

        float viewportHalfHeight = viewportRect.rect.height * 0.5f;

        Vector2 position = creditsTextRect.anchoredPosition;
        position.y = -viewportHalfHeight - startOffset;
        creditsTextRect.anchoredPosition = position;
    }

    private void ScrollCredits()
    {
        if (creditsTextRect == null || viewportRect == null)
        {
            return;
        }

        Vector2 position = creditsTextRect.anchoredPosition;
        position.y += scrollSpeed * Time.deltaTime;
        creditsTextRect.anchoredPosition = position;

        if (IsTextCompletelyOutOfView())
        {
            ResetScrollPosition();
        }
    }

    private bool IsTextCompletelyOutOfView()
    {
        float viewportHalfHeight = viewportRect.rect.height * 0.5f;
        float textHeight = creditsTextRect.rect.height;

        float textBottomY = creditsTextRect.anchoredPosition.y - textHeight;

        return textBottomY >= viewportHalfHeight + endOffset;
    }
}