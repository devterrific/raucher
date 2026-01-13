using UnityEngine;

[CreateAssetMenu(menuName = "SmokingMiniGame/DifficultySettings")]
public class DifficultySettings : ScriptableObject
{
    [Header("Filter Movement")]
    [Min(1f)] public float filterSpeed = 500f;      // px/sec
    public bool loopMovement = true;                // falls man später Ping-Pong willst
    public bool rightToLeft = true;                 // gemäß Design: rechts → links

    [Min(1f)] public float movementRange = 800F;    // Range für die Filterbewegung

    [Header("Scoring (points)")]
    public int pointsPerfect = 30;                  // JACKPOTT
    public int pointsGood = 20;
    public int pointsOkay = 10;
    public int pointsMiss = 0;

    [Header("Timing")]
    public float countdownSeconds = 3f;
    public float assembleSeconds = 0.6f;            // kleine Abschluss-Anim
}
