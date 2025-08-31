using System.Collections;
using System.Collections.Generic;
using TFG.AI.BT;
using UnityEngine;
using UnityEngine.AI;

public class PlanRunner : MonoBehaviour
{
    public Queue<GoapAction> currentPlan = new Queue<GoapAction>();

    public BTState TickPlan()
    {
        if (currentPlan == null || currentPlan.Count == 0) return BTState.Success;

        var a = currentPlan.Peek();
        a.Apply(null);
        var move = a as MoveToAction;
        if (move != null)
        {
            var agent = GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                if (agent.pathPending) return BTState.Running;
                if (agent.remainingDistance > agent.stoppingDistance) return BTState.Running;
                if (!agent.hasPath || agent.velocity.sqrMagnitude < 0.01f)
                {
                    currentPlan.Dequeue();
                    return currentPlan.Count == 0 ? BTState.Success : BTState.Running;
                }
            }
        }
        currentPlan.Dequeue();
        return currentPlan.Count == 0 ? BTState.Success : BTState.Running;
    }
}