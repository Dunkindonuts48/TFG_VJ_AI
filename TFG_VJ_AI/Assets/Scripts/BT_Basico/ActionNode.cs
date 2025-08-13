using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionNode : BTNode
{
    private readonly Func<IEnumerator> action;
    private IEnumerator routine;

    public ActionNode(Func<IEnumerator> act) => action = act;

    public override State Tick()
    {
        if (routine == null) routine = action?.Invoke();

        if (routine == null)
            return CurrentState = State.Success;

        if (routine.MoveNext())
            return CurrentState = State.Running;

        routine = null;
        return CurrentState = State.Success;
    }
}