using UnityEngine;

public sealed class LookAheadSystem
{
    private readonly float _lookAheadDistance;
    private readonly float _smoothTime;
    private readonly float _directionVelocityThreshold;

    private float _currentOffset;
    private float _offsetVelocity;
    private int _lastDirectionSign;

    public LookAheadSystem(float lookAheadDistance, float smoothTime, float directionVelocityThreshold)
    {
        _lookAheadDistance = lookAheadDistance;
        _smoothTime = smoothTime;
        _directionVelocityThreshold = directionVelocityThreshold;
    }

    public float GetOffsetX(float velocityX, float deltaTime)
    {
        float targetOffset = ResolveTargetOffset(velocityX);

        if (_smoothTime <= 0f)
        {
            _currentOffset = targetOffset;
            _offsetVelocity = 0f;
            return _currentOffset;
        }

        _currentOffset = Mathf.SmoothDamp(
            _currentOffset,
            targetOffset,
            ref _offsetVelocity,
            _smoothTime,
            Mathf.Infinity,
            deltaTime);

        return _currentOffset;
    }

    private float ResolveTargetOffset(float velocityX)
    {
        if (Mathf.Abs(velocityX) < _directionVelocityThreshold)
        {
            return _lastDirectionSign * _lookAheadDistance;
        }

        _lastDirectionSign = velocityX > 0f ? 1 : -1;
        return _lastDirectionSign * _lookAheadDistance;
    }
}