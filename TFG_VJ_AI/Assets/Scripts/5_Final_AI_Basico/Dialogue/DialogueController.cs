using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class DialogueController : MonoBehaviour
{
    public MemoryRepository memory;
    public HybridOrchestrator orchestrator;

    float Clip(float v, float min, float max) => Mathf.Clamp(v, min, max);

    public async Task<string> OnPlayerSays(string text)
    {
        float[] qEmb = null;
        try { qEmb = await OllamaClient.EmbedAsync(text); } catch { }
        var top = memory.Recall(new[] { "jugador", "dialogo" }, qEmb, 5);
        var prompt = Prompts.IntentPrompt(text, top);
        string raw;
        try { raw = await OllamaClient.GenerateAsync(prompt); }
        catch (Exception e) { Debug.LogWarning($"LLM error: {e.Message}"); return "No estoy seguro, ¿puedes repetirlo?"; }
        var ir = MiniJson.TryParseIntent(raw);
        if (ir == null || string.IsNullOrEmpty(ir.intent)) return "No estoy seguro, ¿puedes repetir?";


        if (ir.goalDelta != null)
        {
            foreach (var gd in ir.goalDelta) { orchestrator.ApplyUtilityDelta(gd.goal, Clip(gd.utilityDelta, -3f, 3f)); }
        }
        if (ir.memoryWrites != null)
        {
            foreach (var m in ir.memoryWrites)
            {
                var rec = new MemoryRecord
                {
                    id = Guid.NewGuid().ToString(),
                    type = Enum.TryParse<MemoryType>(m.type, true, out var t) ? t : MemoryType.Social,
                    content = m.content,
                    tags = m.tags != null ? m.tags.ToArray() : new string[0],
                    timestamp = DateTime.UtcNow,
                    importance = Mathf.Clamp01(m.importance),
                    occurrences = 1
                };
                try { rec.embedding = await OllamaClient.EmbedAsync(rec.content); } catch { }
                memory.Remember(rec);
            }
        }
        orchestrator.RouteIntent(ir.intent, ir.slots);
        return string.IsNullOrEmpty(ir.npcReply) ? "Entendido." : ir.npcReply;
    }
}

public static class MiniJson
{
    public static IntentResponse TryParseIntent(string raw)
    {
        try
        {
            string s = raw.Trim();
            if (!s.StartsWith("{") || !s.EndsWith("}")) return null;
            var ir = new IntentResponse();
            ir.intent = ExtractString(s, "\"intent\"");
            ir.npcReply = ExtractString(s, "\"npcReply\"");
            var gdGoal = ExtractString(s, "\"goal\"");
            var gdDeltaStr = ExtractNumber(s, "\"utilityDelta\"");
            if (!string.IsNullOrEmpty(gdGoal))
                ir.goalDelta = new System.Collections.Generic.List<GoalDelta> { new GoalDelta { goal = gdGoal, utilityDelta = gdDeltaStr } };
            var mwContent = ExtractString(s, "\"content\"");
            if (!string.IsNullOrEmpty(mwContent))
            {
                var mwType = ExtractString(s, "\"type\"");
                ir.memoryWrites = new System.Collections.Generic.List<MemWrite> { new MemWrite { type = string.IsNullOrEmpty(mwType) ? "Social" : mwType, content = mwContent, tags = new System.Collections.Generic.List<string>(), importance = 0.5f } };
            }
            ir.slots = new System.Collections.Generic.Dictionary<string, string>();
            return ir;
        }
        catch { return null; }
    }


    static string ExtractString(string s, string key)
    {
        int i = s.IndexOf(key); if (i < 0) return null;
        i = s.IndexOf(':', i); if (i < 0) return null; i++;
        while (i < s.Length && char.IsWhiteSpace(s[i])) i++;
        if (i >= s.Length || s[i] != '"') return null; i++;
        int j = i; while (j < s.Length && s[j] != '"') j++;
        return j < s.Length ? s.Substring(i, j - i) : null;
    }


    static float ExtractNumber(string s, string key)
    {
        int i = s.IndexOf(key); if (i < 0) return 0f;
        i = s.IndexOf(':', i); if (i < 0) return 0f; i++;
        int j = i; while (j < s.Length && (char.IsDigit(s[j]) || s[j] == '-' || s[j] == '+' || s[j] == '.')) j++;
        float.TryParse(s.Substring(i, j - i), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var v);
        return v;
    }
}
