using UnityEngine;

[DisallowMultipleComponent]
public class CameraFollow2D_Tagged : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private string targetTag = "snitch";

    [Header("Follow")]
    [Tooltip("Offset in WORLD space. z wird ignoriert, weil die Kamera ihren eigenen z-Abstand behält.")]
    [SerializeField] private Vector2 offset = new Vector2(0f, 0f);

    [Tooltip("Smoothing. 0 = instant, higher = smoother.")]
    [Range(0f, 30f)]
    [SerializeField] private float smooth = 12f;

    [Header("Axis Locks")]
    [SerializeField] private bool followX = true;
    [SerializeField] private bool followY = true;

    private Transform target;
    private float camZ;

    private void Awake()
    {
        camZ = transform.position.z;
        FindTarget();
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            FindTarget();
            if (target == null) return;
        }

        Vector3 current = transform.position;
        Vector3 desired = current;

        if (followX) desired.x = target.position.x + offset.x;
        if (followY) desired.y = target.position.y + offset.y;

        desired.z = camZ;

        if (smooth <= 0f)
            transform.position = desired;
        else
            transform.position = Vector3.Lerp(current, desired, smooth * Time.deltaTime);
    }

    private void FindTarget()
    {
        GameObject go = GameObject.FindGameObjectWithTag(targetTag);
        target = go != null ? go.transform : null;
    }
}