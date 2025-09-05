using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selector : BTNode
{
    private readonly List<BTNode> children;
    public Selector(params BTNode[] nodes) => children = new List<BTNode>(nodes);

    public override State Tick()
    {
        foreach (var child in children)
        {
            var result = child.Tick();
            if (result == State.Success)
                return CurrentState = State.Success;
            if (result == State.Running)
                return CurrentState = State.Running;
        }
        return CurrentState = State.Failure;
    }
}
