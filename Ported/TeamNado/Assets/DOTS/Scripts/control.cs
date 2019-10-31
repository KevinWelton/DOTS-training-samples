using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class control : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        float xn = Input.GetAxis("Mouse X");
        float yn = Input.GetAxis("Mouse Y");
        float zn = Input.GetAxis("Mouse ScrollWheel");
        bool btn = Input.GetMouseButton(0);
        
        if (btn)
        {
            transform.RotateAround(Vector3.zero, -Vector3.up, xn);
            transform.RotateAround(Vector3.zero, transform.right, yn);
        }
        
        transform.Translate(0, 0, zn * 20);
    }
}
