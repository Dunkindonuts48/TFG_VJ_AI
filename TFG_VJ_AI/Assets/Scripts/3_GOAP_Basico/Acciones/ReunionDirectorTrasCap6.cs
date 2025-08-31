using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TFG.GOAP
{
    public class ReunionDirectorTrasCap6 : GoapAction
    {
        private void Reset()
        {
            name = "Reunión con Director (tras Cap 6)";
            energyCost = 6f;
            stressGain = 4f;

            effectFlagKey = WSKeys.Reunion_TrasCap6;

            preconditions.Clear();
            preconditions.Add(WSKeys.Cap6_Entregado);
        }
    }
}
