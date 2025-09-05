using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace TFG.NPC
{
    public class OllamaClient : MonoBehaviour
    {
        private static readonly HttpClient http = new HttpClient();

        public static async Task<string> GenerateAsync(
            string host,
            string model,
            string prompt,
            float temperature = 0f,
            int timeoutMs = 7000)
        {
            http.Timeout = TimeSpan.FromMilliseconds(timeoutMs);

            string payload = "{"
                + $"\"model\":\"{Escape(model)}\","
                + $"\"prompt\":\"{Escape(prompt)}\","
                + "\"stream\":false,"
                + "\"options\":{"
                + $"\"temperature\":{temperature.ToString(System.Globalization.CultureInfo.InvariantCulture)}"
                + "}"
                + "}";

            var req = new HttpRequestMessage(HttpMethod.Post, host.TrimEnd('/') + "/api/generate");
            req.Content = new StringContent(payload, Encoding.UTF8, "application/json");

            using var res = await http.SendAsync(req);
            var text = await res.Content.ReadAsStringAsync();
            if (!res.IsSuccessStatusCode)
                throw new Exception($"Ollama HTTP {(int)res.StatusCode}: {text}");

            var m = Regex.Match(text, "\"response\"\\s*:\\s*\"(.*?)\"", RegexOptions.Singleline);
            if (!m.Success) return null;

            string captured = m.Groups[1].Value;
            captured = captured.Replace("\\n", "\n").Replace("\\\"", "\"");
            return captured.Trim();
        }

        private static string Escape(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "");
        }
    }
}
