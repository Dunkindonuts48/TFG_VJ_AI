using System.Collections;
using System.Collections.Generic;
using TFG.GOAP;
using UnityEngine;

[CreateAssetMenu(menuName = "TFG/GOAP/Actions/AttackPlayerAction")]
public class AttackPlayerAction : GoapAction
{
    public override bool CanRun(WorldState ws)
    {
        bool hostile = ws.Get("player_hostile", false);
        bool pacified = ws.Get("pacified", false);
        return hostile && !pacified;
    }

    public override void Apply(WorldState ws)
    {
        ws.Set("attack_player", true);
        ws.Set("following_player", false);
    }
}