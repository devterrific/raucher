using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerVisibility : MonoBehaviour
{
    private readonly HashSet<object> hiddenReasons = new HashSet<object>();

    public bool Detectable => hiddenReasons.Count == 0;

    public void Initialize()
    {
        ApplyDetectableLayer();
    }

    public void SetHidden(object source, bool hidden)
    {
        if (source == null) return;

        bool changed = hidden ? hiddenReasons.Add(source) : hiddenReasons.Remove(source);
        if (changed)
            ApplyDetectableLayer();
    }

    public bool IsHiddenBy(object source)
    {
        if (source == null) return false;
        return hiddenReasons.Contains(source);
    }

    private void ApplyDetectableLayer()
    {
        int layer = Detectable
            ? LayerMask.NameToLayer("Player")
            : LayerMask.NameToLayer("Hidden");

        if (layer < 0)
        {
            Debug.LogError("Layer fehlt! 'Player' und 'Hidden' in Project Settings > Tags and Layers anlegen.");
            return;
        }

        gameObject.layer = layer;
    }
}