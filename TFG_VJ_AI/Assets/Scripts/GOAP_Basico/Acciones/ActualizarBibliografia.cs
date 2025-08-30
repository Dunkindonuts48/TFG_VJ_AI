using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TFG.GOAP
{
    public class ActualizarBibliografia : GoapAction
    {
        private void Reset()
        {
            name = "Actualizar bibliografía";
            energyCost = 10f;
            stressGain = 6f;

            effectFlagKey = WSKeys.Bibliografia_OK;

            preconditions.Clear();
            preconditions.Add(WSKeys.Reunion_TrasCap6);
        }
    }
}
