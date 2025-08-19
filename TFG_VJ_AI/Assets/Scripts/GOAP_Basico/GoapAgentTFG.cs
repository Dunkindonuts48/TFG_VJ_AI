using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TFG.GOAP
{
    public class GoapAgentTFG : MonoBehaviour
    {
        public WorldState world;
        public List<Goal> goals = new List<Goal>();

        private PlannerAStar planner = new PlannerAStar();

        public string currentGoal = "(none)";
        public List<string> currentPlanKeys = new List<string>();

        private Queue<GoapAction> plan = new Queue<GoapAction>();
        private GoapAction running;

        public float replanCooldown = 0.5f;
        private float replanTimer = 0f;

        void Awake()
        {
            if (!world) world = GetComponent<WorldState>();
            BootstrapInitialWorld();
            BootstrapGoals();
        }

        void Update()
        {
            if (running == null)
            {
                if (plan.Count == 0 && replanTimer <= 0f)
                    BuildBestPlan();

                if (plan.Count > 0)
                {
                    running = plan.Dequeue();
                    Debug.Log($"[GOAP] Nueva acción: {running.actionKey} (Goal: {currentGoal})");
                }
            }
            else
            {
                if (running.Preconditions(world))
                {
                    bool done = running.Perform(world, Time.deltaTime);
                    if (done)
                    {
                        running.ApplyEffects(world);
                        Debug.Log($"[GOAP] Acción completada: {running.actionKey}");
                        running = null;
                    }
                }
                else
                {
                    Debug.LogWarning($"[GOAP] Acción {running.actionKey} abortada (precondiciones no cumplidas)");
                    running = null;
                    plan.Clear();
                    replanTimer = replanCooldown;
                }
            }

            if (replanTimer > 0f) replanTimer -= Time.deltaTime;
            currentPlanKeys = plan.Select(a => a.actionKey).ToList();
        }

        void BuildBestPlan()
        {
            Goal best = null;
            Queue<GoapAction> bestPlan = null;
            float bestScore = -1f;

            var acts = GetComponents<GoapAction>().Where(a => a.enabled).ToList();
            var now = DateTime.Now;

            foreach (var g in goals)
            {
                if (g.IsSatisfied != null && g.IsSatisfied(world)) continue;

                if (planner.Plan(world, g, acts, out var p))
                {
                    float pr = g.Priority(world, now);
                    if (pr > bestScore)
                    {
                        bestScore = pr;
                        best = g;
                        bestPlan = p;
                    }
                }
            }

            if (best != null && bestPlan != null)
            {
                currentGoal = best.goalKey;
                plan = bestPlan;
                Debug.Log($"[GOAP] Nuevo plan para {best.goalKey}: {string.Join(" -> ", bestPlan.Select(a => a.actionKey))}");
            }
            else
            {
                currentGoal = "(sin plan)";
                plan.Clear();
                Debug.Log("[GOAP] No se encontró plan válido");
            }
        }

        void BootstrapInitialWorld()
        {
            world.SetFloat("energia", 55);
            world.SetFloat("nivel_estres", 35);
            world.SetBool("ruido_ambiente_bajo", true);
            world.SetBool("herramientas_ok", true);

            world.SetInt("tfg.capitulos_redactados", 0);
            world.SetFloat("tfg.cap5_analisis_diseno_progreso", 0);
            world.SetFloat("tfg.cap6_sprints_diario_progreso", 0);
            world.SetBool("tfg.aprobado_director", false);
            world.SetBool("tfg.slides_listas", false);

            world.SetInt("bibliografia.citas_pendientes", 12);
            world.SetInt("tiempo_disponible_hoy", 240);

            world.SetBool("registro_sprints_dia_disponible", true);
            world.SetBool("atasco", false);
            world.SetBool("borrador_actualizado", false);

            world.SetDate("calendario.proxima_reunion_direccion", new DateTime(2025, 8, 19, 9, 30, 0));
        }

        void BootstrapGoals()
        {
            foreach (var g in goals)
            {
                switch (g.goalKey)
                {
                    case "G1":
                        g.IsSatisfied = ws =>
                            ws.StringBools.TryGetValue("tfg.cap5_analisis_diseno_completo", out var ok) && ok;
                        g.deadline = new DateTime(2025, 8, 25);
                        g.Riesgo = ws =>
                        {
                            float p = 0f;
                            ws.StringFloat.TryGetValue("tfg.cap5_analisis_diseno_progreso", out p);
                            return 1f - Mathf.Clamp01(p / 100f);
                        };
                        break;

                    case "G2":
                        g.IsSatisfied = ws =>
                            ws.StringBools.TryGetValue("tfg.cap6_sprints_diario_completo", out var ok2) && ok2;
                        g.deadline = new DateTime(2025, 8, 28);
                        g.Riesgo = ws =>
                        {
                            float p6 = 0f;
                            ws.StringFloat.TryGetValue("tfg.cap6_sprints_diario_progreso", out p6);
                            return 1f - Mathf.Clamp01(p6 / 100f);
                        };
                        break;

                    case "G3":
                        g.IsSatisfied = ws =>
                            ws.StringBools.TryGetValue("tfg.slides_listas", out var sl) && sl;
                        g.deadline = new DateTime(2025, 9, 1);
                        g.Riesgo = ws =>
                        {
                            bool done = false;
                            ws.StringBools.TryGetValue("tfg.slides_listas", out done);
                            return done ? 0f : 0.7f;
                        };
                        break;

                    case "G4":
                        g.IsSatisfied = ws =>
                            ws.StringBools.TryGetValue("tfg.aprobado_director", out var ap) && ap;
                        if (world.StringDates.TryGetValue("calendario.proxima_reunion_direccion", out var d))
                            g.deadline = d;
                        g.Riesgo = ws =>
                        {
                            bool bor = false;
                            ws.StringBools.TryGetValue("borrador_actualizado", out bor);
                            return bor ? 0.4f : 0.8f;
                        };
                        break;

                    case "G5":
                        g.IsSatisfied = ws =>
                            ws.StringInteger.TryGetValue("bibliografia.citas_pendientes", out var c) && c == 0;
                        g.deadline = new DateTime(2025, 8, 29);
                        g.Riesgo = ws =>
                        {
                            int cc = 0;
                            ws.StringInteger.TryGetValue("bibliografia.citas_pendientes", out cc);
                            return Mathf.Clamp01(cc / 20f);
                        };
                        break;

                    case "G6":
                        g.usaDeadline = false;
                        g.IsSatisfied = ws =>
                            ws.StringFloat.TryGetValue("energia", out var e) && e >= 50f &&
                            ws.StringFloat.TryGetValue("nivel_estres", out var s) && s <= 40f;
                        g.Riesgo = ws =>
                        {
                            float s2 = 0f;
                            ws.StringFloat.TryGetValue("nivel_estres", out s2);
                            return Mathf.Clamp01(s2 / 100f);
                        };
                        break;
                }
            }
        }
    }
}