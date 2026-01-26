using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMain : MonoBehaviour
{
    [Header("Movement")]
    [Min(0f)] public float walkSpeed = 3.5f;
    [Min(0f)] public float sprintSpeed = 5.5f;
    [Min(0f)] public float sneakSpeed = 1.8f;
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
    [SerializeField] private KeyCode sneakKey = KeyCode.LeftControl;
    [SerializeField] private string horizontalAxis = "Horizontal";

    private Rigidbody2D rb;

    private float xInput;
    private float stamina;
    private float regenCooldown;
    private float targetSpeed;

    private bool isSneaking;
    private bool detectable = true;

    private Vector3 baseScale;

    public bool Detectable => detectable;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        baseScale = transform.localScale;

        stamina = maxStamina;
        targetSpeed = walkSpeed;

        SetDetectable(true);
    }

    void Update()
    {
        HandleInteract();

        ReadInput();
        HandleSneak();
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

    void HandleSneak()
    {
        isSneaking = Input.GetKey(sneakKey);

        if (isSneaking)
        {
            // Sneak = unsichtbar für Snitch (Layer Switch)
            SetDetectable(false);
            targetSpeed = sneakSpeed;
        }
        else
        {
            // außerhalb Sneak wieder normal sichtbar (HideZone kann das trotzdem überschreiben)
            // -> Hier NICHT stumpf true setzen, sonst killst du HideZone.
            // Darum: nur dann auf true, wenn du aktuell "nicht versteckt" bist.
            // (HideZone regelt "versteckt" über SetDetectableExternal)
        }
    }

    void HandleSprintAndStamina()
    {
        if (isSneaking)
            return;

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

    void MovePlayer()
    {
        // Wenn Sneak aktiv: targetSpeed wurde schon auf sneakSpeed gesetzt.
        // Wenn nicht: targetSpeed kommt aus Sprint/Walk.
        float targetVelX = xInput * targetSpeed;
        float velX = rb.velocity.x;

        float rate = Mathf.Abs(targetVelX) > Mathf.Abs(velX) ? accel : decel;
        float newVelX = Mathf.MoveTowards(velX, targetVelX, rate * Time.fixedDeltaTime);

        rb.velocity = new Vector2(newVelX, 0f);
    }

    void HandleFlip()
    {
        // NICHT scale auf (1,1,1) setzen -> sonst Riesen-Player
        if (xInput > 0.01f)
        {
            var s = baseScale;
            s.x = Mathf.Abs(s.x);
            transform.localScale = s;
        }
        else if (xInput < -0.01f)
        {
            var s = baseScale;
            s.x = -Mathf.Abs(s.x);
            transform.localScale = s;
        }
    }

    void HandleInteract()
    {
        if (!Input.GetKeyDown(interactKey))
            return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactRange, interactLayer);

        for (int i = 0; i < hits.Length; i++)
        {
            Interactable interactable = hits[i].GetComponent<Interactable>();
            if (interactable != null)
            {
                interactable.Interact(this);
                break;
            }
        }
    }

    // ========= Detectable API =========

    void SetDetectable(bool value)
    {
        if (detectable == value) return;

        detectable = value;

        int layer = value
            ? LayerMask.NameToLayer("Player")
            : LayerMask.NameToLayer("Hidden");

        if (layer < 0)
        {
            Debug.LogError("Layer fehlt! Player/Hidden in Project Settings prüfen.");
            return;
        }

        gameObject.layer = layer;
    }



    // Für HideZone & andere Interactables
    public void SetDetectableExternal(bool value)
    {
        SetDetectable(value);
    }

    // Optional: wenn du willst, dass Sneak beim Loslassen wieder sichtbar macht,
    // ABER HideZone nicht kaputt geht, brauchst du einen "hiddenByZone"-State.
    // Den machen wir, wenn du’s verlangst. (Aktuell regelt HideZone den Zustand.)
}
