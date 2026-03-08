using UnityEngine;

public sealed class CameraClampSystem
{
    private readonly Camera _camera;

    public CameraClampSystem(Camera camera)
    {
        _camera = camera;
    }

    public float ClampX(float targetX, Bounds worldBounds)
    {
        float halfWidth = _camera.orthographicSize * _camera.aspect;
        float minCameraX = worldBounds.min.x + halfWidth;
        float maxCameraX = worldBounds.max.x - halfWidth;

        if (minCameraX > maxCameraX)
        {
            // Bounds sind schmaler als das Sichtfeld: stabil in der Mitte bleiben.
            return worldBounds.center.x;
        }

        return Mathf.Clamp(targetX, minCameraX, maxCameraX);
    }
}