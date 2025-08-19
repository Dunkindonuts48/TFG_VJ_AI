using UnityEngine;

namespace TFG.GOAP
{
    public class A2_DisenoCap52 : GoapAction
    {
        void Reset() { 
            actionKey = "A2"; 
            tiempoMinMaxMin = new Vector2(5, 10); 
            deltaEnergia = -15f; 
        }

        public override bool Preconditions(WorldState ws)
        {
            return ws.StringFloat.TryGetValue("energia", out var e) && e >= 40f;
        }

        public override void ApplyEffects(WorldState ws)
        {
            ws.IncFloat("tfg.cap5_analisis_diseno_progreso", 25f);
            if (ws.StringFloat.TryGetValue("tfg.cap5_analisis_diseno_progreso", out var p5) && p5 >= 100f)
                ws.SetBool("tfg.cap5_analisis_diseno_completo", true);
            ApplyPhysCost(ws);
        }
    }
}
