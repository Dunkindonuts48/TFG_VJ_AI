using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace TFG.Memory
{
    [Serializable] class EmbReq {public string model; public string input;}
    [Serializable] class EmbRes {public float[][] embeddings;}
    [Serializable] class GenReq { public string model; public string prompt; public bool stream = false;}
    [Serializable] class GenRes { public string response;}

    public static class OllamaClient
    {
        const string ollamaHost = "http://localhost:11434";

        static async Task<string> PostJson(string url, string json)
        {
            var req = new UnityWebRequest(url, "POST");
            var body = Encoding.UTF8.GetBytes(json);
            req.uploadHandler = new UploadHandlerRaw(body);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            var op = req.SendWebRequest();
            while (!op.isDone) await Task.Yield();
#if UNITY_2020_2_OR_NEWER
            if (req.result != UnityWebRequest.Result.Success)
#else
            if (req.isNetworkError || req.isHttpError)
#endif
                throw new Exception($"HTTP {req.responseCode}: {req.error}");
            return req.downloadHandler.text;
        }

        public static async Task<float[]> EmbedAsync(string text, string model = "mxbai-embed-large")
        {
            var json = JsonUtility.ToJson(new EmbReq { model = model, input = text });
            var res = await PostJson($"{ollamaHost}/api/embeddings", json);
            var data = JsonUtility.FromJson<EmbRes>(res);
            return (data != null && data.embeddings != null && data.embeddings.Length > 0) ? data.embeddings[0] : Array.Empty<float>();
        }

        public static async Task<string> GenerateAsync(string prompt, string model = "mistral:7b-instruct")
        {
            var json = JsonUtility.ToJson(new GenReq { model = model, prompt = prompt, stream = false });
            var res = await PostJson($"{ollamaHost}/api/generate", json);
            var data = JsonUtility.FromJson<GenRes>(res);
            return data != null ? data.response : "";
        }
    }
}
