using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMain : MonoBehaviour
{
    [Header("Movement")]
    [Min(0f)] public float walkSpeed = 3.5f;
    [Min(0f)] public float sprintSpeed = 5.5f;
    [Min(0f)] public float accel = 30f;
    [Min(0f)] public float decel = 40f;

    [Header("Stamina")]
    [Min(0f)] public float maxStamina = 100f;
    [Min(0f)] public float drainPerSec = 20f;
    [Min(0f)] public float regenPerSec = 12f;
    [Min(0f)] public float minSprintThreshold = 5f;
    [Min(0f)] public float regenDelay = 0.6f;

    [Header("Interaction")]
    [Tooltip("Wie weit der Spieler Objekte ansprechen kann.")]
    [SerializeField] private float interactRange = 1.5f;
    [Tooltip("Welche Layer als Interactable gelten.")]
    [SerializeField] private LayerMask interactLayer;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Input")]
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private string horizontalAxis = "Horizontal";

    [Header("Flip")]
    public bool flipByScale = true;

    private Rigidbody2D rb;
    private float xInput;
    private float stamina;
    private float regenCooldown;
    private float targetSpeed;
    private bool canMove = true;
    public bool CanMove => canMove;


    private bool detectable;
    public bool Detectable
    {
        get { return detectable; }
        set { detectable = value; }
    }


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        stamina = maxStamina;
        targetSpeed = walkSpeed;
        Detectable = true;
    }

    void Update()
    {
        HandleInteract();

        if (!Detectable)
        {
            xInput = 0f;
            regenCooldown = regenDelay;
            return;
        }

        ReadInput();
        HandleSprintAndStamina();
        HandleFlip();
    }



    void FixedUpdate()
    {
        MovePlayer();
    }

    void ReadInput()
    {
        xInput = Input.GetAxisRaw(horizontalAxis);
    }

    void HandleSprintAndStamina()
    {
        bool wantsSprint = Input.GetKey(sprintKey) && Mathf.Abs(xInput) > 0.01f;
        bool canSprint = stamina > minSprintThreshold;

        if (wantsSprint && canSprint)
        {
            targetSpeed = sprintSpeed;
            stamina = Mathf.Max(0f, stamina - drainPerSec * Time.deltaTime);
            regenCooldown = regenDelay;
        }
        else
        {
            targetSpeed = walkSpeed;

            if (regenCooldown > 0f)
                regenCooldown -= Time.deltaTime;
            else
                stamina = Mathf.Min(maxStamina, stamina + regenPerSec * Time.deltaTime);
        }
    }

    void HandleFlip()
    {
        if (!flipByScale) return;

        if (xInput > 0.01f)
        {
            var s = transform.localScale;
            s.x = Mathf.Abs(s.x);
            transform.localScale = s;
        }
        else if (xInput < -0.01f)
        {
            var s = transform.localScale;
            s.x = -Mathf.Abs(s.x);
            transform.localScale = s;
        }
    }

    void MovePlayer()
    {
        if (!Detectable)
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


    void HandleInteract()
    {
        if (Input.GetKeyDown(interactKey))
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactRange, interactLayer);
            Debug.Log("Hits: " + hits.Length);

            foreach (var hit in hits)
            {
                Debug.Log("Hit: " + hit.name + " Layer: " + LayerMask.LayerToName(hit.gameObject.layer));

                Interactable interactable = hit.GetComponent<Interactable>();
                if (interactable != null)
                {
                    interactable.Interact(this);
                    break;
                }
            }
        }
    }



}
