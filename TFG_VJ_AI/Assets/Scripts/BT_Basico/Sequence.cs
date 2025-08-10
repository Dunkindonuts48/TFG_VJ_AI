using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sequence : BTNode
{
    private readonly List<BTNode> children;
    public Sequence(params BTNode[] nodes) => children = new List<BTNode>(nodes);

    public override State Tick()
    {
        foreach (var child in children)
        {
            var result = child.Tick();
            if (result == State.Failure)
                return CurrentState = State.Failure;
            if (result == State.Running)
                return CurrentState = State.Running;
        }
        return CurrentState = State.Success;
    }
}
