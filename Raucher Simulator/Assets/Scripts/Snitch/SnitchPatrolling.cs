using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnitchPatrolling : MonoBehaviour
{
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private int targetPoint;
    [SerializeField] private float speed;
    [SerializeField] private float waitTime = 2.2f;

    // private vars...
    private bool isWaiting = false;

    private void Start()
    {
        targetPoint = 0;
    }

    private void Update()
    {
        if (!isWaiting)
            MoveSnitch(speed);
    }

    private void MoveSnitch(float speed)
    {
        if (Vector2.Distance(transform.position, patrolPoints[targetPoint].position) < .05f)
            StartCoroutine(WaitAtPoint(waitTime));

        transform.position = Vector2.MoveTowards(transform.position,
                                                patrolPoints[targetPoint].position,
                                                speed * Time.deltaTime);
    }

    private IEnumerator WaitAtPoint(float time)
    {
        isWaiting = true;
        yield return new WaitForSeconds(time);
        IncreaseTargetInt();
        isWaiting = false;
    }

    private void IncreaseTargetInt()
    {
        targetPoint++;
        if (targetPoint >= patrolPoints.Length)
            targetPoint = 0;
    }
}
