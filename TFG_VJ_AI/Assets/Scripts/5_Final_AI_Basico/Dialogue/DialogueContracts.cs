using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntentResponse
{
    public string intent;
    public Dictionary<string, string> slots;
    public System.Collections.Generic.List<GoalDelta> goalDelta;
    public System.Collections.Generic.List<MemWrite> memoryWrites;
    public string npcReply;
}

public class GoalDelta { public string goal; public float utilityDelta; }
public class MemWrite { public string type; public string content; public System.Collections.Generic.List<string> tags; public float importance; }