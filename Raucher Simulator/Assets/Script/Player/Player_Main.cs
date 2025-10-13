using System;
using UnityEngine;

public class Player_Main : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Hier kommt rein wie schnell der Player am ende sein wird.")]
    [SerializeField] private int movementspeed = 1;
    [SerializeField] private int sprintspeed = 5;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log(movementspeed + " UND " + sprintspeed); // Diese Zeile ist dafuer da damit der Editor nicht nervt weil wir die Variablen nicht benutzen
    }

    // Update is called once per frame
    void Update()
    {
        // Hier soll bitte aufgeräumt sein ~ Das bedeuted nur Methoden
        

    }

    private void Movement()
    {
        // Hier kommt das Movement hin

    }
}
