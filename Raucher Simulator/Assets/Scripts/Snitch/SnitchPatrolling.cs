using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnitchPatrolling : MonoBehaviour
{
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private int targetPoint;
    [SerializeField] private float speed;

    private void Start()
    {
        targetPoint = 0;
    }

    private void Update()
    {
        MoveSnitch(speed);
    }

    private void MoveSnitch(float speed)
    {
        if (transform.position == patrolPoints[targetPoint].position)
        {
            increaseTargetInt();
        }  

        transform.position = Vector2.MoveTowards(transform.position, patrolPoints[targetPoint].position, speed * Time.deltaTime);
    }

    private void 

    private void increaseTargetInt()
    {
        targetPoint++;
        if (targetPoint >= patrolPoints.Length)
            targetPoint = 0;
    }
}
