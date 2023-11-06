using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardMove : MonoBehaviour
{
    public float acceleration = 2000f;
    public float SPEED_MAX = 3f;
    //private Vector3 vel = Vector3.zero;

    private void Update()
    {
        //vel *= 0.998f;

        if (Input.GetKey(KeyCode.D))
        {
            //vel += new Vector3(acceleration * Time.deltaTime, 0f, 0f);
            transform.position += new Vector3(SPEED_MAX, 0, 0) * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.A))
        {
            //vel -= new Vector3(acceleration * Time.deltaTime, 0f, 0f);
            transform.position -= new Vector3(SPEED_MAX, 0, 0) * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.W))
        {
            //vel += new Vector3(0, 0, acceleration * Time.deltaTime);
            transform.position += new Vector3(0, 0, SPEED_MAX) * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.S))
        {
            //vel -= new Vector3(0, 0, acceleration * Time.deltaTime);
            transform.position -= new Vector3(0, 0, SPEED_MAX) * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            transform.position += new Vector3(0, SPEED_MAX, 0) * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            transform.position -= new Vector3(0, SPEED_MAX, 0) * Time.deltaTime;
        }
        /*
        if (vel.magnitude >= SPEED_MAX) {
            vel = vel.normalized * SPEED_MAX;
        }
        
        transform.position += (SPEED_MAX * Time.deltaTime);
        */
    }
}
