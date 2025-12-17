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

    //public vars...
    [HideInInspector] public bool canMove = true;

    // private vars...
    private bool isWaiting = false;

    private void Start()
    {
        targetPoint = 0;
    }

    private void Update()
    {
        if (!isWaiting && canMove)
            MoveSnitch(speed);
    }

    private void MoveSnitch(float speed)
    {
        FaceDirection();

        if (Vector2.Distance(transform.position, patrolPoints[targetPoint].position) < .05f)
            StartCoroutine(WaitAtPoint(waitTime));

        transform.position = Vector2.MoveTowards(transform.position,
                                                patrolPoints[targetPoint].position,
                                                speed * Time.deltaTime);
    }

    private void FaceDirection()
    {
        float direction = patrolPoints[targetPoint].position.x - transform.position.x;

        if (direction > 0)
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        else if (direction < 0)
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
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
