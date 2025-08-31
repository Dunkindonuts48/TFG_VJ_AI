using System.Collections.Generic;
using System.Linq;
using TFG.AI.BT;
using UnityEngine;

public class HybridOrchestrator : MonoBehaviour
{
    public PlannerGreedy goap;
    public PlanRunner btRunner;
    public WorldState world = new WorldState();
    public Dictionary<string, float> goalUtilities = new Dictionary<string, float>();

    [Tooltip("Si lo dejas vacío, se autollenará con todos los GoapAction del mismo GameObject")]
    public List<GoapAction> actions = new List<GoapAction>();

    void Awake()
    {
        if (actions == null || actions.Count == 0)
        {
            actions = GetComponents<GoapAction>().ToList();
            Debug.Log($"[Orchestrator] Acciones autodescubiertas: {string.Join(", ", actions.Select(a => a.actionName))}");
        }
        if (!goap) goap = GetComponent<PlannerGreedy>();
        if (!btRunner) btRunner = GetComponent<PlanRunner>();
    }

    public void ApplyUtilityDelta(string goal, float delta)
    {
        if (!goalUtilities.ContainsKey(goal)) goalUtilities[goal] = 0f;
        goalUtilities[goal] = Mathf.Clamp(goalUtilities[goal] + delta, -10f, 10f);
    }

    public void RouteIntent(string intent, Dictionary<string, string> slots)
    {
        switch (intent)
        {
            case "FollowPlayer":
                world.Set("goalFollow", true);
                Replan("goalFollow");
                break;

            case "Trade":
                world.Set("goalTrade", true);
                Replan("goalTrade");
                break;

            default:
                break;
        }
    }

    public void Replan(string goalFlag)
    {
        if (goap == null || btRunner == null)
        {
            Debug.LogWarning("[Orchestrator] goap o btRunner no asignados");
            return;
        }
        if (actions == null || actions.Count == 0)
        {
            Debug.LogWarning("[Orchestrator] No hay acciones en la lista");
            return;
        }

        var q = goap.Plan(world, actions, goalFlag);
        btRunner.currentPlan = q;

        var names = q != null ? string.Join(" -> ", q.Select(a => a != null ? a.actionName : "null")) : "(null)";
        Debug.Log($"[Orchestrator] Replan({goalFlag}) => [{names}]");
    }

    void Update()
    {
        if (btRunner == null) return;
        var s = btRunner.TickPlan();
        if (s == BTState.Failure)
        {
        }
    }
}
