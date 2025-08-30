using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TFG.Memory
{
    public class NPCMemoryAgent : MonoBehaviour
    {
        public MemoryRepository repo;

        [Header("Respuesta del NPC")]
        public bool useLLMForReplies = false;
        public string replyModel = "mistral:7b-instruct";
        public float replyTimeoutSeconds = 20f;

        [TextArea(2, 5)] public string lastUserInput;
        [TextArea(3, 8)] public string lastAgentReply;
        [TextArea(8, 12)] public string lastPromptBuilt;

        void Awake()
        {
            if (!repo) repo = FindObjectOfType<MemoryRepository>();
        }

        [ContextMenu("Simular Turno")]
        public void SimulateDialogueTurn()
        {
            if (string.IsNullOrWhiteSpace(lastUserInput))
                lastUserInput = "Hola, ¿recuerdas lo que hablamos?";

            repo.AddTalk("User", lastUserInput);
            var rel = MemoryRetrieval.Retrieve(repo.Room, lastUserInput, 5);

            lastPromptBuilt = repo.BuildPrompt(lastUserInput);

            if (useLLMForReplies)
            {
                GenerateReplyLLM(lastUserInput, rel);
            }
            else
            {
                lastAgentReply = GenerateReplyMock(lastUserInput, rel);
                repo.AddTalk("NPC", lastAgentReply);
            }

            lastUserInput = "";
        }

        string GenerateReplyMock(string user, System.Collections.Generic.List<MemoryEntry> rel)
        {
            var ctx = string.Join(" | ", rel.Select(e => e.text));
            return $"Basándome en nuestras memorias: {ctx}\nMi respuesta: {user} → te diría que sí, y añadiría detalles recordados.";
        }

        void GenerateReplyLLM(string user, System.Collections.Generic.List<MemoryEntry> rel)
        {
            string system = "Eres un NPC de videojuego que responde en español de forma breve, clara y empática. Usa las memorias recuperadas si son útiles, no inventes.";
            string memories = string.Join("\n- ", rel.Select(e => e.ToString()));
            string userPrompt = $@"Contexto de memoria relevante: - {memories} Usuario dice: {user} Responde en 1-3 frases, naturales y concretas, usando el contexto si aplica.";
            string result = null;
            bool finished = false;
            bool failed = false;

            var go = new GameObject("LLMReplyRunner");
            var runner = go.AddComponent<MonoBehaviourRunner>();

            runner.StartCoroutine(LLMClient.ChatOllama(
                string.IsNullOrWhiteSpace(replyModel) ? "mistral:7b-instruct" : replyModel,
                system,
                userPrompt,
                s => { result = s; finished = true; },
                e => { Debug.LogWarning("[LLM-Reply] " + e); failed = true; finished = true; },
                replyTimeoutSeconds
            ));

            float t = 0f, TMAX = Mathf.Max(3f, replyTimeoutSeconds);
            while (!finished && t < TMAX) { t += Time.deltaTime; }

            Destroy(go);

            if (failed || string.IsNullOrWhiteSpace(result))
            {
                lastAgentReply = GenerateReplyMock(user, rel);
            }
            else
            {
                lastAgentReply = PostprocessLLM(result);
            }

            repo.AddTalk("NPC", lastAgentReply);
        }

        string PostprocessLLM(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return s;
            s = s.Trim();
            if (s.Length > 1000) s = s.Substring(0, 1000);
            return s;
        }
    }
}
