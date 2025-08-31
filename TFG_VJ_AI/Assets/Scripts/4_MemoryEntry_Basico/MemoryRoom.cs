using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TFG.Memory
{
    public class MemoryRoom
    {
        public readonly List<MemoryEntry> ShortTerm = new();
        public readonly List<MemoryEntry> LongTerm = new();
    }
}

