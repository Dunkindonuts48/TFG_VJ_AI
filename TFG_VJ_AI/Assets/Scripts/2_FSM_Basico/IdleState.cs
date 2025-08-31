using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : NPCState
{
    private float idleTime;
    private float timer;

    public IdleState(NPCStateMachine sm, float idleDuration = 2f) : base(sm)
    {
        idleTime = idleDuration;
    }

    public override void Enter()
    {
        timer = 0f;
        Debug.Log("Entrando en Idle");
    }

    public override void Execute()
    {
        timer += Time.deltaTime;
        if (timer >= idleTime)
        {
            stateMachine.ChangeState(new PatrolState(stateMachine));
        }
    }

    public override void Exit()
    {
        Debug.Log("Saliendo de Idle");
    }
}
