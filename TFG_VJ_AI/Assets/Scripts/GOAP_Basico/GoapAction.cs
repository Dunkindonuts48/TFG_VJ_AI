using UnityEngine;

namespace TFG.GOAP
{
    public abstract class GoapAction : MonoBehaviour
    {
        public string actionKey = "Ax";
        public Vector2 tiempoMinMaxMin = new Vector2(30, 60);
        public float deltaEnergia = -10f;
        public float deltaEstres = 0f;

        protected float performTimer = 0f;
        protected float performDuration = 0f;
        public static float timeScaleSecPerMin = 1f;

        public virtual bool Perform(WorldState ws, float dt)
        {
            if (performDuration <= 0f)
            {
                var minutes = Mathf.Clamp(Cost(ws), tiempoMinMaxMin.x, tiempoMinMaxMin.y);
                performDuration = minutes * timeScaleSecPerMin;
                performTimer = 0f;
                Debug.Log($"[GOAP] {actionKey} ~{minutes:F0} min ({performDuration:F1}s escalado)");
            }
            performTimer += dt;
            if (performTimer >= performDuration)
            {
                performDuration = 0f;
                performTimer = 0f;
                return true;
            }
            return false;
        }

        public virtual float Cost(WorldState ws)
        {
            float baseTime = Mathf.Lerp(tiempoMinMaxMin.x, tiempoMinMaxMin.y, 0.5f);
            float penalty = 0f;

            if (ws.StringInteger.TryGetValue("tiempo_disponible_hoy", out var min) && min < baseTime)
                penalty += 10f;
            if (ws.StringFloat.TryGetValue("energia", out var e) && e < 40f)
                penalty += 5f;

            return baseTime + penalty;
        }

        public abstract void ApplyEffects(WorldState ws);

        protected void ApplyPhysCost(WorldState ws)
        {
            int consumedMinutes = Mathf.RoundToInt(Cost(ws));
            ws.ApplyDelta(deltaEnergia, deltaEstres, consumedMinutes);
        }
        public virtual bool Preconditions(WorldState ws) => true;
    }
}
