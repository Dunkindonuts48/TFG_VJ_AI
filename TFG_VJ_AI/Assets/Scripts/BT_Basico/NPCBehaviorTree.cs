using System.Collections;
using Unity.Profiling;
using UnityEngine;

namespace BT_Basico
{
    public class NPCBehaviorTree : MonoBehaviour
    {
        static readonly ProfilerMarker BT_Construct = new("BT.Construct");
        static readonly ProfilerMarker BT_Tick = new("BT.Tick");

        private BTNode root;
        private NPCStateMachine_BT sm;

        public void ConstructTree(NPCStateMachine_BT sm)
        {
            using (BT_Construct.Auto())
            {
                this.sm = sm;

                float sqrAttack = sm.AttackRange * sm.AttackRange;
                float sqrDetect = sm.DetectionRange * sm.DetectionRange;

                var attackSeq = new Sequence(
                    new ConditionNode(() =>
                    {
                        if (!sm.Player) return false;
                        var d = transform.position - sm.Player.position;
                        return d.sqrMagnitude <= sqrAttack;
                    }),
                    new ActionNode(() => sm.AttackRoutine())
                );

                var chaseSeq = new Sequence(
                    new ConditionNode(() =>
                    {
                        if (!sm.Player) return false;
                        var d = transform.position - sm.Player.position;
                        return d.sqrMagnitude <= sqrDetect;
                    }),
                    new ActionNode(() => { sm.StartChase(); return null; })
                );

                var patrolAction = new ActionNode(() => { sm.StartPatrol(); return null; });

                root = new Selector(attackSeq, chaseSeq, patrolAction);
            }
        }

        public void Tick()
        {
            using (BT_Tick.Auto())
            {
                root?.Tick();
            }
        }
    }
}
