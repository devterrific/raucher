using UnityEngine;

public class PlayerMain : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 3.5f;
    public float sprintSpeed = 5.5f;

    [Header("Stamina")]
    public float maxStamina = 100f;        // volle Ausdauer
    public float drainPerSec = 20f;        // wie schnell Ausdauer beim Sprinten runtergeht
    public float regenPerSec = 12f;        // wie schnell Ausdauer wieder hochgeht

    private Rigidbody2D rb;
    private float xInput;
    private float currentSpeed;
    private float stamina;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;      // kein Jump → keine Schwerkraft
        rb.freezeRotation = true;

        stamina = maxStamina;      // Start voll
        currentSpeed = walkSpeed;  // Start langsam
    }

    void Update()
    {
        ReadInput();
        HandleSprintAndStamina();   // sehr simple Logik
        HandleFlip();               // nach links/rechts schauen
    }

    void FixedUpdate()
    {
        MovePlayer();               // Movement ist jetzt eigene Methode
    }

    // --- Eingaben nur horizontal (A/D oder Pfeile) ---
    private void ReadInput()
    {
        xInput = Input.GetAxisRaw("Horizontal"); // -1, 0 oder 1
    }

    // --- Sprint + Ausdauer ---
    private void HandleSprintAndStamina()
    {
        bool wantsSprint = Input.GetKey(KeyCode.LeftShift) && Mathf.Abs(xInput) > 0.01f;

        if (wantsSprint && stamina > 0f)
        {
            currentSpeed = sprintSpeed;
            stamina -= drainPerSec * Time.deltaTime;
            if (stamina < 0f) stamina = 0f; // clamp
        }
        else
        {
            currentSpeed = walkSpeed;
            stamina += regenPerSec * Time.deltaTime;
            if (stamina > maxStamina) stamina = maxStamina; // clamp
        }
    }

    // --- Blickrichtung drehen, je nach Input ---
    private void HandleFlip()
    {
        if (xInput > 0.01f)
        {
            Vector3 s = transform.localScale;
            s.x = Mathf.Abs(s.x);
            transform.localScale = s;
        }
        else if (xInput < -0.01f)
        {
            Vector3 s = transform.localScale;
            s.x = -Mathf.Abs(s.x);
            transform.localScale = s;
        }
    }

    // --- Bewegung anwenden (eigene Methode) ---
    private void MovePlayer()
    {
        rb.velocity = new Vector2(xInput * currentSpeed, 0f);
    }

    // optional: public Getter für UI-Anzeige (Stamina-Bar)
    public float Stamina01()
    {
        return maxStamina <= 0f ? 0f : stamina / maxStamina;
    }
}
