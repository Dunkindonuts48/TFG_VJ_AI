using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TFG.GOAP
{
    public class PlannerAStar
    {
        class Node
        {
            public WorldState state;
            public Node parent;
            public GoapAction action;
            public float CostSoFar;
            public float HeuristicCost;
            public float TotalCost => CostSoFar + HeuristicCost;
        }

        public bool Plan(WorldState start, Goal goal, IEnumerable<GoapAction> actions, out Queue<GoapAction> plan)
        {
            plan = new Queue<GoapAction>();
            if (goal.IsSatisfied != null && goal.IsSatisfied(start)) return true;

            var open = new List<Node>();
            var closed = new HashSet<string>();

            var root = new Node { state = start.Snapshot(), CostSoFar = 0f, HeuristicCost = Heuristic(start, goal) };
            open.Add(root);

            int guard = 0;
            while (open.Count > 0 && guard++ < 600)
            {
                open = open.OrderBy(n => n.TotalCost).ToList();
                var current = open[0];
                open.RemoveAt(0);

                if (goal.IsSatisfied != null && goal.IsSatisfied(current.state))
                {
                    var stack = new Stack<GoapAction>();
                    while (current.parent != null)
                    {
                        stack.Push(current.action);
                        current = current.parent;
                    }
                    plan = new Queue<GoapAction>(stack);
                    return true;
                }

                var hash = HashState(current.state);
                if (!closed.Add(hash)) continue;

                foreach (var a in actions)
                {
                    if (!a.Preconditions(current.state)) continue;

                    var next = current.state.Snapshot();
                    a.ApplyEffects(next);
                    var key = HashState(next);
                    if (closed.Contains(key)) continue;

                    open.Add(new Node
                    {
                        state = next,
                        parent = current,
                        action = a,
                        CostSoFar = current.CostSoFar + a.Cost(current.state),
                        HeuristicCost = Heuristic(next, goal)
                    });
                }
            }
            return false;
        }

        float Heuristic(WorldState worldState, Goal goal) => (goal.IsSatisfied != null && goal.IsSatisfied(worldState)) ? 0f : 1f;

        string HashState(WorldState worldState)
        {
            string boolS = string.Join(",", worldState.StringBools.Select(kv => kv.Key + ":" + kv.Value));
            string integerS = string.Join(",", worldState.StringInteger.Select(kv => kv.Key + ":" + kv.Value));
            string floatS = string.Join(",", worldState.StringFloat.Select(kv => kv.Key + ":" + Mathf.RoundToInt(kv.Value)));
            return boolS + "|" + integerS + "|" + floatS;
        }
    }
}
