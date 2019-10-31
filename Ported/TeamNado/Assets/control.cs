using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class control : MonoBehaviour
{
    public GameObject target;
    private float x;

    private float y;

    private float z;
    
    // Start is called before the first frame update
    void Start()
    {
         x = Input.GetAxis("Mouse X");
         y = Input.GetAxis("Mouse Y");
         z = Input.GetAxis("Mouse ScrollWheel");
    }

    // Update is called once per frame
    void Update()
    {
        float xn = Input.GetAxis("Mouse X");
        float yn = Input.GetAxis("Mouse Y");
        float zn = Input.GetAxis("Mouse ScrollWheel");
        bool btn = Input.GetMouseButton(0);

        float xd = x - xn;
        float yd = y - yn;
        float zd = z - zn;

        //Debug.Log($"x {xn} y {yn} z {zn} b {btn}");

        //transform.Translate(xn, yn, zn);
        if (btn)
        {
            transform.RotateAround(Vector3.zero, -Vector3.up, xn);
            transform.RotateAround(Vector3.zero, transform.right, yn);
        }
        
        transform.Translate(0, 0, zn * 20);

        x = xn;
        y = yn;
        z = zn;
    }
}
