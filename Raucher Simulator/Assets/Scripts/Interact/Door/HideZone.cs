using UnityEngine;

public class Hidezone : Interactable
{
    public override void Interact(PlayerMain player)
    {
        player.Detectable = !player.Detectable;

        if (!player.Detectable)
            Debug.Log("Player ist jetzt nicht mehr sichtbar");
        else
            Debug.Log("Player ist wieder sichtbar");
    }
}
