using UnityEngine;

namespace TFG.GOAP
{
    public class A1_RedactarSeccion : GoapAction
    {
        void Reset() { 
            actionKey = "A1"; 
            tiempoMinMaxMin = new Vector2(10, 20); 
            deltaEnergia = -20f; 
            deltaEstres = +10f; 
        }

        public override bool Preconditions(WorldState ws)
        {
            bool energiaOK = ws.StringFloat.TryGetValue("energia", out var e) && e >= 30f;
            bool ruidoOK = ws.StringBools.TryGetValue("ruido_ambiente_bajo", out var r) && r;
            bool toolsOK = ws.StringBools.TryGetValue("herramientas_ok", out var h) && h;
            return energiaOK && ruidoOK && toolsOK;
        }

        public override void ApplyEffects(WorldState ws)
        {
            ws.IncInt("tfg.capitulos_redactados", 1);
            ApplyPhysCost(ws);
        }
    }
}
