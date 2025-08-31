using OpenCover.Framework.Model;
using System.Collections.Generic;
using System.Linq;


public static class Prompts
{
    public static string IntentPrompt(string playerText, List<MemoryRecord> mem)
    {
        var memories = string.Join("\n- ", mem.Select(m => m.content));

        return $@"Eres el NLU de un NPC. Devuelve SOLO un JSON válido EXACTO al schema:
{{
  ""intent"": ""FollowPlayer|Trade|GiveItem|AskInfo|Gossip|None"",
  ""slots"": {{}},
  ""goalDelta"": [{{""goal"": ""string"", ""utilityDelta"": 0}}],
  ""memoryWrites"": [{{""type"": ""string"", ""content"": ""string"", ""tags"": [], ""importance"": 0}}],
  ""npcReply"": ""string""
}}

Ejemplo de salida válido:
{{""intent"":""FollowPlayer"",""slots"":{{}},""goalDelta"":[{{""goal"":""FollowPlayer"",""utilityDelta"":2}}],""memoryWrites"":[{{""type"":""Social"",""content"":""El jugador pidió que le siguiera"",""tags"":[""jugador"",""dialogo""],""importance"":0.6}}],""npcReply"":""¡Voy contigo!""}}

Contexto (memorias):
- {memories}

Entrada del jugador: ""{playerText}""
Responde SOLO con el JSON sin texto adicional ni explicaciones.";
    }

    public static string EpisodePrompt(string rawEvent)
    {
        return $@"Resume el evento en una sola frase y etiqueta.
Devuelve JSON: {{""summary"": ""string"", ""tags"": [], ""importance"": 0}}
Evento: {rawEvent}";
    }
}