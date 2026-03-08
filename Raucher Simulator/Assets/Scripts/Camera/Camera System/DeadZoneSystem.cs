public sealed class DeadZoneSystem
{
    private readonly float _halfWidth;

    public DeadZoneSystem(float width)
    {
        _halfWidth = width * 0.5f;
    }

    public float ResolveBaseTargetX(float currentAnchorX, float playerX)
    {
        float leftBound = currentAnchorX - _halfWidth;
        float rightBound = currentAnchorX + _halfWidth;

        if (playerX < leftBound)
        {
            return playerX + _halfWidth;
        }

        if (playerX > rightBound)
        {
            return playerX - _halfWidth;
        }

        return currentAnchorX;
    }
}