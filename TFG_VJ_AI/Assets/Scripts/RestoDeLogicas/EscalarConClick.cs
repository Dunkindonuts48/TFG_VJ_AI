using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EscalarConClick : MonoBehaviour
{
    public float escalaMin = 0f;
    public float escalaMax = 1.5f;
    public float velocidad = 0.2f;

    private bool aumentado = false;
    private Coroutine corutinaEscala;

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            aumentado = !aumentado;

            if (corutinaEscala != null)
                StopCoroutine(corutinaEscala);
            float objetivo = aumentado ? escalaMax : escalaMin;
            corutinaEscala = StartCoroutine(CambiarEscalaX(objetivo));
        }
    }

    IEnumerator CambiarEscalaX(float objetivo)
    {
        Vector3 escalaActual = transform.localScale;

        while (Mathf.Abs(transform.localScale.x - objetivo) > 0.01f)
        {
            escalaActual = transform.localScale;
            escalaActual.x = Mathf.MoveTowards(escalaActual.x, objetivo, velocidad * Time.deltaTime);
            transform.localScale = escalaActual;
            yield return null;
        }
        escalaActual.x = objetivo;
        transform.localScale = escalaActual;
    }
}

