using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TFG.Memory
{
    public class MemoryRepository : MonoBehaviour
    {
        [Header("Parámetros de scheduling")]
        [Tooltip("Cada m turnos se resume/olvida el Short-Term")]
        public int m = 10;
        [Tooltip("S de la curva e^{-t/S}")]
        public float S = 12f;
        [Tooltip("Repeticiones para ser forever")]
        public int B = 3;

        [Header("Límites aproximados (proxy de tokens)")]
        public int shortTermMaxChars = 2000;
        public int longTermMaxChars = 10000;

        [Header("Meta prompt (Pm): personalidad, rol, etc.")]
        [TextArea(4, 10)]
        public string metaPrompt =
            "Eres un NPC amable, profesor de IA. Responde con contexto y de forma coherente.";

        [Header("LLM (opcional)")]
        public bool useLLMForSummaries = true;
        [Tooltip("Nombre del modelo en Ollama (p.ej. 'mistral:7b-instruct')")]
        public string ollamaModel = "mistral:7b-instruct";
        [Tooltip("Tiempo máximo de espera para el resumen LLM (segundos)")]
        public float llmTimeoutSeconds = 20f;

        public MemoryRoom Room { get; private set; } = new();
        public int Turn { get; private set; } = 0;

        public System.Action OnMemoryUpdated;

        private Coroutine summarizeRoutine;

        public void AddTalk(string speaker, string text)
        {
            var entry = new MemoryEntry(speaker, text, Turn);
            Room.ShortTerm.Add(entry);
            Turn++;
            foreach (var e in Room.ShortTerm)
                e.f = Mathf.Exp(-(Turn - e.turnIndex) / Mathf.Max(1e-3f, S));

            UpdateRepeatsAndPriority(entry);
            TrimByCharBudget(Room.ShortTerm, shortTermMaxChars);
            TrimByCharBudget(Room.LongTerm, longTermMaxChars);

            if (Turn % m == 0)
            {
                var batch = Room.ShortTerm.Where(e => e.turnIndex >= Turn - m).ToList();
                if (batch.Count > 0)
                {
                    int forgetCount = Mathf.Max(0, batch.Count / 3);
                    var candidates = batch.Where(e => e.priority == "ordinary")
                                          .OrderBy(e => e.f)
                                          .Take(forgetCount).ToList();
                    if (summarizeRoutine != null) StopCoroutine(summarizeRoutine);
                    summarizeRoutine = StartCoroutine(SummarizeAndForgetLastBatch_C(batch, candidates));
                }
            }

            OnMemoryUpdated?.Invoke();
        }

        void UpdateRepeatsAndPriority(MemoryEntry newE)
        {
            int similar = Room.ShortTerm.Concat(Room.LongTerm)
                .Count(e => SimilarKey(e.text) == SimilarKey(newE.text) && e != newE);

            newE.repeats = Mathf.Max(1, similar + 1);

            if (newE.repeats >= B) newE.priority = "forever";
            else if (newE.repeats > 1) newE.priority = "important";
            else newE.priority = "ordinary";
        }

        string SimilarKey(string s)
        {
            var chars = s.ToLowerInvariant().Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)).ToArray();
            return new string(chars);
        }

        private IEnumerator SummarizeAndForgetLastBatch_C(List<MemoryEntry> batch, List<MemoryEntry> candidatesList)
        {
            var candidates = new HashSet<MemoryEntry>(candidatesList);
            string summary = null;

            if (useLLMForSummaries)
            {
                string system = "Eres un asistente que resume conversaciones en español de forma clara y concisa para un videojuego. Devuelve bullets y una línea de 'Temas: ...'. No inventes.";
                var kept = batch.Where(e => !candidates.Contains(e)).ToList();
                var convo = string.Join("\n", kept.Select(e => $"{e.speaker}: {e.text}"));
                string user =
        $@"Resume los siguientes {kept.Count} turnos. Formato:
[Resumen]
Temas: palabra1, palabra2, ...
- (1) Frase concisa sobre lo más importante
- (2) ...
- (3) ...

Conversación:
{convo}";

                string result = null; string error = null; bool done = false;
                var go = new GameObject("LLMRunner");
                var runner = go.AddComponent<MonoBehaviourRunner>();
                yield return runner.StartCoroutine(LLMClient.ChatOllama(
                    ollamaModel,
                    system,
                    user,
                    s => { result = s; done = true; },
                    e => { error = e; done = true; },
                    llmTimeoutSeconds
                ));
                Destroy(go);

                if (!string.IsNullOrWhiteSpace(error))
                {
                    Debug.LogWarning("[LLM] " + error);
                }
                summary = !string.IsNullOrWhiteSpace(result) ? PostprocessLLM(result) : null;
            }
            if (string.IsNullOrWhiteSpace(summary))
                summary = SummarizeBatch_Local(batch, candidates);

            var sumEntry = new MemoryEntry("Summary", summary, Turn);
            sumEntry.priority = "important";
            Room.LongTerm.Add(sumEntry);

            Room.ShortTerm.RemoveAll(e => candidates.Contains(e));

            OnMemoryUpdated?.Invoke();
        }


        string SummarizeBatchLLMOrFallback(List<MemoryEntry> batch, HashSet<MemoryEntry> forgotten)
        {
            if (!useLLMForSummaries)
                return SummarizeBatch_Local(batch, forgotten);

            string system = "Eres un asistente que resume conversaciones en español de forma clara y concisa para un videojuego. Devuelve bullets y una línea de 'Temas: ...'. No inventes.";
            var kept = batch.Where(e => !forgotten.Contains(e)).ToList();
            var convo = string.Join("\n", kept.Select(e => $"{e.speaker}: {e.text}"));
            string user =
$@"Resume los siguientes {kept.Count} turnos. Formato:
[Resumen]
Temas: palabra1, palabra2, ...
- (1) Frase concisa sobre lo más importante
- (2) ...
- (3) ...

Conversación:
{convo}";

            string result = null;
            bool finished = false;
            bool failed = false;
            var go = new GameObject("LLMRunner");
            var runner = go.AddComponent<MonoBehaviourRunner>();

            runner.StartCoroutine(LLMClient.ChatOllama(
                ollamaModel,
                system,
                user,
                s => { result = s; finished = true; },
                e => { Debug.LogWarning("[LLM] " + e); failed = true; finished = true; },
                llmTimeoutSeconds
            ));

            float t = 0f, TMAX = Mathf.Max(3f, llmTimeoutSeconds);
            while (!finished && t < TMAX) { t += Time.deltaTime; }

            Destroy(go);

            if (failed || string.IsNullOrWhiteSpace(result))
                return SummarizeBatch_Local(batch, forgotten);

            return PostprocessLLM(result);
        }

        string SummarizeBatch_Local(List<MemoryEntry> batch, HashSet<MemoryEntry> forgotten)
        {
            var kept = batch.Where(e => !forgotten.Contains(e)).ToList();
            var topWords = kept.SelectMany(e => e.text.Split(' '))
                               .Where(w => w.Length > 3)
                               .GroupBy(w => w.ToLowerInvariant())
                               .OrderByDescending(g => g.Count())
                               .Take(5)
                               .Select(g => g.Key);
            var bullets = kept.Select(e => $"- {e.speaker}: {e.text}").Take(6);
            return $"[Resumen {Mathf.Max(1, Turn / Mathf.Max(1, m))}]\n" +
                   $"Temas: {string.Join(", ", topWords)}\n" +
                   string.Join("\n", bullets);
        }

        string PostprocessLLM(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return s;
            s = s.Trim();
            if (s.Length > 1500) s = s.Substring(0, 1500);
            return s;
        }

        void TrimByCharBudget(List<MemoryEntry> list, int maxChars)
        {
            int total = list.Sum(e => e.text.Length);
            if (total <= maxChars) return;

            foreach (string prio in new[] { "ordinary", "important" })
            {
                var ordered = list.Where(e => e.priority == prio)
                                  .OrderBy(e => e.f)
                                  .ToList();
                foreach (var e in ordered)
                {
                    if (total <= maxChars) return;
                    total -= e.text.Length;
                    list.Remove(e);
                }
            }

            var oldest = list.OrderBy(e => e.turnIndex).ToList();
            foreach (var e in oldest)
            {
                if (total <= maxChars) return;
                total -= e.text.Length;
                list.Remove(e);
            }
        }
        public string BuildPrompt(string currentUserText)
        {
            string Ps = string.Join("\n", Room.ShortTerm.Select(e => e.ToString()));
            string Pl = string.Join("\n", Room.LongTerm.Select(e => e.ToString()));
            string Pt = $"User: {currentUserText}";

            return $"[Pm]\n{metaPrompt}\n\n[Ps]\n{Ps}\n\n[Pl]\n{Pl}\n\n[Pt]\n{Pt}";
        }
    }
}