using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Importa TODO el lore desde un TextAsset (.txt) en fragmentos, los resume con el LLM (Ollama)
/// y los guarda como MemoryRecord en el MemoryRepository. Opcionalmente genera embeddings.
///
/// Uso:
/// 1) Crea un GameObject en la escena y añade este componente.
/// 2) Asigna un TextAsset (tu .txt) en "textFile" y el MemoryRepository en "repo".
/// 3) Marca runOnAwake para importar al iniciar. También puedes usar el ContextMenu "Import Now".
/// 4) Si quieres embeddings, activa generateEmbeddings y ajusta embeddingsModel.
///
/// Requisitos: MemoryRepository, OllamaClient con GenerateAsync (modelo chat) y EmbedAsync (modelo de embeddings).
/// </summary>
public class LoreImporterFromText : MonoBehaviour
{
    [Header("Refs")]
    public TextAsset textFile;
    public MemoryRepository repo;

    [Header("Ejecución")]
    public bool runOnAwake = true;
    public bool skipIfRepoHasData = true;

    [Header("Segmentación del texto")]
    [Range(200, 3000)] public int charsPerChunk = 900;
    public bool keepHeadingsAsTags = true;

    [Header("Resumen con LLM")]
    [TextArea(3, 6)]
    public string summarizationInstruction =
        "Resume el siguiente fragmento del lore de Sonic Unleashed en UNA sola frase clara y concisa, en español, " +
        "sin adornos, sin listas, manteniendo nombres propios. Si ya es una frase, devuélvela tal cual.";
    public string llmModel = "mistral:7b-instruct"; 

    [Header("Embeddings (opcional)")]
    public bool generateEmbeddings = false;
    public string embeddingsModel = "mxbai-embed-large";

    [Header("Etiquetas/Importancia por defecto")]
    public string[] defaultTags = new[] { "lore", "historia" };
    [Range(0f, 1f)] public float defaultImportance = 0.7f;

    [Header("Rendimiento")]
    public int yieldEveryNRecords = 6;

    readonly List<string> _contextTags = new();

    void Reset()
    {
        if (!repo) repo = GetComponent<MemoryRepository>();
    }

    async void Awake()
    {
        if (!repo) repo = GetComponent<MemoryRepository>();
        if (!repo)
        {
            Debug.LogWarning("[LoreImporterFromText] No hay MemoryRepository asignado ni en el mismo GO.");
            return;
        }
        if (!textFile)
        {
            Debug.LogWarning("[LoreImporterFromText] Falta TextAsset (.txt). Asigna tu lore.");
            return;
        }

        if (runOnAwake && (!skipIfRepoHasData || repo.AllReadOnly.Count == 0))
        {
            await ImportNow();
        }
    }

    [ContextMenu("Import Now")]
    public async Task ImportNow()
    {
        if (textFile == null || string.IsNullOrWhiteSpace(textFile.text))
        {
            Debug.LogWarning("[LoreImporterFromText] El archivo de texto está vacío.");
            return;
        }

        _contextTags.Clear();
        var chunks = ChunkFromText(textFile.text, charsPerChunk, keepHeadingsAsTags);
        Debug.Log($"[LoreImporterFromText] Fragmentos a procesar: {chunks.Count}");

        int inserted = 0;
        for (int i = 0; i < chunks.Count; i++)
        {
            var chunk = chunks[i];
            var tags = BuildTagsForChunk();

            string summary = chunk;
            try
            {
                var prompt = summarizationInstruction + "\n\n\"" + chunk + "\"";
                summary = await OllamaClient.GenerateAsync(prompt, llmModel);
                summary = Clean(summary);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[LoreImporterFromText] Error LLM en chunk {i}: {e.Message}");
            }

            float[] embedding = null;
            if (generateEmbeddings)
            {
                try { embedding = await OllamaClient.EmbedAsync(summary, embeddingsModel); }
                catch (Exception e) { Debug.LogWarning($"[LoreImporterFromText] Error embedding: {e.Message}"); }
            }

            var rec = new MemoryRecord
            {
                id = Guid.NewGuid().ToString(),
                type = MemoryType.Semantic,
                content = summary,
                tags = tags,
                timestamp = DateTime.UtcNow,
                importance = defaultImportance,
                occurrences = 1,
                embedding = embedding
            };

            repo.Remember(rec);
            inserted++;

            if (yieldEveryNRecords > 0 && (inserted % yieldEveryNRecords == 0))
                await Task.Yield();
        }

        repo.SaveNow();
        Debug.Log($"[LoreImporterFromText] Importación completada: {inserted} memorias añadidas.");
    }

    // ---- Utilidades ----

    List<string> ChunkFromText(string raw, int size, bool headingsToTags)
    {
        var lines = raw.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
        var chunks = new List<string>();
        var buf = string.Empty;

        foreach (var l in lines)
        {
            var line = l.Trim();
            if (headingsToTags && line.StartsWith("##"))
            {
                var tag = line.TrimStart('#', ' ').ToLowerInvariant();
                if (!string.IsNullOrWhiteSpace(tag)) _contextTags.Add(tag);
                continue;
            }

            if (string.IsNullOrWhiteSpace(line))
            {
                if (buf.Length > 0 && buf.Length >= size * 0.8f)
                {
                    chunks.Add(buf);
                    buf = string.Empty;
                }
                continue;
            }

            if (buf.Length + line.Length + 1 <= size)
            {
                buf = string.IsNullOrEmpty(buf) ? line : (buf + " " + line);
            }
            else
            {
                if (!string.IsNullOrEmpty(buf)) chunks.Add(buf);
                buf = line;
            }
        }

        if (!string.IsNullOrEmpty(buf)) chunks.Add(buf);
        return chunks;
    }

    string[] BuildTagsForChunk()
    {
        if (_contextTags.Count == 0) return defaultTags?.ToArray() ?? Array.Empty<string>();
        var merged = new List<string>();
        if (defaultTags != null) merged.AddRange(defaultTags);
        merged.AddRange(_contextTags);
        return merged.Select(t => t.Trim().ToLowerInvariant())
                     .Where(t => t.Length > 0)
                     .Distinct()
                     .ToArray();
    }

    string Clean(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
        raw = raw.Trim();
        if (raw.StartsWith("- ")) raw = raw.Substring(2).Trim();
        if (raw.StartsWith("* ")) raw = raw.Substring(2).Trim();
        if (raw.StartsWith("\"") && raw.EndsWith("\"")) raw = raw.Substring(1, raw.Length - 2).Trim();
        while (raw.Contains("  ")) raw = raw.Replace("  ", " ");
        return raw;
    }
}