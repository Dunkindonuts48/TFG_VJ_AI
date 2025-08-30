using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TFG.Memory
{
    public static class MemoryRetrieval
    {
        static Dictionary<string, int> Vocab(string s)
        {
            var dict = new Dictionary<string, int>();
            foreach (var w in s.ToLowerInvariant().Split(new[] { ' ', '\t', '\n', '\r', '.', ',', ';', ':', '!', '?', '\"', '\'' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (w.Length < 3) continue;
                dict[w] = dict.TryGetValue(w, out var c) ? c + 1 : 1;
            }
            return dict;
        }

        static float Cosine(Dictionary<string, int> a, Dictionary<string, int> b)
        {
            var keys = a.Keys.Union(b.Keys);
            float dot = 0, na = 0, nb = 0;
            foreach (var k in keys)
            {
                int va = a.TryGetValue(k, out var aa) ? aa : 0;
                int vb = b.TryGetValue(k, out var bb) ? bb : 0;
                dot += va * vb;
                na += va * va;
                nb += vb * vb;
            }
            if (na == 0 || nb == 0) return 0;
            return dot / (Mathf.Sqrt(na) * Mathf.Sqrt(nb));
        }

        public static List<MemoryEntry> Retrieve(MemoryRoom room, string query, int k = 6)
        {
            var qv = Vocab(query);
            var all = room.ShortTerm.Concat(room.LongTerm).ToList();
            float PrioWeight(string p) => p == "forever" ? 1.4f : (p == "important" ? 1.15f : 1.0f);

            return all
                .Select(e => new { e, score = Cosine(qv, Vocab(e.text)) * PrioWeight(e.priority) * Mathf.Max(0.1f, e.f) })
                .OrderByDescending(x => x.score)
                .Take(k)
                .Select(x => x.e)
                .ToList();
        }
    }
}
