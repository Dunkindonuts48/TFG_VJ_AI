using System.Collections;
using System.Collections.Generic;
using TFG.GOAP;
using UnityEngine;

[CreateAssetMenu(menuName = "TFG/GOAP/Actions/StopFollowingAction")]
public class StopFollowingAction : GoapAction
{
    public override bool CanRun(WorldState ws)
    {
        return ws.Get("following_player", false);
    }

    public override void Apply(WorldState ws)
    {
        ws.Set("following_player", false);
    }
}