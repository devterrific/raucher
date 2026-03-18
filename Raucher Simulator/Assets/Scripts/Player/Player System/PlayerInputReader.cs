using UnityEngine;

[DisallowMultipleComponent]
public class PlayerInputReader : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private string horizontalAxis = "Horizontal";
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Crouch")]
    [SerializeField] private KeyCode crouchKeyPrimary = KeyCode.LeftControl;
    [SerializeField] private KeyCode crouchKeySecondary = KeyCode.RightControl;
    [SerializeField] private KeyCode crouchKeyFallback = KeyCode.C;

    public float MoveX { get; private set; }
    public bool SprintHeld { get; private set; }
    public bool SneakHeld { get; private set; }
    public bool InteractPressed { get; private set; }

    public void ReadInput()
    {
        MoveX = Input.GetAxisRaw(horizontalAxis);
        SprintHeld = Input.GetKey(sprintKey);

        SneakHeld =
            Input.GetKey(crouchKeyPrimary) ||
            Input.GetKey(crouchKeySecondary) ||
            Input.GetKey(crouchKeyFallback);

        InteractPressed = Input.GetKeyDown(interactKey);
    }
}