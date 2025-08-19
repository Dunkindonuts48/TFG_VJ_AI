using System;
using System.Collections.Generic;
using UnityEngine;

namespace TFG.GOAP
{
    public class WorldState : MonoBehaviour
    {
        public Dictionary<string, bool> StringBools = new Dictionary<string, bool>();
        public Dictionary<string, int> StringInteger = new Dictionary<string, int>();
        public Dictionary<string, float> StringFloat = new Dictionary<string, float>();
        public Dictionary<string, DateTime> StringDates = new Dictionary<string, DateTime>();
        public Dictionary<string, List<string>> StringLists = new Dictionary<string, List<string>>();

        public bool HasBool(string k, bool v = true) => StringBools.TryGetValue(k, out var x) && x == v;
        public bool HasIntAtLeast(string k, int min) => StringInteger.TryGetValue(k, out var x) && x >= min;
        public bool HasFloatAtLeast(string k, float min) => StringFloat.TryGetValue(k, out var x) && x >= min;
        public bool HasFloatAtMost(string k, float max) => StringFloat.TryGetValue(k, out var x) && x <= max;
        public bool HasDateOn(string k, DateTime date) => StringDates.TryGetValue(k, out var x) && x.Date == date.Date;

        public void SetBool(string s_bool, bool b_bool) { 
            StringBools[s_bool] = b_bool; 
        }
        public void SetInt(string s_int, int i_int) {
            StringInteger[s_int] = i_int; 
        }
        public void IncInt(string s_incrInt, int i_incrInt) {
            StringInteger[s_incrInt] = (StringInteger.TryGetValue(s_incrInt, out var x) ? x : 0) + i_incrInt; 
        }
        public void SetFloat(string s_float, float f_float) {
            StringFloat[s_float] = f_float; 
        }
        public void IncFloat(string s_incrFloat, float f_incrFloat) {
            StringFloat[s_incrFloat] = (StringFloat.TryGetValue(s_incrFloat, out var x) ? x : 0f) + f_incrFloat; 
        }
        public void SetDate(string s_date, DateTime d_date) {
            StringDates[s_date] = d_date; 
        }

        public WorldState Snapshot()
        {
            var GameObj = new GameObject("WS_Snapshot_TMP");
            GameObj.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            var WorldState = GameObj.AddComponent<WorldState>();

            foreach (var kv in StringBools) WorldState.StringBools[kv.Key] = kv.Value;
            foreach (var kv in StringInteger) WorldState.StringInteger[kv.Key] = kv.Value;
            foreach (var kv in StringFloat) WorldState.StringFloat[kv.Key] = kv.Value;
            foreach (var kv in StringDates) WorldState.StringDates[kv.Key] = kv.Value;
            foreach (var kv in StringLists) WorldState.StringLists[kv.Key] = new List<string>(kv.Value);
            return WorldState;
        }

        public void ApplyDelta(float dEnergy, float dStress, int dTimeMin)
        {
            IncFloat("energia", dEnergy);
            IncFloat("nivel_estres", dStress);
            IncInt("tiempo_disponible_hoy", -Mathf.Max(0, dTimeMin));

            if (!StringFloat.ContainsKey("energia")) StringFloat["energia"] = 0f;
            if (!StringFloat.ContainsKey("nivel_estres")) StringFloat["nivel_estres"] = 0f;
            if (!StringInteger.ContainsKey("tiempo_disponible_hoy")) StringInteger["tiempo_disponible_hoy"] = 0;

            StringFloat["energia"] = Mathf.Clamp(StringFloat["energia"], 0f, 100f);
            StringFloat["nivel_estres"] = Mathf.Clamp(StringFloat["nivel_estres"], 0f, 100f);
            StringInteger["tiempo_disponible_hoy"] = Mathf.Max(0, StringInteger["tiempo_disponible_hoy"]);
        }

    }
}
