using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerMovementLockController : MonoBehaviour
{
    private readonly HashSet<object> movementLocks = new HashSet<object>();

    public bool CanMove => movementLocks.Count == 0;

    public bool AddMovementLock(object source)
    {
        if (source == null)
            return false;

        bool added = movementLocks.Add(source);
        Debug.Log($"[MovementLocks] ADD: {source} | Count={movementLocks.Count}", this);
        return added;
    }

    public void RemoveMovementLock(object source)
    {
        if (source == null)
            return;

        movementLocks.Remove(source);
        Debug.Log($"[MovementLocks] REMOVE: {source} | Count={movementLocks.Count}", this);
    }

    public bool HasLock(object source)
    {
        return source != null && movementLocks.Contains(source);
    }
}