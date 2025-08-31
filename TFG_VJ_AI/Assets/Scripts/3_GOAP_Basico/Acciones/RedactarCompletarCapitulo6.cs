using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TFG.GOAP
{
    public class RedactarCompletarCapitulo6 : GoapAction
    {
        private void Reset()
        {
            name = "Redactar/Completar Capítulo 6";
            energyCost = 16f;
            stressGain = 8f;

            effectFlagKey = WSKeys.Cap6_Redactado;

            preconditions.Clear();
            preconditions.Add(WSKeys.Reunion_TrasCap5);
        }
    }
}
