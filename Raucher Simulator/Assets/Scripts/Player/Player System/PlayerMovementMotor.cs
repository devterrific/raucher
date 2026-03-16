using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[DisallowMultipleComponent]
public class PlayerMovementMotor : MonoBehaviour
{
    [Header("Movement Tuning")]
    [Min(0f)][SerializeField] private float accel = 30f;
    [Min(0f)][SerializeField] private float decel = 40f;

    [Header("Physics Setup")]
    [SerializeField] private RigidbodyType2D bodyType = RigidbodyType2D.Kinematic;
    [SerializeField] private RigidbodyInterpolation2D interpolation = RigidbodyInterpolation2D.Interpolate;
    [SerializeField] private CollisionDetectionMode2D collisionDetection = CollisionDetectionMode2D.Continuous;

    private Rigidbody2D rb;
    private float currentSpeedX;

    public void Initialize(Rigidbody2D body)
    {
        rb = body;
        if (rb == null)
            return;

        rb.bodyType = bodyType;
        rb.gravityScale = 0f;
        rb.interpolation = interpolation;
        rb.collisionDetectionMode = collisionDetection;
        rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;

        currentSpeedX = 0f;
    }

    public void Execute(float xInput, float targetSpeed, bool canMove)
    {
        if (rb == null)
            return;

        float desiredSpeedX = canMove ? xInput * targetSpeed : 0f;
        float rate = Mathf.Abs(desiredSpeedX) > Mathf.Abs(currentSpeedX) ? accel : decel;
        currentSpeedX = Mathf.MoveTowards(currentSpeedX, desiredSpeedX, rate * Time.fixedDeltaTime);

        Vector2 delta = new Vector2(currentSpeedX * Time.fixedDeltaTime, 0f);
        rb.MovePosition(rb.position + delta);
    }

    public void StopImmediately()
    {
        currentSpeedX = 0f;
    }
}
