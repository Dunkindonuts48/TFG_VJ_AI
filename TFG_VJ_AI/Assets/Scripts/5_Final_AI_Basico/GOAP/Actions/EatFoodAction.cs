using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class EatFoodAction : GoapAction
{
    public override bool CanRun(WorldState s) => s.Get<bool>("hasFood", false);
    public override void Apply(WorldState s) { s.Set("isHungry", false); }
    public override float MemoryCost(WorldState s, MemoryRepository mem)
    {
        var rec = mem.AllReadOnly.FirstOrDefault(m =>
            m.tags != null && m.tags.Contains("comida") &&
            m.content.ToLower().Contains("enfermo"));
        return rec != null ? 5f : 0f;
    }
}
