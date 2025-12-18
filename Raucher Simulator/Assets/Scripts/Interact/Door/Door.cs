using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Door : Interactable
{
    public string targetSceneName;

    public override void Interact(PlayerMain player)
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("Door: Keine Ziel-Scene gesetzt!");
            return;
        }

        SceneManager.LoadScene(targetSceneName);
    }
}

