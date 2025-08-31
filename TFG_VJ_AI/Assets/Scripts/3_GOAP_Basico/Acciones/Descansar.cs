using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TFG.GOAP
{
    public class Descansar : GoapAction
    {
        private void Reset()
        {
            name = "Descansar";
            energyCost = -30f;
            stressGain = -6f;

            effectFlagKey = "";
            preconditions.Clear();
        }
    }
}

