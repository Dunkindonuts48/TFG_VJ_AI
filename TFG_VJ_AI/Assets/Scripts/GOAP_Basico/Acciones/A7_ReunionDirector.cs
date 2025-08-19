using System;
using UnityEngine;

namespace TFG.GOAP
{
    public class A7_ReunionDirector : GoapAction
    {
        void Reset() {
            actionKey = "A7"; 
            tiempoMinMaxMin = new Vector2(15, 20); 
            deltaEnergia = -5f;
        }

        public override bool Preconditions(WorldState ws)
        {
            if (!ws.StringDates.TryGetValue("calendario.proxima_reunion_direccion", out var d)) return false;
            var now = DateTime.Now;
            bool sameDay = d.Date == now.Date;
            bool nearHour = Mathf.Abs((float)(d - now).TotalMinutes) <= 90f; // ventana ±90 min
            bool borrador = ws.HasBool("borrador_actualizado", true);
            return sameDay && nearHour && borrador;
        }

        public override float Cost(WorldState ws)
        {
            return Mathf.Lerp(tiempoMinMaxMin.x, tiempoMinMaxMin.y, 0.5f);
        }

        public override void ApplyEffects(WorldState ws)
        {
            bool aprobado = UnityEngine.Random.value < 0.6f;
            ws.SetBool("tfg.aprobado_director", aprobado);

            deltaEstres = UnityEngine.Random.Range(-5f, +5f);
            ApplyPhysCost(ws);
        }
    }
}
