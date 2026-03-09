using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[DisallowMultipleComponent]
public class PlayerMovementMotor : MonoBehaviour
{
    [Header("Movement Tuning")]
    [Min(0f)][SerializeField] private float accel = 30f;
    [Min(0f)][SerializeField] private float decel = 40f;

    private Rigidbody2D rb;

    public void Initialize(Rigidbody2D body)
    {
        rb = body;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    public void Execute(float xInput, float targetSpeed, bool canMove)
    {
        if (rb == null)
            return;

        if (!canMove)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        float targetVelX = xInput * targetSpeed;
        float velX = rb.velocity.x;

        float rate = Mathf.Abs(targetVelX) > Mathf.Abs(velX) ? accel : decel;
        float newVelX = Mathf.MoveTowards(velX, targetVelX, rate * Time.fixedDeltaTime);

        rb.velocity = new Vector2(newVelX, 0f);
    }

    public void StopImmediately()
    {
        if (rb != null)
            rb.velocity = Vector2.zero;
    }
}
