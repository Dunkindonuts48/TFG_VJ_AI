using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Linq;
using TFG.Memory;

namespace TFG.Memory
{
    public class DialogueAgent : MonoBehaviour
    {
        public MemoryRepository memory;
        [TextArea(3, 6)]
        public string systemPersona = "Eres un NPC amable y coherente con sus recuerdos. Contestas en 1-3 frases.";
        public string llmModel = "mistral:7b-instruct";
        public string embModel = "mxbai-embed-large";
        public bool useEmbeddings = false;
        public int shortTermMaxTurns = 8;
        public int recallTopK = 5;
        public int summarizeEveryNTurns = 8;

        readonly List<(string role, string text)> shortTerm = new();
        int turnsSinceSummary = 0;

        public int ShortTermPairs => shortTerm.Count / 2;
        public int LongTermSummariesCount => memory?.AllReadOnly?.Count(r => r.type == MemoryType.Semantic &&  r.tags != null && r.tags.Any(t => t.Equals("resumen", StringComparison.OrdinalIgnoreCase))) ?? 0;
        public int TotalMemories => memory?.AllReadOnly?.Count ?? 0;

        void Awake()
        {
            if (!memory) memory = GetComponent<MemoryRepository>();
            if (!memory) Debug.LogWarning("[DialogueAgent] No MemoryRepository asignado.");
        }

        public async Task<string> TalkAsync(string userText)
        {
            if (string.IsNullOrWhiteSpace(userText)) return "¿Puedes repetir?";

            float[] qEmb = null;
            if (useEmbeddings)
            {
                try { qEmb = await OllamaClient.EmbedAsync(userText, embModel); } catch { }
            }
            var top = memory.Recall(new[] { "lore", "historia", "jugador", "dialogo" }, qEmb, recallTopK);

            var prompt = Prompts.ChatPrompt(systemPersona, shortTerm, top, userText);

            string reply;
            try
            {
                reply = await OllamaClient.GenerateAsync(prompt, llmModel);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[DialogueAgent] LLM error: " + e.Message);
                return "Ahora mismo no puedo responder.";
            }
            reply = Clean(reply);
            shortTerm.Add(("Usuario", userText));
            shortTerm.Add(("Asistente", reply));
            while (shortTerm.Count > shortTermMaxTurns * 2) shortTerm.RemoveRange(0, 2);

            memory.Remember(new MemoryRecord
            {
                id = Guid.NewGuid().ToString(),
                type = MemoryType.Episodic,
                content = $"U:{userText} | N:{reply}",
                tags = new[] { "jugador", "dialogo" },
                timestamp = DateTime.UtcNow,
                importance = 0.5f,
                occurrences = 1
            });
            turnsSinceSummary++;
            if (turnsSinceSummary >= summarizeEveryNTurns)
            {
                var transcript = string.Join("\n", shortTerm.ConvertAll(t => $"{t.role}: {t.text}"));
                var sumPrompt = Prompts.SummarizeTurnsPrompt(transcript);
                try
                {
                    var summary = await OllamaClient.GenerateAsync(sumPrompt, llmModel);
                    summary = Clean(summary);
                    memory.Remember(new MemoryRecord
                    {
                        id = Guid.NewGuid().ToString(),
                        type = MemoryType.Semantic,
                        content = summary,
                        tags = new[] { "resumen", "dialogo" },
                        timestamp = DateTime.UtcNow,
                        importance = 0.6f,
                        occurrences = 1
                    });
                }
                catch { }
                turnsSinceSummary = 0;
            }

            return reply;
        }

        string Clean(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "";
            raw = raw.Trim();
            if (raw.StartsWith("\"") && raw.EndsWith("\"") && raw.Length > 1) raw = raw.Substring(1, raw.Length - 2).Trim();
            if (raw.StartsWith("- ")) raw = raw.Substring(2).Trim();
            return raw;
        }
    }
}