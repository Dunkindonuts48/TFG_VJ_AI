using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TFG.GOAP
{
    public class PresentarTFG : GoapAction
    {
        private void Reset()
        {
            name = "Presentar TFG (final)";
            energyCost = 25f;
            stressGain = 12f;

            effectFlagKey = WSKeys.TFG_Presentado;

            preconditions.Clear();
            preconditions.Add(WSKeys.Defensa_Preparada);
        }
    }
}
