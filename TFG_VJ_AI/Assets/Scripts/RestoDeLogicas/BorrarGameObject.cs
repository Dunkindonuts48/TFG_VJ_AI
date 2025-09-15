using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BorrarGameObject : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Destroy(gameObject);
        }
    }
}
