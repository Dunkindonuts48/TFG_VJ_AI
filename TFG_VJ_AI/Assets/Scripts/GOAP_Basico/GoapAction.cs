using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GoapAction : MonoBehaviour
{
    [Header("Planificación")]
    [SerializeField] private List<string> preconditions = new List<string>();
    [SerializeField] private List<string> effects = new List<string>();
    [SerializeField] private float cost = 1f;

    public HashSet<string> Preconditions => new HashSet<string>(preconditions);
    public HashSet<string> Effects => new HashSet<string>(effects);
    public float Cost => cost;

    public virtual bool CheckContext(WorldState ws) => true;
    public virtual void OnPlanCalculated() { }
    public abstract void OnStart();
    public abstract bool Perform(float dt);
    public virtual void OnEnd(bool success) { }

    public WorldState Apply(WorldState s)
    {
        var ns = s.Clone();
        foreach (var e in Effects) ns.Add(e);
        return ns;
    }
}
