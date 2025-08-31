using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using UnityEngine.Networking;

namespace TFG.Memory
{
    public static class LLMClient
    {
        // --- OLLAMA CHAT ---
        public static IEnumerator ChatOllama(
            string model,
            string systemPrompt,
            string userPrompt,
            System.Action<string> onDone,
            System.Action<string> onError,
            float timeout = 20f)
        {
            var url = "http://localhost:11434/api/chat";

            var payload = new ChatPayload
            {
                model = model,
                messages = new ChatMessage[]
                {
                    new ChatMessage{ role = "system", content = systemPrompt },
                    new ChatMessage{ role = "user",   content = userPrompt   }
                },
                stream = false
            };

            var json = JsonUtility.ToJson(payload);
            var req = new UnityWebRequest(url, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.timeout = Mathf.RoundToInt(timeout);

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke(req.error);
                yield break;
            }

            var resp = JsonUtility.FromJson<OllamaChatResponse>(req.downloadHandler.text);
            if (resp != null && resp.message != null && !string.IsNullOrWhiteSpace(resp.message.content))
                onDone?.Invoke(resp.message.content);
            else
                onError?.Invoke("Respuesta vacía del LLM (Ollama).");
        }

        [System.Serializable]
        class ChatPayload
        {
            public string model;
            public ChatMessage[] messages;
            public bool stream;
        }

        [System.Serializable]
        class ChatMessage
        {
            public string role;
            public string content;
        }

        [System.Serializable]
        class OllamaChatResponse
        {
            public OllamaMessage message;
        }

        [System.Serializable]
        class OllamaMessage
        {
            public string role;
            public string content;
        }
    }
}
