using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BTNode
{
    public enum State { Success, Failure, Running }
    public State CurrentState { get; protected set; }
    public abstract State Tick();
}
