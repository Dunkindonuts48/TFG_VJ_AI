using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DebugAILogger : MonoBehaviour
{
    public HybridOrchestrator orchestrator;

    float t;
    void Update()
    {
        if (!orchestrator) return;
        t += Time.deltaTime;
        if (t < 1f) return;
        t = 0f;

        var goals = string.Join(", ", orchestrator.goalUtilities.Select(kv => $"{kv.Key}:{kv.Value:F1}"));
        Debug.Log($"[AI] Goals: {goals}");

        if (orchestrator.btRunner != null && orchestrator.btRunner.currentPlan != null)
        {
            var names = orchestrator.btRunner.currentPlan.Select(a => a != null ? a.actionName : "null").ToArray();
            Debug.Log($"[AI] Plan: [{string.Join(" -> ", names)}]");
        }
    }
}
