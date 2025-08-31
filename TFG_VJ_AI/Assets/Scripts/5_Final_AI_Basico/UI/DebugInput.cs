using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DebugInput : MonoBehaviour
{
    [Header("Refs UI")]
    public DialogueController dialogue;
    public TMP_InputField input;
    public TMP_Text output;

    [Header("Opcional: auto prueba al iniciar")]
    public bool showOnlyLastQA = true;
    public bool runSmokeOnStart = false;
    public int keepLastLines = 2;

    readonly List<string> history = new();

    async void Start()
    {
        if (runSmokeOnStart && dialogue != null)
        {
            var reply = await dialogue.OnPlayerSays("hola");
            LogBoth($"> hola\n< {reply}");
        }
    }

    public async void OnSend()
    {
        if (dialogue == null) { Debug.LogWarning("[DebugInput] Falta DialogueController."); return; }

        var txt = (input != null && !string.IsNullOrWhiteSpace(input.text)) ? input.text : "hola";
        LogBoth($"> {txt}");

        var reply = await dialogue.OnPlayerSays(txt);

        LogBoth($"< {reply}");

        if (input) input.text = string.Empty;
    }

    async System.Threading.Tasks.Task ShowReply(string userText)
    {
        AddLine($"> {userText}");
        var reply = await dialogue.OnPlayerSays(userText);
        AddLine($"< {reply}");
        Render();
    }

    void AddLine(string line)
    {
        history.Add(line);
        if (history.Count > 500) history.RemoveRange(0, history.Count - 500);
        Debug.Log("[Dialogue] " + line);
    }

    void Render()
    {
        if (!output) return;

        if (showOnlyLastQA)
        {
            string lastQ = null, lastA = null;
            for (int i = history.Count - 1; i >= 0 && (lastQ == null || lastA == null); i--)
            {
                if (lastA == null && history[i].StartsWith("<")) lastA = history[i];
                if (lastQ == null && history[i].StartsWith(">")) lastQ = history[i];
            }
            output.text = (lastQ ?? "") + (lastA != null ? "\n" + lastA : "");
        }
        else
        {
            int start = Mathf.Max(0, history.Count - keepLastLines);
            output.text = string.Join("\n", history.GetRange(start, history.Count - start));
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            history.Clear();
            Render();
        }
    }

    void LogBoth(string line)
    {
        Debug.Log("[Dialogue] " + line);
        if (output)
        {
            output.text = (output.text?.Length > 0 ? output.text + "\n" : "") + line;
            if (output.text.Length > 4000) output.text = output.text.Substring(output.text.Length - 4000);
        }
    }
}
