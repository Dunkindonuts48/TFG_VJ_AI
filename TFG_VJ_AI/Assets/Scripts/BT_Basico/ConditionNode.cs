using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionNode : BTNode
{
    private readonly Func<bool> condition;
    public ConditionNode(Func<bool> cond) => condition = cond;

    public override State Tick() =>
        CurrentState = condition() ? State.Success : State.Failure;
}
