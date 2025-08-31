using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TFG.GOAP
{
    public class PrepararDefensa : GoapAction
    {
        private void Reset()
        {
            name = "Preparar Defensa";
            energyCost = 18f;
            stressGain = 9f;

            effectFlagKey = WSKeys.Defensa_Preparada;

            preconditions.Clear();
            preconditions.Add(WSKeys.Bibliografia_OK);
        }
    }
}
