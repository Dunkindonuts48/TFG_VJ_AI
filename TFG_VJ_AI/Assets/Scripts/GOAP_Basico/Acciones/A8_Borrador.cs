using UnityEngine;

namespace TFG.GOAP
{
    public class A8_Borrador : GoapAction
    {
        void Reset() { 
            actionKey = "A8"; 
            tiempoMinMaxMin = new Vector2(5, 10); 
            deltaEnergia = -15f; 
        }

        public override bool Preconditions(WorldState ws)
        {
            bool cap5_60 = ws.StringFloat.TryGetValue("tfg.cap5_analisis_diseno_progreso", out var p5) && p5 >= 60f;
            bool cap6_60 = ws.StringFloat.TryGetValue("tfg.cap6_sprints_diario_progreso", out var p6) && p6 >= 60f;
            return cap5_60 || cap6_60;
        }

        public override void ApplyEffects(WorldState ws)
        {
            ws.SetBool("borrador_actualizado", true);
            ApplyPhysCost(ws);
        }
    }
}
