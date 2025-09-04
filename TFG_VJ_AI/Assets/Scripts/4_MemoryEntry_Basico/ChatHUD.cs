using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace TFG.Memory
{
    public class ChatHUD : MonoBehaviour
    {
        [Header("Refs")]
        public DialogueAgent agent;
        public TMP_InputField input;
        public TMP_Text lastExchangeText;
        public TMP_Text countersText;
        public bool autoFocusInput = true;

        void Awake() { if (!agent) agent = FindObjectOfType<DialogueAgent>(); }
        void Start() { RefreshCounters(); }

        public async void OnSend()
        {
            if (!agent) return;

            var text = (input && !string.IsNullOrWhiteSpace(input.text)) ? input.text : "hola";
            var reply = await agent.TalkAsync(text);

            if (lastExchangeText) lastExchangeText.text = $"> {text}\n< {reply}";
            if (input) { input.text = ""; if (autoFocusInput) input.ActivateInputField(); }

            RefreshCounters();
        }

        public void RefreshCounters()
        {
            if (!agent || countersText == null) return;
            countersText.text = $"ST: {agent.ShortTermPairs}  |  LT summaries: {agent.LongTermSummariesCount}  |  Mems: {agent.TotalMemories}";
        }
    }
}
