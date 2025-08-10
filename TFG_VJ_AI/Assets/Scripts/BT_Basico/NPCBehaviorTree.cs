using System.Collections;
using UnityEngine;

namespace BT_Basico
{
    public class NPCBehaviorTree : MonoBehaviour
    {
        private BTNode root;
        private NPCStateMachine_BT sm;
        public void ConstructTree(NPCStateMachine_BT sm)
        {
            this.sm = sm;
            var attackSeq = new Sequence(
                new ConditionNode(() =>
                    Vector3.Distance(transform.position, sm.Player.position) <= sm.AttackRange),
                new ActionNode(() => sm.AttackRoutine())
            );
            var chaseSeq = new Sequence(
                new ConditionNode(() =>
                    Vector3.Distance(transform.position, sm.Player.position) <= sm.DetectionRange),
                new ActionNode(() =>
                {
                    sm.StartChase();
                    return null;
                })
            );
            var patrolAction = new ActionNode(() =>
            {
                sm.StartPatrol();
                return null;
            });
            root = new Selector(attackSeq, chaseSeq, patrolAction);
        }
        public void Tick()
        {
            root.Tick();
        }
    }
}