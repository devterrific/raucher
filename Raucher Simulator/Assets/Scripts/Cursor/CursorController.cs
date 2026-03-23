using UnityEngine;

public class CursorController : MonoBehaviour
{
    [Header("Cursor Textures")]
    [SerializeField] private Texture2D normalCursor;
    [SerializeField] private Texture2D pressedCursor;

    [Header("Cursor Settings")]
    [SerializeField] private Vector2 hotspot = Vector2.zero;
    [SerializeField] private CursorMode cursorMode = CursorMode.Auto;

    private static CursorController instance;

    private void Awake()
    {
        // Singleton-Schutz
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SetNormalCursor();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SetPressedCursor();
        }

        if (Input.GetMouseButtonUp(0))
        {
            SetNormalCursor();
        }
    }

    private void SetNormalCursor()
    {
        Cursor.SetCursor(normalCursor, hotspot, cursorMode);
    }

    private void SetPressedCursor()
    {
        Cursor.SetCursor(pressedCursor, hotspot, cursorMode);
    }
}