using UnityEngine;

namespace TFG.GOAP
{
    public class A5_Citas : GoapAction
    {
        void Reset() { 
            actionKey = "A5";
            tiempoMinMaxMin = new Vector2(10, 15); 
            deltaEnergia = -10f; 
        }

        public override bool Preconditions(WorldState ws)
        {
            return ws.StringInteger.TryGetValue("bibliografia.citas_pendientes", out var c) && c > 0;
        }

        public override void ApplyEffects(WorldState ws)
        {
            int k = Random.Range(3, 7);
            ws.IncInt("bibliografia.citas_pendientes", -k);
            if (ws.StringInteger["bibliografia.citas_pendientes"] < 0)
                ws.StringInteger["bibliografia.citas_pendientes"] = 0;

            ApplyPhysCost(ws);
        }
    }
}
