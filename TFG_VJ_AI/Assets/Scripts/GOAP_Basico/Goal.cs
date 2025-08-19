using System;
using UnityEngine;

namespace TFG.GOAP
{
    [CreateAssetMenu(menuName = "TFG/GOAP/Goal", fileName = "NewGoal")]
    public class Goal : ScriptableObject
    {
        public string goalKey;
        public float impacto = 0.8f;
        public bool usaDeadline = true;
        public DateTime deadline;
        public string descripcion;
        public Func<WorldState, bool> IsSatisfied;
        public Func<WorldState, float> Riesgo;

        public float Priority(WorldState ws, DateTime now)
        {
            float urgencia = 0f;

            if (usaDeadline && deadline != default)
            {
                var days = Mathf.Max(0.0001f, (float)(deadline - now).TotalDays);
                urgencia = Mathf.Clamp01(1f / days);
            }

            float riesgo = Mathf.Clamp01(Riesgo != null ? Riesgo(ws) : 0.2f);

            const float wDeadline = 0.55f;
            const float wImpacto = 0.30f;
            const float wRiesgo = 0.15f;

            return wDeadline * urgencia + wImpacto * impacto + wRiesgo * riesgo;
        }
    }
}
