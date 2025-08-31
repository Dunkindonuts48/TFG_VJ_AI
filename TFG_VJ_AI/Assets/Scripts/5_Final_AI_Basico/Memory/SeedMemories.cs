using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SeedMemories : MonoBehaviour
{
    public MemoryRepository mem;

    [TextArea]
    public string[] seedContents = {
        "El mercado abre de día",
        "Ayer el jugador me regaló pan"
    };
    public string[][] seedTags = {
        new [] { "mercado", "horario" },
        new [] { "jugador", "pan", "agradecimiento" }
    };
    public MemoryType[] seedTypes = {
        MemoryType.Semantic,
        MemoryType.Social
    };


    void Awake()
    {
        if (!mem) mem = GetComponent<MemoryRepository>();
        if (!mem) { Debug.LogWarning("[SeedMemories] No hay MemoryRepository en este GameObject."); return; }

        int n = Mathf.Min(seedContents.Length, Mathf.Min(seedTags.Length, seedTypes.Length));
        for (int i = 0; i < n; i++)
        {
            var rec = new MemoryRecord
            {
                id = Guid.NewGuid().ToString(),
                type = seedTypes[i],
                content = seedContents[i],
                tags = seedTags[i],
                timestamp = DateTime.UtcNow.AddHours(-i * 3),
                importance = 0.6f,
                occurrences = 1
            };
            mem.Remember(rec);
        }
    }
}
