using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public sealed class SideScrollerCameraController : MonoBehaviour
{
    private const string PlayerTag = "Player";
    private const string CameraBoundsTag = "CameraBounds";
    private const float DefaultRetryInterval = 0.25f;
    private const float DefaultCameraZOffset = -10f;

    [System.Serializable]
    private struct DeadZoneSettings
    {
        [Min(0.1f)]
        [Tooltip("Horizontale Breite der Dead Zone in Welt-Einheiten.")]
        public float width;
    }

    [System.Serializable]
    private struct FollowSettings
    {
        [Min(0f)]
        [Tooltip("Zeit (SmoothDamp) bis zur Follow-Zielposition. 0 bedeutet harte Bewegung.")]
        public float smoothTime;
    }

    [System.Serializable]
    private struct LookAheadSettings
    {
        [Min(0f)]
        [Tooltip("Maximaler horizontaler Vorausblick in Bewegungsrichtung.")]
        public float distance;

        [Min(0f)]
        [Tooltip("Geschwindigkeit, mit der der Look Ahead Offset zur Zielseite interpoliert.")]
        public float smoothTime;

        [Min(0f)]
        [Tooltip("Mindestgeschwindigkeit auf X, ab der Look Ahead als 'laufend' gilt.")]
        public float directionVelocityThreshold;
    }

    [System.Serializable]
    private struct ClampSettings
    {
        [Tooltip("Fixe Y-Position der Kamera.")]
        public float fixedY;
    }

    [System.Serializable]
    private struct CameraViewSettings
    {
        [Tooltip("Fester Z-Abstand der Kamera (z.B. -10 in 2D).")]
        public float cameraZOffset;

        [Min(0.01f)]
        [Tooltip("Orthografischer Zoom über Camera.orthographicSize.")]
        public float orthographicSize;

        [Min(0.01f)]
        [Tooltip("Optionales Minimum für spätere Zoom-Features.")]
        public float minOrthographicSize;

        [Min(0.01f)]
        [Tooltip("Optionales Maximum für spätere Zoom-Features.")]
        public float maxOrthographicSize;
    }

    [System.Serializable]
    private struct TargetSearchSettings
    {
        [Min(0.05f)]
        [Tooltip("Suchintervall für Player/CameraBounds per Tag, wenn aktuell kein gültiges Ziel existiert.")]
        public float retryInterval;
    }

    [Header("Dead Zone")]
    [SerializeField]
    private DeadZoneSettings deadZoneSettings;

    [Header("Follow")]
    [SerializeField]
    private FollowSettings followSettings;

    [Header("Look Ahead")]
    [SerializeField]
    private LookAheadSettings lookAheadSettings;

    [Header("Position")]
    [SerializeField]
    private ClampSettings clampSettings;

    [Header("Camera View")]
    [SerializeField]
    private CameraViewSettings cameraViewSettings;

    [Header("Tag Search")]
    [SerializeField]
    private TargetSearchSettings targetSearchSettings;

    private Camera _camera;
    private DeadZoneSystem _deadZoneSystem;
    private FollowSystem _followSystem;
    private LookAheadSystem _lookAheadSystem;
    private CameraClampSystem _cameraClampSystem;

    private Transform _playerTransform;
    private Rigidbody2D _playerRigidbody;

    private Transform _cameraBoundsTransform;
    private SpriteRenderer _cameraBoundsSpriteRenderer;
    private Collider2D _cameraBoundsCollider;

    private float _nextTargetSearchTime;
    private bool _loggedMissingBoundsRenderer;

    private Vector3 _baseAnchorPosition;
    private Vector3 _lastValidCameraPosition;
    private float _lastPlayerX;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        EnsureRuntimeDefaults();

        _deadZoneSystem = new DeadZoneSystem(deadZoneSettings.width);
        _followSystem = new FollowSystem(followSettings.smoothTime);
        _lookAheadSystem = new LookAheadSystem(
            lookAheadSettings.distance,
            lookAheadSettings.smoothTime,
            lookAheadSettings.directionVelocityThreshold);
        _cameraClampSystem = new CameraClampSystem(_camera);

        ApplyConfiguredCameraView();

        _baseAnchorPosition = new Vector3(transform.position.x, clampSettings.fixedY, cameraViewSettings.cameraZOffset);
        transform.position = BuildCameraPosition(_baseAnchorPosition.x);
        _lastValidCameraPosition = transform.position;

        _nextTargetSearchTime = Time.unscaledTime;
    }

    private void LateUpdate()
    {
        if (!EnsureTargets())
        {
            transform.position = _lastValidCameraPosition;
            return;
        }

        if (!TryGetCameraBounds(out Bounds bounds))
        {
            transform.position = _lastValidCameraPosition;
            return;
        }

        float playerX = _playerTransform.position.x;
        float velocityX = ResolvePlayerVelocityX(playerX);

        float desiredBaseX = _deadZoneSystem.ResolveBaseTargetX(_baseAnchorPosition.x, playerX);
        float followedX = _followSystem.GetSmoothedX(_baseAnchorPosition.x, desiredBaseX, Time.deltaTime);
        _baseAnchorPosition = new Vector3(followedX, clampSettings.fixedY, cameraViewSettings.cameraZOffset);

        float lookAheadOffsetX = _lookAheadSystem.GetOffsetX(velocityX, Time.deltaTime);
        float unclampedX = _baseAnchorPosition.x + lookAheadOffsetX;
        float clampedX = _cameraClampSystem.ClampX(unclampedX, bounds);

        if (!IsFinite(clampedX))
        {
            transform.position = _lastValidCameraPosition;
            return;
        }

        transform.position = BuildCameraPosition(clampedX);
        _lastValidCameraPosition = transform.position;
        _lastPlayerX = playerX;
    }

    private bool EnsureTargets()
    {
        if (_playerTransform != null && _cameraBoundsTransform != null)
        {
            return true;
        }

        if (Time.unscaledTime < _nextTargetSearchTime)
        {
            return false;
        }

        _nextTargetSearchTime = Time.unscaledTime + targetSearchSettings.retryInterval;

        if (_playerTransform == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag(PlayerTag);
            if (playerObject != null)
            {
                _playerTransform = playerObject.transform;
                _playerRigidbody = playerObject.GetComponent<Rigidbody2D>();
                _lastPlayerX = _playerTransform.position.x;
            }
        }

        if (_cameraBoundsTransform == null)
        {
            GameObject boundsObject = GameObject.FindGameObjectWithTag(CameraBoundsTag);
            if (boundsObject != null)
            {
                _cameraBoundsTransform = boundsObject.transform;
                _cameraBoundsSpriteRenderer = boundsObject.GetComponent<SpriteRenderer>();
                _cameraBoundsCollider = boundsObject.GetComponent<Collider2D>();
                _loggedMissingBoundsRenderer = false;
            }
        }

        return _playerTransform != null && _cameraBoundsTransform != null;
    }

    private bool TryGetCameraBounds(out Bounds bounds)
    {
        bounds = default;

        if (_cameraBoundsTransform == null)
        {
            return false;
        }

        if (_cameraBoundsSpriteRenderer == null && _cameraBoundsCollider == null)
        {
            _cameraBoundsSpriteRenderer = _cameraBoundsTransform.GetComponent<SpriteRenderer>();
            _cameraBoundsCollider = _cameraBoundsTransform.GetComponent<Collider2D>();
        }

        if (_cameraBoundsSpriteRenderer != null)
        {
            bounds = _cameraBoundsSpriteRenderer.bounds;
            return true;
        }

        if (_cameraBoundsCollider != null)
        {
            bounds = _cameraBoundsCollider.bounds;
            return true;
        }

        if (!_loggedMissingBoundsRenderer)
        {
            Debug.LogWarning(
                $"[{nameof(SideScrollerCameraController)}] Objekt mit Tag '{CameraBoundsTag}' hat weder SpriteRenderer noch Collider2D. Clamping wird übersprungen.",
                _cameraBoundsTransform);
            _loggedMissingBoundsRenderer = true;
        }

        return false;
    }

    private void EnsureRuntimeDefaults()
    {
        deadZoneSettings.width = Mathf.Max(0.1f, deadZoneSettings.width);
        followSettings.smoothTime = Mathf.Max(0f, followSettings.smoothTime);
        lookAheadSettings.distance = Mathf.Max(0f, lookAheadSettings.distance);
        lookAheadSettings.smoothTime = Mathf.Max(0f, lookAheadSettings.smoothTime);
        lookAheadSettings.directionVelocityThreshold = Mathf.Max(0f, lookAheadSettings.directionVelocityThreshold);
        targetSearchSettings.retryInterval = targetSearchSettings.retryInterval <= 0f
            ? DefaultRetryInterval
            : targetSearchSettings.retryInterval;

        if (cameraViewSettings.cameraZOffset >= 0f)
        {
            float currentZ = transform.position.z;
            cameraViewSettings.cameraZOffset = currentZ < 0f ? currentZ : DefaultCameraZOffset;
        }

        float fallbackOrthoSize = _camera != null ? Mathf.Max(0.01f, _camera.orthographicSize) : 5f;

        cameraViewSettings.minOrthographicSize = cameraViewSettings.minOrthographicSize <= 0f
            ? 1f
            : Mathf.Max(0.01f, cameraViewSettings.minOrthographicSize);

        cameraViewSettings.maxOrthographicSize = cameraViewSettings.maxOrthographicSize <= 0f
            ? Mathf.Max(50f, cameraViewSettings.minOrthographicSize)
            : Mathf.Max(cameraViewSettings.minOrthographicSize, cameraViewSettings.maxOrthographicSize);

        if (cameraViewSettings.orthographicSize <= 0f)
        {
            cameraViewSettings.orthographicSize = fallbackOrthoSize;
        }

        cameraViewSettings.orthographicSize = Mathf.Clamp(
            cameraViewSettings.orthographicSize,
            cameraViewSettings.minOrthographicSize,
            cameraViewSettings.maxOrthographicSize);
    }

    private Vector3 BuildCameraPosition(float x)
    {
        float safeX = IsFinite(x) ? x : _lastValidCameraPosition.x;
        float safeY = IsFinite(clampSettings.fixedY) ? clampSettings.fixedY : _lastValidCameraPosition.y;
        float safeZ = IsFinite(cameraViewSettings.cameraZOffset) && cameraViewSettings.cameraZOffset < 0f
            ? cameraViewSettings.cameraZOffset
            : DefaultCameraZOffset;

        return new Vector3(safeX, safeY, safeZ);
    }

    private void ApplyConfiguredCameraView()
    {
        _camera.orthographic = true;
        _camera.orthographicSize = Mathf.Clamp(
            cameraViewSettings.orthographicSize,
            cameraViewSettings.minOrthographicSize,
            cameraViewSettings.maxOrthographicSize);
    }

    private float ResolvePlayerVelocityX(float currentPlayerX)
    {
        if (_playerRigidbody != null)
        {
            return _playerRigidbody.velocity.x;
        }

        return (currentPlayerX - _lastPlayerX) / Mathf.Max(Time.deltaTime, 0.0001f);
    }

    private static bool IsFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }

    private void OnValidate()
    {
        deadZoneSettings.width = Mathf.Max(0.1f, deadZoneSettings.width);
        followSettings.smoothTime = Mathf.Max(0f, followSettings.smoothTime);
        lookAheadSettings.distance = Mathf.Max(0f, lookAheadSettings.distance);
        lookAheadSettings.smoothTime = Mathf.Max(0f, lookAheadSettings.smoothTime);
        lookAheadSettings.directionVelocityThreshold = Mathf.Max(0f, lookAheadSettings.directionVelocityThreshold);
        targetSearchSettings.retryInterval = Mathf.Max(0.05f, targetSearchSettings.retryInterval);

        if (cameraViewSettings.cameraZOffset >= 0f)
        {
            float currentZ = transform.position.z;
            cameraViewSettings.cameraZOffset = currentZ < 0f ? currentZ : DefaultCameraZOffset;
        }

        cameraViewSettings.minOrthographicSize = Mathf.Max(0.01f, cameraViewSettings.minOrthographicSize);
        cameraViewSettings.maxOrthographicSize = Mathf.Max(cameraViewSettings.minOrthographicSize, cameraViewSettings.maxOrthographicSize);
        cameraViewSettings.orthographicSize = Mathf.Clamp(
            Mathf.Max(0.01f, cameraViewSettings.orthographicSize),
            cameraViewSettings.minOrthographicSize,
            cameraViewSettings.maxOrthographicSize);

        if (_camera == null)
        {
            _camera = GetComponent<Camera>();
        }

        if (_camera != null)
        {
            ApplyConfiguredCameraView();
            Vector3 current = transform.position;
            transform.position = new Vector3(current.x, clampSettings.fixedY, cameraViewSettings.cameraZOffset);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        float centerX = Application.isPlaying ? _baseAnchorPosition.x : transform.position.x;
        float halfWidth = deadZoneSettings.width * 0.5f;

        Gizmos.color = new Color(0.1f, 0.8f, 1f, 0.45f);
        float cameraHeight = _camera != null ? _camera.orthographicSize * 2f : 10f;
        Gizmos.DrawWireCube(
            new Vector3(centerX, transform.position.y, transform.position.z),
            new Vector3(deadZoneSettings.width, cameraHeight, 0.1f));

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(
            new Vector3(centerX - halfWidth, transform.position.y - cameraHeight * 0.5f, 0f),
            new Vector3(centerX - halfWidth, transform.position.y + cameraHeight * 0.5f, 0f));
        Gizmos.DrawLine(
            new Vector3(centerX + halfWidth, transform.position.y - cameraHeight * 0.5f, 0f),
            new Vector3(centerX + halfWidth, transform.position.y + cameraHeight * 0.5f, 0f));
    }
#endif
}