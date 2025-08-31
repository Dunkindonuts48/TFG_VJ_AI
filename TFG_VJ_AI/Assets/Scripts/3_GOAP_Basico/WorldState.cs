using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TFG.GOAP
{
    public class WorldState
    {
        public float Energia { get; private set; }
        public float Estres { get; private set; }

        private readonly Dictionary<string, bool> flags = new();

        public WorldState(float energiaInicial = 100f, float estresInicial = 0f)
        {
            Energia = energiaInicial;
            Estres = estresInicial;

            flags[WSKeys.Cap5_Redactado] = false;
            flags[WSKeys.Cap5_Entregado] = false;
            flags[WSKeys.Reunion_TrasCap5] = false;
            flags[WSKeys.Cap6_Redactado] = false;
            flags[WSKeys.Cap6_Entregado] = false;
            flags[WSKeys.Reunion_TrasCap6] = false;
            flags[WSKeys.Bibliografia_OK] = false;
            flags[WSKeys.Defensa_Preparada] = false;
            flags[WSKeys.TFG_Presentado] = false;
        }

        public bool Get(string key) => flags.TryGetValue(key, out var v) && v;
        public void Set(string key, bool value) => flags[key] = value;

        public void SpendEnergy(float amount) => Energia = UnityEngine.Mathf.Max(0f, Energia - amount);
        public void AddStress(float amount) => Estres = UnityEngine.Mathf.Max(0f, Estres + amount);

        public WorldState Clone()
        {
            var w = new WorldState(Energia, Estres);
            foreach (var kv in flags) w.flags[kv.Key] = kv.Value;
            return w;
        }
    }
}
