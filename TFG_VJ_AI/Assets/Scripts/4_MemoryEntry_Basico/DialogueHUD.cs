using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using UnityEngine.UI;

namespace TFG.Memory
{
    public class DialogueHUD : MonoBehaviour
    {
        [Header("Refs")]
        public MemoryRepository repo;
        public NPCMemoryAgent agent;

        [Header("UI")]
        public InputField inputUser;
        public Button btnSimular;

        [Tooltip("Muestra la última respuesta del NPC")]
        public Text txtLastReply;

        [Tooltip("Área de historial (multilínea)")]
        public Text txtHistory;

        [Tooltip("Cuántas líneas máx. mostrar en historial")]
        public int historyMaxLines = 30;

        void Start()
        {
            if (!repo) repo = FindObjectOfType<MemoryRepository>();
            if (!agent) agent = FindObjectOfType<NPCMemoryAgent>();

            if (btnSimular != null)
            {
                btnSimular.onClick.RemoveAllListeners();
                btnSimular.onClick.AddListener(OnClickSimulate);
            }

            if (repo != null) repo.OnMemoryUpdated += Refresh;
            Refresh();
        }

        void OnClickSimulate()
        {
            var text = (inputUser != null && !string.IsNullOrWhiteSpace(inputUser.text))
                ? inputUser.text
                : "Hola";

            agent.lastUserInput = text;
            agent.SimulateDialogueTurn();

            if (inputUser != null) inputUser.text = "";
            Refresh();
        }

        void Refresh()
        {
            if (txtLastReply != null && agent != null)
                txtLastReply.text = string.IsNullOrWhiteSpace(agent.lastAgentReply)
                    ? "NPC: (sin respuesta aún)"
                    : $"NPC: {agent.lastAgentReply}";

            if (txtHistory != null && repo != null)
            {
                var all = repo.Room.ShortTerm.Concat(repo.Room.LongTerm)
                    .OrderByDescending(e => e.turnIndex)
                    .Take(historyMaxLines)
                    .OrderBy(e => e.turnIndex)
                    .ToList();

                var sb = new StringBuilder();
                foreach (var e in all)
                {
                    if (e.speaker == "User" || e.speaker == "NPC")
                        sb.AppendLine($"{e.speaker}: {e.text}");
                }

                txtHistory.text = sb.Length > 0 ? sb.ToString() : "(historial vacío)";
            }
        }
    }
}
