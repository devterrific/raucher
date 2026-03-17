using UnityEngine;

public class SpeechBubbleIdleAnimation : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float moveAmplitude = 8f;
    [SerializeField] private float moveSpeed = 2f;

    [Header("Scale")]
    [SerializeField] private float scaleAmplitude = 0.03f;
    [SerializeField] private float scaleSpeed = 2f;

    private RectTransform rectTransform;
    private Vector2 startAnchoredPosition;
    private Vector3 startScale;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        startScale = transform.localScale;

        if (rectTransform != null)
        {
            startAnchoredPosition = rectTransform.anchoredPosition;
        }
    }

    private void OnEnable()
    {
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = startAnchoredPosition;
        }

        transform.localScale = startScale;
    }

    private void Update()
    {
        float time = Time.unscaledTime;

        float yOffset = Mathf.Sin(time * moveSpeed) * moveAmplitude;
        float scaleOffset = Mathf.Sin(time * scaleSpeed) * scaleAmplitude;

        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = startAnchoredPosition + new Vector2(0f, yOffset);
        }

        transform.localScale = startScale + Vector3.one * scaleOffset;
    }

    private void OnDisable()
    {
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = startAnchoredPosition;
        }

        transform.localScale = startScale;
    }
}