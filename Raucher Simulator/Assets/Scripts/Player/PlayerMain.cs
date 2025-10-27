using UnityEngine;

public class PlayerMain : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3.5f;
    public float sprintSpeed = 5.5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private float currentSpeed;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f; // kein Fallen
        rb.freezeRotation = true; // dreht sich nicht
    }

    void Update()
    {
        // Eingaben holen
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        moveInput = new Vector2(x, y).normalized;

        // Sprint Taste
        if (Input.GetKey(KeyCode.LeftShift))
            currentSpeed = sprintSpeed;
        else
            currentSpeed = moveSpeed;
    }

    void FixedUpdate()
    {
        // Bewegung anwenden
        rb.velocity = moveInput * currentSpeed;
    }
}
