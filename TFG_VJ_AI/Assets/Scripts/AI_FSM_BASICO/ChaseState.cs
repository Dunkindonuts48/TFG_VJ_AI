using UnityEngine;

public class ChaseState : NPCState
{
    private readonly Transform player;
    private readonly float speed;

    public ChaseState(NPCStateMachine sm) : base(sm)
    {
        player = stateMachine.GetPlayer();
        speed = stateMachine.GetChaseSpeed();
    }

    public override void Enter()
    {
        Debug.Log("Entrando en Chase");
    }

    public override void Execute()
    {
        if (player == null || !player.gameObject.activeInHierarchy)
        {
            stateMachine.ChangeState(new PatrolState(stateMachine));
            return;
        }
        stateMachine.NPCTransform.position = Vector3.MoveTowards(
            stateMachine.NPCTransform.position,
            player.position,
            speed * Time.deltaTime
        );
    }


    public override void Exit()
    {
        Debug.Log("Saliendo de Chase");
    }
}
