using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

[Serializable]
public class MemoryRecordDTO
{
    public string id;
    public string type;
    public string content;
    public string[] tags;
    public string timestampIso;
    public float importance;
    public int occurrences;
    public float[] embedding;
}

public static class MemPersistence
{
    static string FilePath(string fileName) =>
        Path.Combine(Application.persistentDataPath, fileName);

    static MemoryRecordDTO ToDTO(MemoryRecord r) => new MemoryRecordDTO
    {
        id = r.id,
        type = r.type.ToString(),
        content = r.content,
        tags = r.tags,
        timestampIso = r.timestamp.ToUniversalTime().ToString("o"),
        importance = r.importance,
        occurrences = r.occurrences,
        embedding = r.embedding
    };

    static MemoryRecord FromDTO(MemoryRecordDTO d) => new MemoryRecord
    {
        id = string.IsNullOrEmpty(d.id) ? Guid.NewGuid().ToString() : d.id,
        type = Enum.TryParse<MemoryType>(d.type, true, out var t) ? t : MemoryType.Semantic,
        content = d.content ?? "",
        tags = d.tags ?? Array.Empty<string>(),
        timestamp = DateTime.TryParse(d.timestampIso, null, System.Globalization.DateTimeStyles.AdjustToUniversal, out var ts)
                    ? ts : DateTime.UtcNow,
        importance = Mathf.Clamp01(d.importance),
        occurrences = Mathf.Max(0, d.occurrences),
        embedding = d.embedding
    };

    [Serializable] class Wrapper<T> { public T[] items; }

    public static void Save(string fileName, List<MemoryRecord> all)
    {
        try
        {
            var dtoList = new List<MemoryRecordDTO>();
            foreach (var r in all) dtoList.Add(ToDTO(r));
            var wrap = new Wrapper<MemoryRecordDTO> { items = dtoList.ToArray() };
            var json = JsonUtility.ToJson(wrap, true);
            File.WriteAllText(FilePath(fileName), json);
            Debug.Log($"[MemPersistence] Guardado {dtoList.Count} recuerdos → {FilePath(fileName)}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[MemPersistence] Error al guardar: {e.Message}");
        }
    }

    public static List<MemoryRecord> Load(string fileName)
    {
        var list = new List<MemoryRecord>();
        var path = FilePath(fileName);
        try
        {
            if (!File.Exists(path)) return list;
            var json = File.ReadAllText(path);
            var wrap = JsonUtility.FromJson<Wrapper<MemoryRecordDTO>>(json);
            if (wrap?.items != null)
            {
                foreach (var dto in wrap.items) list.Add(FromDTO(dto));
            }
            Debug.Log($"[MemPersistence] Cargado {list.Count} recuerdos ← {path}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[MemPersistence] Error al cargar desde {path}: {e.Message}");
        }
        return list;
    }

    public static bool Exists(string fileName) => File.Exists(FilePath(fileName));
    public static string GetAbsolutePath(string fileName) => FilePath(fileName);
}