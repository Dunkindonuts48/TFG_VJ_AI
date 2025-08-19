using UnityEngine;

namespace TFG.GOAP
{
    public class A6_Slides : GoapAction
    {
        void Reset() { 
            actionKey = "A6"; 
            tiempoMinMaxMin = new Vector2(20, 25);
            deltaEnergia = -20f; 
            deltaEstres = +5f; 
        }

        public override bool Preconditions(WorldState ws)
        {
            bool cap5ok = ws.StringFloat.TryGetValue("tfg.cap5_analisis_diseno_progreso", out var p5) && p5 >= 80f;
            bool cap6ok = ws.StringFloat.TryGetValue("tfg.cap6_sprints_diario_progreso", out var p6) && p6 >= 80f;
            return cap5ok && cap6ok;
        }

        public override void ApplyEffects(WorldState ws)
        {
            ws.SetBool("tfg.slides_listas", true);
            ApplyPhysCost(ws);
        }
    }
}
