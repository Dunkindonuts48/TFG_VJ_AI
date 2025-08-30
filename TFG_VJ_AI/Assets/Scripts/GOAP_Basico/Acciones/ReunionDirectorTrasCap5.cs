using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TFG.GOAP
{
    public class ReunionDirectorTrasCap5 : GoapAction
    {
        private void Reset()
        {
            name = "Reunión con Director (tras Cap 5)";
            energyCost = 6f;
            stressGain = 4f;

            effectFlagKey = WSKeys.Reunion_TrasCap5;
            preconditions.Clear();
            preconditions.Add(WSKeys.Cap5_Entregado);
        }
    }
}
