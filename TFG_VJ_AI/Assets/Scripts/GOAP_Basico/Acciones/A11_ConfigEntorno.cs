using UnityEngine;

namespace TFG.GOAP
{
    public class A11_ConfigEntorno : GoapAction
    {
        void Reset() { 
            actionKey = "A11"; 
            tiempoMinMaxMin = new Vector2(20, 30); 
            deltaEnergia = -5f; 
        }

        public override bool Preconditions(WorldState ws)
        {
            return ws.StringBools.TryGetValue("herramientas_ok", out var ok) && !ok;
        }

        public override void ApplyEffects(WorldState ws)
        {
            ws.SetBool("herramientas_ok", true);
            ApplyPhysCost(ws);
        }
    }
}
