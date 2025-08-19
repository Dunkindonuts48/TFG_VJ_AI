using UnityEngine;

namespace TFG.GOAP
{
    public class A9_PausaActiva : GoapAction
    {
        void Reset() { 
            actionKey = "A9"; 
            tiempoMinMaxMin = new Vector2(15, 20); 
            deltaEnergia = +20f; 
            deltaEstres = -15f; 
        }

        public override bool Preconditions(WorldState ws)
        {
            bool energiaBaja = ws.StringFloat.TryGetValue("energia", out var e) && e < 50f;
            bool estresAlto = ws.StringFloat.TryGetValue("nivel_estres", out var s) && s > 60f;
            return energiaBaja || estresAlto;
        }

        public override float Cost(WorldState ws)
        {
            // Duración aleatoria dentro del rango
            return Random.Range(tiempoMinMaxMin.x, tiempoMinMaxMin.y);
        }

        public override void ApplyEffects(WorldState ws)
        {
            ApplyPhysCost(ws);
        }
    }
}
