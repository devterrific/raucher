using UnityEngine;

public class FlyWorld : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float directionChangeInterval = 1.2f;
    [SerializeField] private float directionStrength = 0.6f;

    [Header("Jitter")]
    [SerializeField] private float jitterAmount = 0.05f;
    [SerializeField] private float jitterSpeed = 18f;

    [Header("Movement Area (World Space)")]
    [SerializeField] private Transform centerPoint;
    [SerializeField] private Vector2 areaSize = new Vector2(5f, 3f);

    [Header("Pulse")]
    [SerializeField] private float pulseAmount = 0.1f;
    [SerializeField] private float pulseSpeed = 6f;

    private Vector3 originalScale;

    private Vector2 currentDirection;
    private float directionTimer;
    private Vector3 basePosition;

    private void Start()
    {
        basePosition = transform.position;

        currentDirection = Random.insideUnitCircle.normalized;
        if (currentDirection == Vector2.zero)
        {
            currentDirection = Vector2.right;
        }

        directionTimer = directionChangeInterval;

        originalScale = transform.localScale;
    }

    private void Update()
    {
        UpdateDirection();
        MoveFly();
        KeepInsideArea();
        ApplyJitter();
        ApplyPulse();
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
        Vector3 movement = (Vector3)(currentDirection * moveSpeed * Time.deltaTime);
        basePosition += movement;
    }

    private void ApplyJitter()
    {
        float jitterX = Mathf.Sin(Time.time * jitterSpeed) * jitterAmount;
        float jitterY = Mathf.Cos(Time.time * jitterSpeed * 1.15f) * jitterAmount;

        Vector3 jitterOffset = new Vector3(jitterX, jitterY, 0f);
        transform.position = basePosition + jitterOffset;
    }

    private void KeepInsideArea()
    {
        if (centerPoint == null)
            return;

        Vector3 center = centerPoint.position;

        float minX = center.x - areaSize.x * 0.5f;
        float maxX = center.x + areaSize.x * 0.5f;
        float minY = center.y - areaSize.y * 0.5f;
        float maxY = center.y + areaSize.y * 0.5f;

        basePosition.x = Mathf.Clamp(basePosition.x, minX, maxX);
        basePosition.y = Mathf.Clamp(basePosition.y, minY, maxY);
    }

    private void OnDrawGizmos()
    {
        if (centerPoint == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(centerPoint.position, new Vector3(areaSize.x, areaSize.y, 0f));
    }
    private void ApplyPulse()
    {
        float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;

        Vector3 newScale = originalScale + Vector3.one * pulse;
        transform.localScale = newScale;
    }
}