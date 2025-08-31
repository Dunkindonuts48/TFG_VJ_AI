using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TFG.GOAP
{
    public class EntregarCap6 : GoapAction
    {
        private void Reset()
        {
            name = "Entregar Capítulo 6";
            energyCost = 8f;
            stressGain = 5f;

            effectFlagKey = WSKeys.Cap6_Entregado;

            preconditions.Clear();
            preconditions.Add(WSKeys.Cap6_Redactado);
        }
    }
}

