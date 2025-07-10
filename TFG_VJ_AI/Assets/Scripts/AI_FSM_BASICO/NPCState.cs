using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NPCState
{
    protected readonly NPCStateMachine stateMachine;

    protected NPCState(NPCStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }
    public virtual void Enter() { }
    public abstract void Execute();
    public virtual void Exit() { }
}
