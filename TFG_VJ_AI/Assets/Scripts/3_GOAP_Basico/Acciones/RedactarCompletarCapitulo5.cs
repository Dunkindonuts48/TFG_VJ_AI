using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TFG.GOAP
{
    public class RedactarCompletarCapitulo5 : GoapAction
    {
        private void Reset()
        {
            name = "Redactar/Completar Capítulo 5";
            energyCost = 15f;
            stressGain = 7f;

            effectFlagKey = WSKeys.Cap5_Redactado;
            effectValue = true;

            preconditions.Clear();
        }
    }
}
