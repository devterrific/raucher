using UnityEngine;

public class TitleImageIdleAnimation : MonoBehaviour
{
    [Header("Bewegung")]
    [SerializeField] private float moveAmount = 10f;
    [SerializeField] private float moveSpeed = 1f;

    [Header("Skalierung")]
    [SerializeField] private float scaleAmount = 0.03f;
    [SerializeField] private float scaleSpeed = 1f;

    private RectTransform rectTransform;
    private Vector2 startAnchoredPosition;
    private Vector3 startScale;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        startAnchoredPosition = rectTransform.anchoredPosition;
        startScale = rectTransform.localScale;
    }

    private void Update()
    {
        float moveOffset = Mathf.Sin(Time.time * moveSpeed) * moveAmount;
        rectTransform.anchoredPosition = startAnchoredPosition + new Vector2(0f, moveOffset);

        float scaleOffset = Mathf.Sin(Time.time * scaleSpeed) * scaleAmount;
        rectTransform.localScale = startScale + Vector3.one * scaleOffset;
    }

    private void OnDisable()
    {
        if (rectTransform == null)
            return;

        rectTransform.anchoredPosition = startAnchoredPosition;
        rectTransform.localScale = startScale;
    }
}