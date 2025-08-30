using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TFG.GOAP
{
    public abstract class GoapAction : MonoBehaviour
    {
        [Header("Costes base")]
        [SerializeField] protected float energyCost = 10f;
        [SerializeField] protected float stressGain = 5f;

        [Header("Efecto principal")]
        [SerializeField] protected string effectFlagKey = "";
        [SerializeField] protected bool effectValue = true;

        [Header("Precondiciones (todas deben cumplirse)")]
        [SerializeField] protected List<string> preconditions = new();

        public string ActionName => name;

        public IReadOnlyList<string> Preconditions => preconditions;
        public string EffectKey => effectFlagKey;
        public bool EffectValue => effectValue;

        public virtual bool CanRun(WorldState ws)
        {
            if (ws == null || ws.Energia < energyCost) return false;
            foreach (var p in preconditions)
                if (!ws.Get(p)) return false;

            return true;
        }

        public virtual void Apply(WorldState ws)
        {
            ws.SpendEnergy(energyCost);
            ws.AddStress(stressGain);
            if (!string.IsNullOrEmpty(effectFlagKey))
                ws.Set(effectFlagKey, effectValue);
        }
    }
}
