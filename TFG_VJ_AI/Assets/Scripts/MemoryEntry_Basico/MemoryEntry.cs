using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace TFG.Memory
{
    [Serializable]
    public class MemoryEntry
    {
        public string speaker;
        public string text;
        public int turnIndex;
        public float f;
        public int repeats;
        public string priority;

        public MemoryEntry(string speaker, string text, int turnIndex)
        {
            this.speaker = speaker;
            this.text = text;
            this.turnIndex = turnIndex;
            this.f = 1f;
            this.repeats = 1;
            this.priority = "ordinary";
        }

        public override string ToString() => $"[{turnIndex}] {speaker}: {text}";
    }
}
