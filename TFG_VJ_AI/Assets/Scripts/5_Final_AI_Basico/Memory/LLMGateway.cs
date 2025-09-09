using System;
using System.Threading.Tasks;
using TFG.Memory;
using TFG.NPC;
using UnityEngine;

[Serializable]
public class LLMGateway
{
    public TFG.Memory.MemoryRepository repo;
    public bool useOllama = true;
    public string ollamaHost = "http://127.0.0.1:11434";
    public string model = "llama3.1:8b-instruct";
    public float temperature = 0f;
    public int timeoutMs = 7000;

    public LLMGateway(TFG.Memory.MemoryRepository repo) { this.repo = repo; }

    public async Task<SocialIntent> ClassifyIntentAsync(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return SocialIntent.None;

        try
        {
            string prompt =
                "Eres un clasificador de intenciones para un NPC en un videojuego. " +
                "Devuelve SOLO: FollowMe | StopFollow | AttackPlayer | Apologize | None.\n" +
                "La frase puede venir en español o inglés.\n" +
                "Criterios: FollowMe (pedir que el NPC siga al jugador, 'sígueme','follow me'), StopFollow (pedir que el NPC que deje de seguir al jugador, 'para de seguir'), " +
                "AttackPlayer (hostilidad explícita del jugador contra el NPC), Apologize (`Disculpa`, 'sorry', Disculpas o arrepentimiento por parte del jugador contra el NPC).\n\n" +
                "Frase: '" + raw + "'\nEtiqueta:";

            string outText = null;

            if (useOllama)
                outText = await TFG.NPC.OllamaClient.GenerateAsync(ollamaHost, model, prompt, temperature, timeoutMs);
            else
                outText = SafeAskLLM(prompt);

            if (!string.IsNullOrEmpty(outText) && Enum.TryParse<SocialIntent>(outText.Trim(), true, out var intentLLM))
                return intentLLM;
        }
        catch {}
        return ClassifyIntent(raw);
    }

    public SocialIntent ClassifyIntent(string raw)
    {
        var s = raw.ToLowerInvariant();
        if (s.Contains("sigueme") || s.Contains("sígueme") || s.Contains("follow me") || s.Contains("ven conmigo") || s.Contains("acomp"))
            return SocialIntent.FollowMe;
        if (s.Contains("para de seguir") || s.Contains("deja de seguir") || s.Contains("stop following") || s.Contains("dejame") || s.Contains("déjame") || s.Contains("vuelve a lo de antes"))
            return SocialIntent.StopFollow;
        if (s.Contains("te odio") || s.Contains("muere") || s.Contains("te atacar") || s.Contains("te pego") || s.Contains("attack you"))
            return SocialIntent.AttackPlayer;
        if (s.Contains("perdon") || s.Contains("perdón") || s.Contains("lo siento") || s.Contains("disculp") || s.Contains("sorry"))
            return SocialIntent.Apologize;
        return SocialIntent.None;
    }

    private string SafeAskLLM(string prompt) => null;
}