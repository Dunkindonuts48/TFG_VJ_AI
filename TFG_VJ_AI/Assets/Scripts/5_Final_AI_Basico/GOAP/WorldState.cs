using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldState
{
    Dictionary<string, object> map = new Dictionary<string, object>();
    public T Get<T>(string key, T def = default(T)) { return map.TryGetValue(key, out var v) && v is T t ? t : def; }
    public void Set<T>(string key, T value) { map[key] = value; }
    public WorldState Clone() { var w = new WorldState(); foreach (var kv in map) w.map[kv.Key] = kv.Value; return w; }
}
