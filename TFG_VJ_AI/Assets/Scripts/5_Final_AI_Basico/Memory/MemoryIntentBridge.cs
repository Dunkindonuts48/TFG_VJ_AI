using System;
using UnityEngine;
using TFG.Memory;
using System.Threading.Tasks;

namespace TFG.NPC
{
    public class MemoryIntentBridge : MonoBehaviour
    {
        [Header("Refs")]
        public MemoryRepository memory;
        public LLMGateway gateway;
        public SocialBlackboard bb = new SocialBlackboard();

        [Header("Chat (opcional)")]
        public DialogueAgent agent;
        public bool chatWhenCommand = true;
        public bool chatWhenNoCommand = true;

        [Header("Log")]
        public bool verbose = true;

        void Awake()
        {
            if (gateway == null) gateway = new LLMGateway(memory);
            if (!agent) agent = FindObjectOfType<DialogueAgent>();
        }

        public void OnPlayerUtterance(string text) => _ = OnPlayerUtteranceAsync(text);

        public async Task<string> OnPlayerUtteranceAsync(string text)
        {
            var intent = (gateway != null)
                ? await gateway.ClassifyIntentAsync(text)
                : SocialIntent.None;

            if (verbose) Debug.Log($"[MemoryIntentBridge] Intent -> {intent} (from: '{text}')");

            TryStoreMemory($"player_said::{DateTime.UtcNow:O}", intent.ToString(), text);

            switch (intent)
            {
                case SocialIntent.FollowMe:
                    bb.followingPlayer = true;
                    bb.attackPlayer = false;
                    bb.pacified = true;
                    break;
                case SocialIntent.StopFollow:
                    bb.followingPlayer = false;
                    bb.attackPlayer = false;
                    break;
                case SocialIntent.AttackPlayer:
                    bb.playerHostile = true;
                    bb.attackPlayer = true;
                    bb.followingPlayer = false;
                    bb.pacified = false;
                    break;
                case SocialIntent.Apologize:
                    bb.pacified = true;
                    bb.playerHostile = false;
                    bb.attackPlayer = false;
                    break;
                case SocialIntent.None:
                    break;
            }

            SendMessage("OnSocialBlackboardChanged", bb, SendMessageOptions.DontRequireReceiver);

            bool isCommand = intent != SocialIntent.None;
            bool shouldChat = (isCommand && chatWhenCommand) || (!isCommand && chatWhenNoCommand);

            string reply = null;
            if (shouldChat && agent != null)
            {
                reply = await agent.TalkAsync(text);
            }
            return reply;
        }

        void TryStoreMemory(string id, string tag, string content)
        {
            try
            {
                if (!memory) return;
                var rec = new MemoryRecord
                {
                    id = id,
                    type = MemoryType.Social,
                    tags = new[] { tag },
                    content = content,
                    timestamp = DateTime.UtcNow,
                    importance = 0.5f,
                    occurrences = 1
                };

                int idx = -1;
                var list = memory.AllReadOnly;
                for (int i = 0; i < list.Count; i++)
                    if (list[i].id == id) { idx = i; break; }

                if (idx >= 0) memory.UpdateRecord(idx, rec);
                else memory.Remember(rec);
            }
            catch (Exception e)
            {
                if (verbose) Debug.LogWarning($"No se pudo guardar en memoria: {e.Message}");
            }
        }
    }
}
