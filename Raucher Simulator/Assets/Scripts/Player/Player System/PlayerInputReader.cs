using UnityEngine;

[DisallowMultipleComponent]
public class PlayerInputReader : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode sneakKey = KeyCode.LeftControl;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private string horizontalAxis = "Horizontal";

    public float MoveX { get; private set; }
    public bool SprintHeld { get; private set; }
    public bool SneakHeld { get; private set; }
    public bool InteractPressed { get; private set; }

    public void ReadInput()
    {
        MoveX = Input.GetAxisRaw(horizontalAxis);
        SprintHeld = Input.GetKey(sprintKey);
        SneakHeld = Input.GetKey(sneakKey);
        InteractPressed = Input.GetKeyDown(interactKey);
    }
}
