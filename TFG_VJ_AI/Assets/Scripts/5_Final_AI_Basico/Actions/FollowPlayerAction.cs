using System.Collections;
using System.Collections.Generic;
using TFG.GOAP;
using UnityEngine;

[CreateAssetMenu(menuName = "TFG/GOAP/Actions/FollowPlayerAction")]
public class FollowPlayerAction : GoapAction
{
    public float followRadius = 2.5f;
    public float helpAttackRadius = 8f;

    public override bool CanRun(WorldState ws)
    {
        bool attackingPlayer = ws.Get("attack_player", false);
        return !attackingPlayer;
    }

    public override void Apply(WorldState ws)
    {
        ws.Set("following_player", true);
        ws.Set("attack_player", false);
    }
}
