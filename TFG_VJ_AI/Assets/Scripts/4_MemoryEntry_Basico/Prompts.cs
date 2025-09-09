using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TFG.Memory;

namespace TFG.Memory
{
    public static class Prompts
    {
        public static string ChatPrompt(string systemPersona, List<(string role, string text)> shortTermTurns, List<MemoryRecord> longTermMemories,string userText)
        {
            var mem = string.Join("\n- ", longTermMemories.Select(m => m.content));
            var recent = string.Join("\n", shortTermTurns.Select(t => $"{t.role}: {t.text}"));
            return $@"{systemPersona} Contexto (memoria relevante): - {mem} Historial reciente:{recent} Instrucciones: - Responde en español, claro y breve. - Mantén coherencia con el contexto. - Si no sabes algo, dilo explícitamente. Usuario: ""{userText}"" Asistente:";
        }
        public static string SummarizeTurnsPrompt(string transcript)
        {
            return $@"Resume la siguiente conversación a 1-4 frases (hechos, decisiones o relaciones), en español, conciso: ""{transcript}""";
        }
    }
}

