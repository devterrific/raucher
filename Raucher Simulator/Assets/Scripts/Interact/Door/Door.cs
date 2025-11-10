using System;

public class Door : Interactable
{
    public override void Interact(PlayerMain player)
    {
        Console.WriteLine("Tür öffnet sich!");
        // Hier kommt dein eigenes Verhalten rein:
        // z. B. animator.SetTrigger("Open");
        // oder: GetComponent<BoxCollider2D>().enabled = false;
    }
}
