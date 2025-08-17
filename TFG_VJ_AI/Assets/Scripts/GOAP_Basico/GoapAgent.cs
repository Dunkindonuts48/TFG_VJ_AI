using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoapAgent : MonoBehaviour
{
    [Header("Assets")]
    public List<GoapGoal> goals = new List<GoapGoal>();

    [Header("Debug")]
    public string currentGoal;
    public List<string> currentPlan = new List<string>();

    // Estado del mundo (actualízalo con sensores)
    public WorldState world = new WorldState();

    private Queue<GoapAction> planQueue = new Queue<GoapAction>();
    private GoapAction runningAction;
    private GoapPlanner planner = new GoapPlanner();
    private float replanCooldown = 0.5f, replanTimer = 0f;

    void Start()
    {
        // Ejemplo de hechos iniciales
        // world.Add("alive"); world.Add("hasMoney");
    }

    void Update()
    {
        Sense();

        // 2) Si no hay acción corriendo, intenta ejecutar o replanifica
        if (runningAction == null)
        {
            if (planQueue.Count == 0 && (replanTimer <= 0f))
                BuildBestPlan();

            if (planQueue.Count > 0)
            {
                runningAction = planQueue.Dequeue();
                runningAction.OnStart();
            }
        }
        else
        {
            // 3) Ejecuta
            bool done = runningAction.Perform(Time.deltaTime);
            if (done)
            {
                runningAction.OnEnd(true);
                runningAction = null;
                // (opcional) aplicar efectos al mundo si no lo haces en Perform
                foreach (var e in runningAction.Effects) world.Add(e);
            }
            else
            {
                // si algo invalida las precondiciones, replanifica
                if (!PreconditionsStillHold(runningAction))
                {
                    runningAction.OnEnd(false);
                    runningAction = null;
                    planQueue.Clear();
                    replanTimer = replanCooldown;
                }
            }
        }

        if (replanTimer > 0f) replanTimer -= Time.deltaTime;

        // Debug UI
        currentPlan.Clear();
        foreach (var a in planQueue) currentPlan.Add(a.name);
    }

    bool PreconditionsStillHold(GoapAction a)
    {
        foreach (var p in a.Preconditions) if (!world.Has(p)) return false;
        return true;
    }

    void BuildBestPlan()
    {
        GoapGoal best = null;
        Queue<GoapAction> bestPlan = null;

        foreach (var g in goals)
        {
            if (world.Satisfies(g.DesiredSet)) continue;
            if (planner.Plan(world, g.DesiredSet, GetComponents<GoapAction>(), out var plan))
            {
                if (best == null || g.priority > best.priority)
                {
                    best = g;
                    bestPlan = plan;
                }
            }
        }

        if (best != null && bestPlan != null)
        {
            currentGoal = best.name;
            foreach (var a in GetComponents<GoapAction>()) a.OnPlanCalculated();
            planQueue = bestPlan;
        }
        else
        {
            currentGoal = "(sin plan)";
            planQueue.Clear();
        }
    }

    void Sense()
    {
        // EJEMPLO sencillo: cada 60 s el agente tiene hambre
        // (Cámbialo por tus sensores reales)
        // if (Time.time % 60f < Time.deltaTime) world.Add("hungry");
        // if (world.Has("ate")) { world.Remove("hungry"); world.Remove("ate"); }
    }
}