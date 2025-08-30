using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TFG.GOAP
{
    public class SimpleSequentialPlanner
    {
        public List<GoapAction> Plan(WorldState start, IEnumerable<GoapAction> actions, string goalFlagKey)
        {
            var remaining = new HashSet<GoapAction>(actions);
            var ordered = new List<GoapAction>();
            var sim = start.Clone();

            const int hardLimit = 128;
            int guard = 0;

            while (!sim.Get(goalFlagKey) && remaining.Count > 0 && guard++ < hardLimit)
            {
                var selectable = remaining.Where(a => a.CanRun(sim)).ToList();
                if (selectable.Count == 0) break;
                GoapAction best = selectable
                    .OrderByDescending(a => remaining.Count(r => r.Preconditions.Contains(a.EffectKey)))
                    .First();

                ordered.Add(best);
                best.Apply(sim);
                remaining.Remove(best);
            }

            if (!sim.Get(goalFlagKey))
            {
                Debug.LogWarning("[Planner] No se alcanzó la meta. Revisa precondiciones/energía.");
                return new List<GoapAction>();
            }

            return ordered;
        }
    }
}

