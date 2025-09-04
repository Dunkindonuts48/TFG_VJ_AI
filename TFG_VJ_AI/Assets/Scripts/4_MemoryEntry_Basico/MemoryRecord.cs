using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace TFG.Memory
{
    public enum MemoryType { Working, Episodic, Semantic, Social }

    [Serializable]
    public class MemoryRecord
    {
        public string id;
        public MemoryType type;
        [TextArea] public string content;
        public string[] tags;
        public DateTime timestamp;
        [Range(0f, 1f)] public float importance;
        public int occurrences;
        public float[] embedding;
    }
}
