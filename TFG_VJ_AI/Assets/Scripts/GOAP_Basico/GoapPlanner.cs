using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GoapPlanner
{
    class Node
    {
        public WorldState state;
        public Node parent;
        public GoapAction action;
        public float g, h;
        public float F => g + h;
    }

    private float Heuristic(WorldState s, HashSet<string> goal)
    {
        int missing = 0;
        foreach (var d in goal) if (!s.Has(d)) missing++;
        return missing;
    }

    public bool Plan(WorldState start, HashSet<string> desired, IEnumerable<GoapAction> actions, out Queue<GoapAction> plan)
    {
        plan = new Queue<GoapAction>();
        if (start.Satisfies(desired)) return true;

        var open = new List<Node>();
        var closed = new List<HashSet<string>>();

        var startNode = new Node { state = start.Clone(), g = 0, h = Heuristic(start, desired) };
        open.Add(startNode);

        while (open.Count > 0)
        {
            open = open.OrderBy(n => n.F).ToList();
            var current = open[0];
            open.RemoveAt(0);

            if (current.state.Satisfies(desired))
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

            closed.Add(new HashSet<string>(current.state.facts));

            foreach (var a in actions)
            {
                var pre = a.Preconditions;
                bool ok = true;
                foreach (var p in pre) if (!current.state.Has(p)) { ok = false; break; }
                if (!ok || !a.CheckContext(current.state)) continue;

                var nextState = a.Apply(current.state);
                if (closed.Any(set => set.SetEquals(nextState.facts))) continue;

                var node = new Node
                {
                    state = nextState,
                    parent = current,
                    action = a,
                    g = current.g + a.Cost,
                    h = Heuristic(nextState, desired)
                };
                open.Add(node);
            }
        }
        return false;
    }
}
