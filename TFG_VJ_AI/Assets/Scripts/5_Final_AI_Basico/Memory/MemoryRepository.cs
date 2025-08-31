using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class MemoryRepository : MonoBehaviour
{
    [Header("Scoring")]
    [Range(0, 1)] public float wSimilarity = 0.45f;
    [Range(0, 1)] public float wRecency = 0.25f;
    [Range(0, 1)] public float wImportance = 0.20f;
    [Range(0, 1)] public float wFrequency = 0.10f;

    [Header("Persistencia")]
    public string fileName = "memories.json";
    public bool loadOnAwake = true;
    public bool autoSave = true;
    public float autosaveEverySeconds = 10f;

    [Header("Debug")]
    public bool logChanges = true;

    readonly List<MemoryRecord> _all = new List<MemoryRecord>();
    public IReadOnlyList<MemoryRecord> AllReadOnly => _all;

    public event Action OnChanged;

    float _autosaveTimer;

    void Awake()
    {
        if (loadOnAwake)
        {
            var loaded = MemPersistence.Load(fileName);
            _all.Clear();
            _all.AddRange(loaded);
            if (logChanges) Debug.Log($"[MemoryRepository] Cargadas: {_all.Count}. Ruta: {MemPersistence.GetAbsolutePath(fileName)}");
            OnChanged?.Invoke();
        }
    }

    void Update()
    {
        if (!autoSave) return;
        _autosaveTimer += Time.deltaTime;
        if (_autosaveTimer >= autosaveEverySeconds)
        {
            SaveNow();
            _autosaveTimer = 0f;
        }
    }

    void OnApplicationQuit() { if (autoSave) SaveNow(); }
    void OnDisable() { if (autoSave) SaveNow(); }

    public void SaveNow() => MemPersistence.Save(fileName, _all);

    public void Remember(MemoryRecord r)
    {
        _all.Add(r);
        OnChanged?.Invoke();
    }

    public void UpdateRecord(int index, MemoryRecord updated)
    {
        if (index < 0 || index >= _all.Count) return;
        _all[index] = updated;
        if (logChanges) Debug.Log($"[MemoryRepository] ~Update[{index}]");
        OnChanged?.Invoke();
    }

    public void RemoveAt(int index)
    {
        if (index < 0 || index >= _all.Count) return;
        if (logChanges) Debug.Log($"[MemoryRepository] -Remove[{index}] {_all[index].content}");
        _all.RemoveAt(index);
        OnChanged?.Invoke();
    }

    public void ClearAll()
    {
        _all.Clear();
        if (logChanges) Debug.Log("[MemoryRepository] ClearAll");
        OnChanged?.Invoke();
    }

    // ===== Recall =====
    float Cosine(float[] a, float[] b)
    {
        if (a == null || b == null || a.Length != b.Length) return 0f;
        double dot = 0, na = 0, nb = 0;
        for (int i = 0; i < a.Length; i++) { dot += a[i] * b[i]; na += a[i] * a[i]; nb += b[i] * b[i]; }
        return (float)(dot / (Math.Sqrt(na) * Math.Sqrt(nb) + 1e-8));
    }

    float TagSimilarity(string[] a, string[] b)
    {
        if (a == null || b == null || a.Length == 0 || b.Length == 0) return 0f;
        var inter = a.Intersect(b).Count();
        var uni = a.Union(b).Count();
        return uni == 0 ? 0f : (float)inter / uni;
    }

    float Recency(MemoryRecord r)
    {
        var hours = (float)(DateTime.UtcNow - r.timestamp.ToUniversalTime()).TotalHours;
        return 1f / (1f + hours / 12f);
    }

    float Frequency(MemoryRecord r) => Mathf.Clamp01(r.occurrences / 10f);

    float Score(MemoryRecord r, string[] queryTags, float[] queryEmbedding)
    {
        var sim = (queryEmbedding != null && r.embedding != null) ? Cosine(r.embedding, queryEmbedding) : 0f;
        var tSim = TagSimilarity(r.tags, queryTags);
        return wSimilarity * (0.7f * sim + 0.3f * tSim)
             + wRecency * Recency(r)
             + wImportance * r.importance
             + wFrequency * Frequency(r);
    }

    public List<MemoryRecord> Recall(string[] queryTags, float[] queryEmbedding, int k = 5)
    {
        return _all.OrderByDescending(r => Score(r, queryTags, queryEmbedding)).Take(Mathf.Max(1, k)).ToList();
    }
}