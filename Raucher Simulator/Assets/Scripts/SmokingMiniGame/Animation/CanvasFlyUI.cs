using UnityEngine;

public class CanvasFlyUI : MonoBehaviour
{
    [Header("Bewegung")]
    [SerializeField] private float moveSpeed = 140f;
    [SerializeField] private float directionChangeInterval = 1.2f;
    [SerializeField] private float directionStrength = 0.6f;

    [Header("Zittern")]
    [SerializeField] private float jitterAmount = 6f;
    [SerializeField] private float jitterSpeed = 18f;

    [Header("Bewegungsbereich")]
    [SerializeField] private RectTransform movementArea;

    private RectTransform flyRectTransform;

    private Vector2 currentDirection;
    private float directionTimer;
    private Vector2 basePosition;

    private void Awake()
    {
        flyRectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        currentDirection = Random.insideUnitCircle.normalized;

        if (currentDirection == Vector2.zero)
        {
            currentDirection = Vector2.right;
        }

        directionTimer = directionChangeInterval;
        basePosition = flyRectTransform.anchoredPosition;
    }

    private void Update()
    {
        UpdateDirection();
        MoveFly();
        KeepInsideArea();
        ApplyJitter();
    }

    private void UpdateDirection()
    {
        directionTimer -= Time.deltaTime;

        if (directionTimer <= 0f)
        {
            Vector2 randomOffset = Random.insideUnitCircle * directionStrength;
            currentDirection = (currentDirection + randomOffset).normalized;

            if (currentDirection == Vector2.zero)
            {
                currentDirection = Vector2.right;
            }

            directionTimer = directionChangeInterval;
        }
    }

    private void MoveFly()
    {
        Vector2 movement = currentDirection * moveSpeed * Time.deltaTime;
        basePosition += movement;
    }

    private void ApplyJitter()
    {
        float jitterX = Mathf.Sin(Time.time * jitterSpeed) * jitterAmount;
        float jitterY = Mathf.Cos(Time.time * jitterSpeed * 1.15f) * jitterAmount;

        Vector2 jitterOffset = new Vector2(jitterX, jitterY);
        flyRectTransform.anchoredPosition = basePosition + jitterOffset;
    }

    private void KeepInsideArea()
    {
        if (movementArea == null)
        {
            flyRectTransform.anchoredPosition = basePosition;
            return;
        }

        Vector2 areaSize = movementArea.rect.size;
        Vector2 flySize = flyRectTransform.rect.size;

        float minX = -areaSize.x * 0.5f + flySize.x * 0.5f;
        float maxX = areaSize.x * 0.5f - flySize.x * 0.5f;
        float minY = -areaSize.y * 0.5f + flySize.y * 0.5f;
        float maxY = areaSize.y * 0.5f - flySize.y * 0.5f;

        basePosition.x = Mathf.Clamp(basePosition.x, minX, maxX);
        basePosition.y = Mathf.Clamp(basePosition.y, minY, maxY);
    }
}