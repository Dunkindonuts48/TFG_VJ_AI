using System.Collections;
using System.Collections.Generic;
using TFG.NPC;
using TMPro;
using UnityEngine;

public class UIBridge : MonoBehaviour
{
    public HybridMemoryDrivenAI npc;
    public TMP_InputField input;
    public TMP_Text lastExchangeText;

    public async void OnSend()
    {
        var text = string.IsNullOrWhiteSpace(input.text) ? "hola" : input.text;
        var reply = await npc.ReceivePlayerTextAsync(text);

        if (lastExchangeText) lastExchangeText.text = $"> {text}\n< {reply}";
        input.text = "";
        input.ActivateInputField();
    }
}