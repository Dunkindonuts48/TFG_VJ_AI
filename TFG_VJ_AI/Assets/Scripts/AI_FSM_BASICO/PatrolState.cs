using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolState : NPCState
{
    private readonly Transform[] waypoints;
    private int currentWP;
    private float speed = 40f;

    public PatrolState(NPCStateMachine sm, int startIndex = 0) : base(sm)
    {
        waypoints = stateMachine.GetPatrolPoints();
        currentWP = stateMachine.GetLastPatrolIndex();
    }

    public override void Enter()
    {
        Debug.Log("Entrando en Patrol (start index = " + currentWP + ")");
    }

    public override void Execute()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Transform target = waypoints[currentWP];
        stateMachine.NPCTransform.position = Vector3.MoveTowards(
            stateMachine.NPCTransform.position,
            target.position,
            speed * Time.deltaTime
        );

        if (Vector3.Distance(stateMachine.NPCTransform.position, target.position) < 0.1f)
        {
            currentWP = (currentWP + 1) % waypoints.Length;
            stateMachine.SetLastPatrolIndex(currentWP);
        }
    }

    public override void Exit()
    {
        Debug.Log("Saliendo de Patrol");
    }
    public int CurrentWaypointIndex => currentWP;
}
