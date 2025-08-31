using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TFG.GOAP
{
    public class EntregarCap5 : GoapAction
    {
        private void Reset()
        {   
            name = "Entregar Capítulo 5";
            energyCost = 8f;
            stressGain = 5f;

            effectFlagKey = WSKeys.Cap5_Entregado;

            preconditions.Clear();
            preconditions.Add(WSKeys.Cap5_Redactado);
        }
    }
}

