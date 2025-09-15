using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace TFG.GOAP
{
    public class GoapHUD : MonoBehaviour
    {
        public GoapAgent agent;
        public Slider energySlider;
        public Slider stressSlider;
        public Slider planProgress;
        public Text txtCurrentAction;
        public Text txtGoal;
        public Text txtPlanNext;
        public Text energyText;
        public Text stressText;
        public Button btnPlan;
        public bool autoscaleEnergyMax = true;
        public float stressMax = 100f;

        Color goalGreen = new Color(0.2f, 0.75f, 0.2f);
        Color goalAmber = new Color(1f, 0.72f, 0.2f);

        void Awake()
        {
            if (agent == null) agent = FindObjectOfType<GoapAgent>();

            if (agent != null)
            {
                agent.OnPlanBuilt += RefreshPlanSnapshot;
                agent.OnActionStarted += _ => RefreshPlanSnapshot();
                agent.OnActionFinished += _ => RefreshPlanSnapshot();
                agent.OnPlanCompleted += () => RefreshPlanSnapshot();
            }
        }

        void Start()
        {
            if (energySlider != null)
            {
                if (autoscaleEnergyMax && agent != null)
                    energySlider.maxValue = Mathf.Max(energySlider.maxValue, agent.energiaInicial + 40f);
                energySlider.minValue = 0f;
            }

            if (stressSlider != null)
            {
                stressSlider.minValue = 0f;
                stressSlider.maxValue = stressMax;
            }

            if (planProgress != null)
            {
                planProgress.minValue = 0f;
                planProgress.maxValue = 1f;
                planProgress.value = 0f;
            }

            if (btnPlan != null && agent != null)
            {
                btnPlan.onClick.RemoveAllListeners();
                btnPlan.onClick.AddListener(agent.PlanAndExecute);
            }

            RefreshPlanSnapshot();
        }

        void Update()
        {
            if (agent == null) return;

            if (energySlider != null) energySlider.value = agent.EnergiaActual;
            if (stressSlider != null) stressSlider.value = agent.EstresActual;

            if (energyText != null) energyText.text = $"Energía: {agent.EnergiaActual:0} / {energySlider.maxValue:0}";
            if (stressText != null) stressText.text = $"Estrés: {agent.EstresActual:0} / {stressSlider.maxValue:0}";

            if (txtCurrentAction != null)
            {
                txtCurrentAction.text = string.IsNullOrEmpty(agent.CurrentActionName)
                    ? "Acción actual: —"
                    : $"Acción actual: 🟢 {agent.CurrentActionName}";
            }

            if (txtGoal != null)
            {
                bool done = agent.ObjetivoCumplido;
                txtGoal.text = done
                    ? $"Objetivo: {agent.ObjetivoKey} ✅"
                    : $"Objetivo: {agent.ObjetivoKey} …";
                txtGoal.color = done ? goalGreen : goalAmber;
            }

            if (planProgress != null && agent.PlanTotalCount > 0)
                planProgress.value = (float)agent.PlanDoneCount / agent.PlanTotalCount;
        }

        void RefreshPlanSnapshot()
        {
            if (agent == null || txtPlanNext == null) return;

            if (agent.PlannedActionNames == null || agent.PlannedActionNames.Count == 0)
            {
                txtPlanNext.text = "Plan: —";
                return;
            }

            var lines = agent.PlannedActionNames.Select((name, idx) =>
            {
                if (idx < agent.PlanDoneCount) return $"✔ {name}";
                if (!string.IsNullOrEmpty(agent.CurrentActionName) && idx == agent.PlanDoneCount) return $"🟢 {name}";
                return $"• {name}";
            });

            txtPlanNext.text = "Plan:\n" + string.Join("\n", lines);
        }
    }
}