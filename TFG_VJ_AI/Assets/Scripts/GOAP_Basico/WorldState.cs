using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WorldState
{
    public HashSet<string> facts = new HashSet<string>();

    public bool Has(string fact) => facts.Contains(fact);
    public void Add(string fact) => facts.Add(fact);
    public void Remove(string fact) => facts.Remove(fact);

    public WorldState Clone()
    {
        var c = new WorldState();
        foreach (var f in facts) c.facts.Add(f);
        return c;
    }

    public bool Satisfies(HashSet<string> desired)
    {
        foreach (var d in desired) if (!facts.Contains(d)) return false;
        return true;
    }
}
