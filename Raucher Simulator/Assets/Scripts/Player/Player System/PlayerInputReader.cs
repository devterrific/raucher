using UnityEngine;

[DisallowMultipleComponent]
public class PlayerInputReader : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private string horizontalAxis = "Horizontal";
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    public float MoveX { get; private set; }
    public bool SprintHeld { get; private set; }
    public bool SneakHeld { get; private set; }
    public bool InteractPressed { get; private set; }

    public void ReadInput()
    {
        if (IsGameplayInputBlocked())
        {
            ClearInput();
            return;
        }

        MoveX = Input.GetAxisRaw(horizontalAxis);
        SprintHeld = Input.GetKey(sprintKey);

        // Sneaken wird nicht mehr direkt über eine Taste ausgelöst.
        // Das passiert nur noch über Interaktion mit Objekten / Hidezones.
        SneakHeld = false;

        InteractPressed = Input.GetKeyDown(interactKey);
    }

    private bool IsGameplayInputBlocked()
    {
        if (PauseMenuManager.Instance != null && PauseMenuManager.Instance.IsPaused)
            return true;

        if (GameOverManager.Instance != null && GameOverManager.Instance.IsGameplayInputBlocked)
            return true;

        return false;
    }

    private void ClearInput()
    {
        MoveX = 0f;
        SprintHeld = false;
        SneakHeld = false;
        InteractPressed = false;
    }
}