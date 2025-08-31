using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class MoveToAction : GoapAction
{
    [Header("Destino")]
    public Transform target;

    [Header("Goal que satisfago en la simulación")]
    public string goalFlagToSatisfy = "goalFollow";

    NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        actionName = "MoveTo";
    }

    public override bool CanRun(WorldState s)
    {
        return target != null && agent != null;
    }

    public override void Apply(WorldState s)
    {
        if (target && agent) agent.SetDestination(target.position);

        if (!string.IsNullOrEmpty(goalFlagToSatisfy))
            s.Set(goalFlagToSatisfy, true);
    }
}
