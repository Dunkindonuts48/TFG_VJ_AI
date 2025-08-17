using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "GOAP/Goal")]
public class GoapGoal : ScriptableObject
{
    [Tooltip("Predicados que deben ser verdaderos al final del plan")]
    public List<string> desiredFacts = new List<string>();
    [Range(0, 100)] public int priority = 1;

    public HashSet<string> DesiredSet => new HashSet<string>(desiredFacts);
}
