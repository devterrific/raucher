using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[DisallowMultipleComponent]
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
    private SpriteRenderer sr;

    private float xInput;
    private float stamina;
    private float regenCooldown;
    private float targetSpeed;

    private bool isSneaking;

    private Vector3 baseScale;

    // ====== Reason-based Locks / Hidden ======
    private readonly HashSet<object> movementLocks = new HashSet<object>();
    private readonly HashSet<object> hiddenReasons = new HashSet<object>();

    private readonly object sneakToken = new object();  // interner Grund fürs Sneaken

    private Sprite standSpriteBeforeHide;

    public bool CanMove => movementLocks.Count == 0;
    public bool Detectable => hiddenReasons.Count == 0;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        baseScale = transform.localScale;

        stamina = maxStamina;
        targetSpeed = walkSpeed;

        // Initialer Layer korrekt setzen
        ApplyDetectableLayer();
    }

    void Update()
    {
        HandleInteract();

        ReadInput();

        // Sneak/ Sprint nur wenn Bewegung nicht gelockt ist
        if (CanMove)
        {
            HandleSneak();
            HandleSprintAndStamina();
        }
        else
        {
            // Input & Movement neutralisieren
            isSneaking = false;
            xInput = 0f;

            // Stamina nicht kaputt-ticken während Lock
            targetSpeed = walkSpeed;
        }

        HandleFlip();
    }

    void FixedUpdate()
    {
        if (!CanMove)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        MovePlayer();
    }

    void ReadInput()
    {
        xInput = Input.GetAxisRaw(horizontalAxis);
    }

    void HandleSneak()
    {
        bool wantsSneak = Input.GetKey(sneakKey);

        if (wantsSneak && !isSneaking)
        {
            isSneaking = true;
            SetHidden(sneakToken, true);
        }
        else if (!wantsSneak && isSneaking)
        {
            isSneaking = false;
            SetHidden(sneakToken, false);
        }

        if (isSneaking)
            targetSpeed = sneakSpeed;
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
        float targetVelX = xInput * targetSpeed;
        float velX = rb.velocity.x;

        float rate = Mathf.Abs(targetVelX) > Mathf.Abs(velX) ? accel : decel;
        float newVelX = Mathf.MoveTowards(velX, targetVelX, rate * Time.fixedDeltaTime);

        rb.velocity = new Vector2(newVelX, 0f);
    }

    void HandleFlip()
    {
        // NICHT scale hard resetten, sonst Riesen-Player
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
            if (interactable != null && interactable.CanInteract(this))
            {
                interactable.Interact(this);
                break;
            }
        }
    }

    // =========================
    // PUBLIC API (Facade)
    // =========================

    /// <summary>Lockt Bewegung (Reason-based). Quelle kann z.B. Hidezone, Cutscene, UI usw. sein.</summary>
    public void AddMovementLock(object source)
    {
        if (source == null) return;

        if (movementLocks.Add(source))
        {
            rb.velocity = Vector2.zero;
        }
    }

    public void RemoveMovementLock(object source)
    {
        if (source == null) return;
        movementLocks.Remove(source);
    }

    /// <summary>Setzt Hidden-Reason an/aus. Detectable wird daraus abgeleitet.</summary>
    public void SetHidden(object source, bool hidden)
    {
        if (source == null) return;

        bool changed = false;

        if (hidden)
            changed = hiddenReasons.Add(source);
        else
            changed = hiddenReasons.Remove(source);

        if (changed)
            ApplyDetectableLayer();
    }

    public bool IsHiddenBy(object source)
    {
        if (source == null) return false;
        return hiddenReasons.Contains(source);
    }

    /// <summary>Hidezone-Entry: Hidden + Movement-Lock + Sprite setzen.</summary>
    public void EnterHidezone(object source, Sprite hideSprite)
    {
        if (source == null) return;

        // Merke das aktuelle "Stand"-Sprite genau in dem Moment, bevor wir verstecken
        if (sr != null)
            standSpriteBeforeHide = sr.sprite;

        AddMovementLock(source);
        SetHidden(source, true);

        if (sr != null && hideSprite != null)
            sr.sprite = hideSprite;
    }

    /// <summary>Hidezone-Exit: Hidden aus + Movement-Lock weg + Sprite zurück.</summary>
    public void ExitHidezone(object source)
    {
        if (source == null) return;

        RemoveMovementLock(source);
        SetHidden(source, false);

        if (sr != null && standSpriteBeforeHide != null)
            sr.sprite = standSpriteBeforeHide;
    }

    // =========================
    // INTERNALS
    // =========================

    void ApplyDetectableLayer()
    {
        int layer = Detectable
            ? LayerMask.NameToLayer("Player")
            : LayerMask.NameToLayer("Hidden");

        if (layer < 0)
        {
            Debug.LogError("Layer fehlt! 'Player' und 'Hidden' in Project Settings > Tags and Layers anlegen.");
            return;
        }

        gameObject.layer = layer;
    }
}
