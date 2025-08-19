using UnityEngine;

namespace TFG.GOAP
{
    public class A10_MitigacionBloqueo : GoapAction
    {
        void Reset() { 
            actionKey = "A10"; 
            tiempoMinMaxMin = new Vector2(20, 30); 
            deltaEnergia = -2f; 
            deltaEstres = -10f; 
        }

        public override bool Preconditions(WorldState ws)
        {
            return ws.StringBools.TryGetValue("atasco", out var a) && a;
        }

        public override void ApplyEffects(WorldState ws)
        {
            ws.SetBool("atasco", false);
            ApplyPhysCost(ws);
        }
    }
}
