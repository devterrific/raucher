using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerMovementLockController : MonoBehaviour
{
    private readonly HashSet<object> movementLocks = new HashSet<object>();

    public bool CanMove => movementLocks.Count == 0;

    public bool AddMovementLock(object source)
    {
        if (source == null) return false;
        return movementLocks.Add(source);
    }

    public bool RemoveMovementLock(object source)
    {
        if (source == null) return false;
        return movementLocks.Remove(source);
    }

    public bool HasLockFrom(object source)
    {
        if (source == null) return false;
        return movementLocks.Contains(source);
    }
}