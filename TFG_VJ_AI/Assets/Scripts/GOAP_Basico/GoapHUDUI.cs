using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace TFG.GOAP
{
    public class GoapHUDUI : MonoBehaviour
    {
        public WorldState world;
        public GoapAgentTFG agent;

        public Slider sEnergia;
        public Slider sEstres;
        public Slider sTiempo;
        public Text tGoal;
        public Text tPlan;

        public Image energiaFill;
        public Image estresFill;

        void Awake()
        {
            if (!world) world = GetComponent<WorldState>();
            if (!agent) agent = GetComponent<GoapAgentTFG>();
        }

        void Update()
        {
            float energia = world.StringFloat.TryGetValue("energia", out var e) ? e : 0f;
            float estres = world.StringFloat.TryGetValue("nivel_estres", out var s) ? s : 0f;
            int tiempo = world.StringInteger.TryGetValue("tiempo_disponible_hoy", out var t) ? t : 0;

            if (sEnergia) sEnergia.value = Mathf.Clamp(energia, 0f, sEnergia.maxValue);
            if (sEstres) sEstres.value = Mathf.Clamp(estres, 0f, sEstres.maxValue);
            if (sTiempo) sTiempo.value = Mathf.Clamp(tiempo, 0f, sTiempo.maxValue);

            if (tGoal) tGoal.text = $"Goal: {agent.currentGoal}";
            if (tPlan) tPlan.text = "Plan: " + string.Join(" → ", agent.currentPlanKeys);

            if (energiaFill)
            {
                energiaFill.color = Color.Lerp(Color.red, Color.green, energia / 100f);
            }
            if (estresFill)
            {
                estresFill.color = Color.Lerp(Color.green, Color.red, estres / 100f);
            }
        }
    }
}
