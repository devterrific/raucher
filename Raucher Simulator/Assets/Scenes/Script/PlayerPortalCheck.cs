using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerPortalCheck : MonoBehaviour
{
    [Header("CircleCast Settings")]
    [SerializeField] private Transform castPoint;
    [SerializeField] private float circleRadius = 0.5f;
    [SerializeField] private float castDistance = 5f;
    [SerializeField] private LayerMask portalPointLayer;

    private SpawnManager _spawnManager;

    private void Start()
    {
        _spawnManager = GameObject.FindGameObjectWithTag("SpawnManager").GetComponent<SpawnManager>();
    }

    private void Update()
    {
        CheckPortalPoint();
    }

    private void CheckPortalPoint()
    {
        if (castPoint == null)
        {
            Debug.LogWarning("Cast Point is not assigned!");
            return;
        }

        Vector2 direction = castPoint.right;

        RaycastHit2D hit = Physics2D.CircleCast(
            castPoint.position,
            circleRadius,
            direction,
            castDistance,
            portalPointLayer
        );

        if (hit.collider != null)
        {
            SpawnerPoints spawnerPoints = hit.collider.GetComponent<SpawnerPoints>();

            if (spawnerPoints != null && _spawnManager.PlayerReadyToGo)
            {
                _spawnManager.LoadSceneWithDelay(spawnerPoints.Index, spawnerPoints.delayTime);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("PortalPoint"))
        {
            _spawnManager.PlayerReadyToGo = true;
        }
    }

    private void OnDrawGizmos()
    {
        if (castPoint == null) return;

        Gizmos.color = Color.cyan;

        Vector3 start = castPoint.position;
        Vector3 end = start + castPoint.right * castDistance;

        Gizmos.DrawWireSphere(start, circleRadius);
        Gizmos.DrawLine(start, end);
        Gizmos.DrawWireSphere(end, circleRadius);
    }
}