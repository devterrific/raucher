using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UIStageSlideFade : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private RectTransform target;
    [SerializeField] private float startX = 800f;
    [SerializeField] private float centerX = 0f;
    [SerializeField] private float endX = -800f;

    [Header("Timing")]
    [SerializeField] private float moveInDuration = 0.5f;
    [SerializeField] private float stayDuration = 1f;
    [SerializeField] private float moveOutDuration = 0.5f;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        if (target == null)
            target = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        StartCoroutine(PlayAnimation());
    }

    private IEnumerator PlayAnimation()
    {
        SetPosition(startX);
        canvasGroup.alpha = 0f;

        float t = 0f;
        while (t < moveInDuration)
        {
            t += Time.deltaTime;
            float lerp = t / moveInDuration;

            SetPosition(Mathf.Lerp(startX, centerX, lerp));
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, lerp);

            yield return null;
        }

        yield return new WaitForSeconds(stayDuration);

        t = 0f;
        while (t < moveOutDuration)
        {
            t += Time.deltaTime;
            float lerp = t / moveOutDuration;

            SetPosition(Mathf.Lerp(centerX, endX, lerp));
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, lerp);

            yield return null;
        }
    }

    private void SetPosition(float x)
    {
        if (target == null) return;

        Vector2 pos = target.anchoredPosition;
        pos.x = x;
        target.anchoredPosition = pos;
    }
}