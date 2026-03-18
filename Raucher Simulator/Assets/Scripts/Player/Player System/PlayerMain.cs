using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInputReader))]
[RequireComponent(typeof(PlayerMovementMotor))]
[RequireComponent(typeof(PlayerMovementState))]
[RequireComponent(typeof(PlayerStamina))]
[RequireComponent(typeof(PlayerMovementLockController))]
[RequireComponent(typeof(PlayerVisibility))]
[RequireComponent(typeof(PlayerInteraction))]
[RequireComponent(typeof(PlayerVisuals))]
[RequireComponent(typeof(PlayerRespawn))]
[DisallowMultipleComponent]
public class PlayerMain : MonoBehaviour
{
    private readonly object sneakToken = new object();

    [Header("Visual References")]
    [SerializeField] private SpriteRenderer playerSpriteRenderer;
    [SerializeField] private Animator playerAnimator;

    private Rigidbody2D rb;

    private PlayerInputReader inputReader;
    private PlayerMovementMotor movementMotor;
    private PlayerMovementState movementState;
    private PlayerStamina stamina;
    private PlayerMovementLockController movementLocks;
    private PlayerVisibility visibility;
    private PlayerInteraction interaction;
    private PlayerVisuals visuals;
    private PlayerRespawn respawn;

    public bool CanMove => movementLocks != null && movementLocks.CanMove;
    public bool Detectable => visibility != null && visibility.Detectable;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (playerSpriteRenderer == null)
            playerSpriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (playerAnimator == null)
            playerAnimator = GetComponentInChildren<Animator>();

        inputReader = GetComponent<PlayerInputReader>();
        movementMotor = GetComponent<PlayerMovementMotor>();
        movementState = GetComponent<PlayerMovementState>();
        stamina = GetComponent<PlayerStamina>();
        movementLocks = GetComponent<PlayerMovementLockController>();
        visibility = GetComponent<PlayerVisibility>();
        interaction = GetComponent<PlayerInteraction>();
        visuals = GetComponent<PlayerVisuals>();
        respawn = GetComponent<PlayerRespawn>();

        movementMotor.Initialize(rb);
        stamina.Initialize();
        visibility.Initialize();
        visuals.Initialize(playerSpriteRenderer, playerAnimator);

        Debug.Log($"[PlayerMain] Animator: {(playerAnimator != null ? playerAnimator.name : "NULL")}", this);
    }

    private void Update()
    {
        inputReader.ReadInput();
        if (Input.GetKeyDown(KeyCode.LeftControl) ||
    Input.GetKeyDown(KeyCode.RightControl) ||
    Input.GetKeyDown(KeyCode.C) ||
    Input.GetKeyUp(KeyCode.LeftControl) ||
    Input.GetKeyUp(KeyCode.RightControl) ||
    Input.GetKeyUp(KeyCode.C))
        {
            Debug.Log($"SneakHeld={inputReader.SneakHeld}");
        }
        if (inputReader.SneakHeld)
        {
            Debug.Log($"CurrentMode={movementState.CurrentMode}");
        }
        interaction.TryInteract(this, inputReader.InteractPressed);

        bool canMove = movementLocks.CanMove;
        bool isMoving = Mathf.Abs(inputReader.MoveX) > 0.01f;
        bool sprintRequested = canMove && !inputReader.SneakHeld && inputReader.SprintHeld && isMoving;
        bool sprintActive = stamina.TickSprint(sprintRequested);

        movementState.Resolve(canMove, inputReader.MoveX, inputReader.SneakHeld, sprintActive);
        visibility.SetHidden(sneakToken, movementState.IsSneaking);

        visuals.UpdateVisuals(inputReader.MoveX, movementState.CurrentMode, inputReader.SneakHeld);
    }

    private void FixedUpdate()
    {
        float xInput = movementLocks.CanMove ? inputReader.MoveX : 0f;
        movementMotor.Execute(xInput, movementState.TargetSpeed, movementLocks.CanMove);
    }

    public void Respawn(Vector3 spawnPosition)
    {
        respawn.Respawn(rb, transform, spawnPosition);
    }

    public void AddMovementLock(object source)
    {
        Debug.Log($"[PlayerMain] AddMovementLock from: {source}", this);

        if (movementLocks.AddMovementLock(source))
            movementMotor.StopImmediately();
    }

    public void RemoveMovementLock(object source)
    {
        Debug.Log($"[PlayerMain] RemoveMovementLock from: {source}", this);
        movementLocks.RemoveMovementLock(source);
    }

    public void SetHidden(object source, bool hidden)
    {
        visibility.SetHidden(source, hidden);
    }

    public bool IsHiddenBy(object source)
    {
        return visibility.IsHiddenBy(source);
    }

    public void EnterHidezone(object source, Sprite hideSprite)
    {
        if (source == null) return;

        visuals.EnterHidezone(hideSprite);
        AddMovementLock(source);
        SetHidden(source, true);
    }

    public void ExitHidezone(object source)
    {
        if (source == null) return;

        RemoveMovementLock(source);
        SetHidden(source, false);
        visuals.ExitHidezone();
    }
}