using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TFG.GOAP
{
    public class GoapAgent : MonoBehaviour
    {
        [Header("Estado inicial")]
        public float energiaInicial = 100f;
        public float estresInicial = 10f;

        [Header("Objetivo")]
        public string goalFlagKey = WSKeys.TFG_Presentado;

        private WorldState world;
        private List<GoapAction> actions;
        private SimpleSequentialPlanner planner;

        private Queue<GoapAction> currentPlan = new();

        void Awake()
        {
            world = new WorldState(energiaInicial, estresInicial);
            planner = new SimpleSequentialPlanner();
            actions = new List<GoapAction>(GetComponents<GoapAction>());
        }

        [ContextMenu("Plan & Execute")]
        public void PlanAndExecute()
        {
            var plan = planner.Plan(world, actions, goalFlagKey);
            currentPlan = new Queue<GoapAction>(plan);
            PlannedActionNames.Clear();
            PlannedActionNames.AddRange(plan.ConvertAll(a => a.GetType().Name));
            PlanTotalCount = PlannedActionNames.Count;
            PlanDoneCount = 0;
            CurrentActionName = "";
            OnPlanBuilt?.Invoke();

            StopAllCoroutines();
            StartCoroutine(ExecutePlan());
        }


        private IEnumerator ExecutePlan()
        {
            GoapAction rest = null;
            foreach (var a in actions)
            {
                float dE = GetField(a, "energyCost");
                if (string.IsNullOrEmpty(a.EffectKey) && dE < 0f) { rest = a; break; }
            }

            while (currentPlan.Count > 0)
            {
                var a = currentPlan.Peek();
                float needE = GetField(a, "energyCost");

                if (!a.CanRun(world))
                {
                    if (rest != null && world.Energia < needE)
                    {
                        CurrentActionName = rest.GetType().Name;
                        OnActionStarted?.Invoke(CurrentActionName);

                        rest.Apply(world);

                        OnActionFinished?.Invoke(CurrentActionName);
                        CurrentActionName = "";
                        yield return new WaitForSeconds(0.15f);
                        continue;
                    }

                    Debug.LogWarning($"[Agent] Acción {a.GetType().Name} no se puede ejecutar ahora. Abortando.");
                    yield break;
                }

                CurrentActionName = a.GetType().Name;
                OnActionStarted?.Invoke(CurrentActionName);

                var effectKey = a.EffectKey;
                var effectInfo = string.IsNullOrEmpty(effectKey) ? "(sin flag)" : $"set '{effectKey}'={a.EffectValue}";
                float dE = GetField(a, "energyCost");
                float dS = GetField(a, "stressGain");
                Debug.Log($"[Agent] Ejecutando: {CurrentActionName} → {effectInfo} (ΔE={dE}, ΔS={dS})");

                a.Apply(world);

                currentPlan.Dequeue();
                PlanDoneCount = Mathf.Min(PlanDoneCount + 1, PlanTotalCount);
                OnActionFinished?.Invoke(CurrentActionName);

                CurrentActionName = "";

                Debug.Log($"[Agent] Estado => Energía:{world.Energia:F1} | Estrés:{world.Estres:F1} | Goal:{goalFlagKey}={world.Get(goalFlagKey)}");
                yield return new WaitForSeconds(0.35f);
            }

            OnPlanCompleted?.Invoke();

            if (world.Get(goalFlagKey))
                Debug.Log($"[Agent] ¡Meta alcanzada! {goalFlagKey}=true");
            else
                Debug.LogWarning("[Agent] Plan ejecutado pero la meta no se alcanzó.");
        }

        private static float GetField(GoapAction a, string field)
        {
            var f = typeof(GoapAction).GetField(field, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return f != null ? (float)f.GetValue(a) : 0f;
        }

        public float EnergiaActual => world != null ? world.Energia : 0f;
        public float EstresActual => world != null ? world.Estres : 0f;
        public bool ObjetivoCumplido => world != null && world.Get(goalFlagKey);
        public string ObjetivoKey => goalFlagKey;

        public System.Action<string> OnActionStarted;
        public System.Action<string> OnActionFinished;
        public System.Action OnPlanBuilt;
        public System.Action OnPlanCompleted;
        public string CurrentActionName { get; private set; } = "";
        public int PlanTotalCount { get; private set; } = 0;
        public int PlanDoneCount { get; private set; } = 0;
        public List<string> PlannedActionNames { get; private set; } = new List<string>();

    }
}
