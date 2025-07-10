using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    public void DeactivateAndReactivate(GameObject obj, float delay)
    {
        StartCoroutine(DeactivateReactivateCoroutine(obj, delay));
    }

    private IEnumerator DeactivateReactivateCoroutine(GameObject obj, float delay)
    {
        obj.SetActive(false);
        yield return new WaitForSeconds(delay);
        obj.SetActive(true);
    }
}
