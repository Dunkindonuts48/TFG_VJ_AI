using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace TFG.Memory
{
    public class MemoryHUD : MonoBehaviour
    {
        public MemoryRepository repo;
        public NPCMemoryAgent agent;

        [Header("UI")]
        public Text txtCounters;
        public Text txtLastSummary;
        public InputField inputUser;
        public Button btnSimulate;

        void Start()
        {
            if (!repo) repo = FindObjectOfType<MemoryRepository>();
            if (!agent) agent = FindObjectOfType<NPCMemoryAgent>();

            if (btnSimulate != null)
            {
                btnSimulate.onClick.RemoveAllListeners();
                btnSimulate.onClick.AddListener(() =>
                {
                    agent.lastUserInput = inputUser != null ? inputUser.text : "Hola";
                    agent.SimulateDialogueTurn();
                    if (inputUser != null) inputUser.text = "";
                    Refresh();
                });
            }

            if (repo != null) repo.OnMemoryUpdated += Refresh;
            Refresh();
        }

        void Refresh()
        {
            if (repo == null || txtCounters == null) return;

            int st = repo.Room.ShortTerm.Count;
            int lt = repo.Room.LongTerm.Count;

            txtCounters.text = $"Turno: {repo.Turn}\nShort-Term: {st}  |  Long-Term: {lt}";

            if (txtLastSummary != null)
            {
                var lastSum = repo.Room.LongTerm.LastOrDefault(e => e.speaker == "Summary");
                txtLastSummary.text = lastSum != null ? $"Último resumen:\n{lastSum.text}" : "Sin resúmenes aún.";
            }
        }
    }
}
