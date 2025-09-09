using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TFG.Memory;

namespace TFG.NPC
{

    [Serializable] 
    public class SocialBlackboard
    {
        public bool followingPlayer;
        public bool attackPlayer;
        public bool playerHostile;
        public bool pacified;

        public void ResetNonPersistent()
        {
            attackPlayer = false;
            pacified = false;
        }
    }
}