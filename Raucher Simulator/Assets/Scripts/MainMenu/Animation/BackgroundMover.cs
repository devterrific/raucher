using UnityEngine;

public class BackgroundMover : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform targetRectTransform;

    [Header("Positions")]
    [SerializeField] private float mainMenuY = 913f;
    [SerializeField] private float highscoreY = 100f;
    [SerializeField] private float creditsY = 100f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1200f;

    private float currentTargetY;
    private bool isMoving;

    private void Awake()
    {
        if (targetRectTransform == null)
        {
            targetRectTransform = GetComponent<RectTransform>();
        }
    }

    private void Start()
    {
        SetImmediateToMainMenu();
    }

    private void Update()
    {
        if (!isMoving || targetRectTransform == null)
            return;

        Vector2 currentPosition = targetRectTransform.anchoredPosition;

        float t = 1f - Mathf.Exp(-moveSpeed * Time.deltaTime);
        float newY = Mathf.Lerp(currentPosition.y, currentTargetY, t);

        currentPosition.y = newY;
        targetRectTransform.anchoredPosition = currentPosition;

        // 👉 Wichtig: Stoppen, wenn nah genug
        if (Mathf.Abs(currentPosition.y - currentTargetY) < 0.5f)
        {
            currentPosition.y = currentTargetY;
            targetRectTransform.anchoredPosition = currentPosition;
            isMoving = false;
        }
    }

    public void MoveToHighscore()
    {
        currentTargetY = highscoreY;
        isMoving = true;
    }

    public void MoveToCredits()
    {
        currentTargetY = creditsY;
        isMoving = true;
    }

    public void MoveToMainMenu()
    {
        currentTargetY = mainMenuY;
        isMoving = true;
    }

    public void SetImmediateToMainMenu()
    {
        if (targetRectTransform == null)
        {
            return;
        }

        Vector2 position = targetRectTransform.anchoredPosition;
        position.y = mainMenuY;
        targetRectTransform.anchoredPosition = position;

        currentTargetY = mainMenuY;
        isMoving = false;
    }

    public void SetImmediateToHighscore()
    {
        if (targetRectTransform == null)
        {
            return;
        }

        Vector2 position = targetRectTransform.anchoredPosition;
        position.y = highscoreY;
        targetRectTransform.anchoredPosition = position;

        currentTargetY = highscoreY;
        isMoving = false;
    }

    public void SetImmediateToCredits()
    {
        if (targetRectTransform == null)
        {
            return;
        }

        Vector2 position = targetRectTransform.anchoredPosition;
        position.y = creditsY;
        targetRectTransform.anchoredPosition = position;

        currentTargetY = creditsY;
        isMoving = false;
    }
}