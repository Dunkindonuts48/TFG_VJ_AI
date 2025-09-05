using System.Collections;
using System.Collections.Generic;
using TFG.NPC;
using UnityEngine;

public class ChatDebug : MonoBehaviour
{
    public HybridMemoryDrivenAI ai;
    void OnGUI()
    {
        if (!ai) return;
        GUILayout.BeginArea(new Rect(10, 10, 220, 160), GUI.skin.box);
        if (GUILayout.Button("Sígueme")) ai.ReceivePlayerText("sígueme");
        if (GUILayout.Button("Deja de seguir")) ai.ReceivePlayerText("deja de seguir");
        if (GUILayout.Button("Enemistarse (ataca)")) ai.ReceivePlayerText("te odio");
        if (GUILayout.Button("Pedir perdón")) ai.ReceivePlayerText("perdón");
        GUILayout.EndArea();
    }
}