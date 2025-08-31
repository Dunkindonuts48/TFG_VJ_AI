using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public abstract class GoapAction : MonoBehaviour
{
    public string actionName;
    public float energyCost = 1f;
    public abstract bool CanRun(WorldState s);
    public abstract void Apply(WorldState s);
    public virtual float MemoryCost(WorldState s, MemoryRepository mem) => 0f;
}