using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnitchVision : MonoBehaviour
{
    [Header("Raycast Points")]
    [SerializeField] private Transform eyePoint;
    [SerializeField] private Transform suspicionPoint;

    [Header("Raycast Settings")]
    [SerializeField] private float viewDistance = 5f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Suspicion Settings")]
    [SerializeField] private float timeToCatch = 3f;

    // private vars...
    private float suspicionTimer = 0f;
    private SnitchPatrolling snitchPatrolling;

    private void Start()
    {
        snitchPatrolling = GetComponent<SnitchPatrolling>();
    }

    private void Update()
    {
        bool instantHit = InstantRaycast();
        bool suspicionHit = TimedRaycast();

        snitchPatrolling.canMove = !(instantHit || suspicionHit);
    }

    private bool InstantRaycast()
    {
        RaycastHit2D hit = Physics2D.Raycast(eyePoint.position,
                                            transform.right,
                                            viewDistance,
                                            playerLayer);

        if (hit.collider != null)
        {
            Debug.Log("Player got caught!");
            return true;
        } 

        return false;
    }

    private bool TimedRaycast()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            suspicionPoint.position,
            transform.right,
            viewDistance,
            playerLayer
        );

        if (hit.collider != null)
        {
            suspicionTimer += Time.deltaTime;

            if (suspicionTimer >= timeToCatch)
            {
                Debug.Log("Player got caught after 3 seconds!");
                return true;
            }

            return true;
        }
        else
        {
            suspicionTimer = 0f;
            return false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (eyePoint == null || suspicionPoint == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(
            eyePoint.position,
            eyePoint.position + transform.right * viewDistance
        );

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(
            suspicionPoint.position,
            suspicionPoint.position + transform.right * viewDistance
        );
    }

}
