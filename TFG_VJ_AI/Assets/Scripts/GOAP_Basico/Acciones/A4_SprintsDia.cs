using UnityEngine;

namespace TFG.GOAP
{
    public class A4_SprintsDia : GoapAction
    {
        void Reset() { 
            actionKey = "A4"; 
            tiempoMinMaxMin = new Vector2(5, 15); 
            deltaEnergia = -10f;
        }

        public override bool Preconditions(WorldState ws)
        {
            return ws.StringBools.TryGetValue("registro_sprints_dia_disponible", out var ok) && ok;
        }

        public override void ApplyEffects(WorldState ws)
        {
            float inc = Random.Range(10f, 20f);
            ws.IncFloat("tfg.cap6_sprints_diario_progreso", inc);

            if (ws.StringFloat.TryGetValue("tfg.cap6_sprints_diario_progreso", out var p6) && p6 >= 100f)
                ws.SetBool("tfg.cap6_sprints_diario_completo", true);

            ApplyPhysCost(ws);
        }
    }
}
