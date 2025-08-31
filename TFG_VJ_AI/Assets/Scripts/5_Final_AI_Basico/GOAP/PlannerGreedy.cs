using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class PlannerGreedy : MonoBehaviour
{
    public MemoryRepository memory;


    float Cost(GoapAction a, WorldState ws)
    {
        float baseC = Mathf.Max(0f, a.energyCost);
        float memC = a.MemoryCost(ws, memory);
        return baseC + memC;
    }


    public Queue<GoapAction> Plan(WorldState start, IEnumerable<GoapAction> all, string goalFlag)
    {
        var sim = start.Clone(); var remaining = new List<GoapAction>(all); var plan = new Queue<GoapAction>();
        int guard = 0; while (!sim.Get<bool>(goalFlag, false) && remaining.Count > 0 && guard++ < 64)
        {
            var pool = remaining.Where(a => a.CanRun(sim)).ToList();
            if (pool.Count == 0) break;
            var best = pool.OrderBy(a => Cost(a, sim)).First();
            plan.Enqueue(best); best.Apply(sim); remaining.Remove(best);
        }
        return plan;
    }
}