using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplePlayerController : MonoBehaviour
{
    public float speed = 6f;
    void Update()
    {
        var h = Input.GetAxis("Horizontal");
        var v = Input.GetAxis("Vertical");
        var dir = new Vector3(h, 0, v);
        if (dir.sqrMagnitude > 0.001f)
            transform.position += dir.normalized * speed * Time.deltaTime;
    }
}