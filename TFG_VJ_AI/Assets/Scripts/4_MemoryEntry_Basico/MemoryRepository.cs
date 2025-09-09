using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;


namespace TFG.Memory
{
    public class MemoryRepository : MonoBehaviour
    {
        [Range(0, 1)] public float wSimilarity = 0.45f, wRecency = 0.25f, wImportance = 0.20f, wFrequency = 0.10f;
        public string fileName = "memories.json";
        public bool loadOnAwake = true;
        public bool autoSave = true;
        public float autosaveEverySeconds = 10f;
        public event Action OnChanged;

        readonly List<MemoryRecord> all = new List<MemoryRecord>();
        public IReadOnlyList<MemoryRecord> AllReadOnly => all;

        float _t;

        void Awake()
        {
            if (loadOnAwake)
            {
                var loaded = MemPersistence.Load(fileName);
                all.Clear(); all.AddRange(loaded);
                Debug.Log($"[MemoryRepository] Cargadas: {all.Count}. Ruta: {MemPersistence.GetAbsolutePath(fileName)}");
            }
        }

        void Update()
        {
            if (!autoSave) return;
            _t += Time.deltaTime;
            if (_t >= autosaveEverySeconds) { SaveNow(); _t = 0f; }
        }

        void OnApplicationQuit() { if (autoSave) SaveNow(); }
        void OnDisable() { if (autoSave) SaveNow(); }

        public void SaveNow()
        {
            MemPersistence.Save(fileName, all);
        }
        public void Remember(MemoryRecord r) {
            all.Add(r); OnChanged?.Invoke(); 
        }
        public void UpdateRecord(int index, MemoryRecord r) {
            if (index >= 0 && index < all.Count) { all[index] = r; OnChanged?.Invoke(); }
        }
        public void RemoveAt(int index) {
            if (index >= 0 && index < all.Count) { all.RemoveAt(index); OnChanged?.Invoke(); }
        }
        public void ClearAll() {
            all.Clear(); 
        }
        float Cosine(float[] a, float[] b)
        {
            if (a == null || b == null || a.Length != b.Length) return 0f;
            double dot = 0, na = 0, nb = 0;
            for (int i = 0; i < a.Length; i++)
            {
                dot += a[i] * b[i];
                na += a[i] * a[i];
                nb += b[i] * b[i];
            }
            return (float)(dot / (Math.Sqrt(na) * Math.Sqrt(nb) + 1e-8));
        }

        float TagSimilarity(string[] a, string[] b)
        {
            if (a == null || b == null || a.Length == 0 || b.Length == 0) return 0f;
            var inter = a.Intersect(b).Count(); var uni = a.Union(b).Count();
            return uni == 0 ? 0f : (float)inter / uni;
        }
        float Recency(MemoryRecord r)
        {
            var hours = (float)(DateTime.UtcNow - r.timestamp.ToUniversalTime()).TotalHours;
            return 1f / (1f + hours / 12f);
        }
        float Frequency(MemoryRecord r) => Mathf.Clamp01(r.occurrences / 10f);
        float Score(MemoryRecord r, string[] queryTags, float[] emb)
        {
            var sim = (emb != null && r.embedding != null) ? Cosine(r.embedding, emb) : 0f;
            var tSim = TagSimilarity(r.tags, queryTags);
            return wSimilarity * (0.7f * sim + 0.3f * tSim)
                 + wRecency * Recency(r)
                 + wImportance * r.importance
                 + wFrequency * Frequency(r);
        }
        public List<MemoryRecord> Recall(string[] queryTags, float[] emb, int k = 5)
        {
            return all.OrderByDescending(r => Score(r, queryTags, emb)).Take(Mathf.Max(1, k)).ToList();
        }
    }
}
