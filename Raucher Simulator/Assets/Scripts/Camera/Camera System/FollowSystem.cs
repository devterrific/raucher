using UnityEngine;

public sealed class FollowSystem
{
    private readonly float _smoothTime;
    private float _velocity;

    public FollowSystem(float smoothTime)
    {
        _smoothTime = smoothTime;
    }

    public float GetSmoothedX(float currentX, float targetX, float deltaTime)
    {
        if (_smoothTime <= 0f)
        {
            _velocity = 0f;
            return targetX;
        }

        return Mathf.SmoothDamp(currentX, targetX, ref _velocity, _smoothTime, Mathf.Infinity, deltaTime);
    }
}