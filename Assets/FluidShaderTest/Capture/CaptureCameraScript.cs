using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptureCameraScript : MonoBehaviour
{
    public const int PLANE_DEFAULT_DIMENSION = 10;
    public float halfFoV = 30f;
    public float thickness = 0.2f;
    public GameObject projectionPlane;

    private Camera thisCam;
    // Start is called before the first frame update
    void Start()
    {
        thisCam = GetComponent<Camera>();

        thisCam.fieldOfView = 2 * halfFoV;

        float halfPlaneWidth = projectionPlane.transform.localScale.z * PLANE_DEFAULT_DIMENSION * 0.5f;

        thisCam.farClipPlane = halfPlaneWidth / (float)Math.Tan(halfFoV * Mathf.Deg2Rad);
        thisCam.nearClipPlane = thisCam.farClipPlane - thickness;

        transform.position = projectionPlane.transform.position 
            - new Vector3(0, (thisCam.farClipPlane + thisCam.nearClipPlane) * 0.5f, 0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
