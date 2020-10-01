using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeCameraMovement : MonoBehaviour
{
    public BuilderInputWrapper builderInputWrapper;
    public float lookSpeedH = 2f;

    public float lookSpeedV = 2f;

    public float zoomSpeed = 2f;

    public float dragSpeed = 3f;

    private float yaw = 0f;
    private float pitch = 0f;

    private void Awake()
    {
        builderInputWrapper.OnMouseDrag += MouseDrag;
        builderInputWrapper.OnMouseDragRaw += MouseDragRaw;
        builderInputWrapper.OnMouseWheel += MouseWheel;
    }


    //private void Update()
    //{

        //Look around
        //if (Input.GetMouseButton(1))
        //{
        //    yaw += lookSpeedH * Input.GetAxis("Mouse X");
        //    pitch -= lookSpeedV * Input.GetAxis("Mouse Y");

        //    transform.eulerAngles = new Vector3(pitch, yaw, 0f);
        //}

        ////drag camera
        //if (Input.GetMouseButton(0))
        //{
        //transform.Translate(-Input.GetAxisRaw("Mouse X") * Time.deltaTime * dragSpeed, -Input.GetAxisRaw("Mouse Y") * Time.deltaTime * dragSpeed, 0);
        //}

        //if (Input.GetMouseButton(2))
        //{
        //    //Zoom in and out 
        //    transform.Translate(0, 0, Input.GetAxisRaw("Mouse X") * zoomSpeed * .07f, Space.Self);
        //}

        //Zoom in and out 
    

    //}



    private void MouseWheel(float axis)
    {
        transform.Translate(0, 0, axis * zoomSpeed, Space.Self);
    }
    private void MouseDragRaw(int buttonId, Vector3 mousePosition, float axisX, float axisY)
    {
        if(buttonId == 1) CameraLook(axisX, axisY);
    }
    private void MouseDrag(int buttonId, Vector3 mousePosition, float axisX, float axisY)
    {
        if (buttonId == 0) CameraDrag(axisX, axisY);
    }
 

    public void CameraDrag(float axisX, float axisY)
    {

        transform.Translate(-axisX * Time.deltaTime * dragSpeed, -axisY * Time.deltaTime * dragSpeed, 0);
    }
    public void CameraLook(float axisX, float axisY)
    {
        yaw += lookSpeedH * axisX;
        pitch -= lookSpeedV * axisY;

        transform.eulerAngles = new Vector3(pitch, yaw, 0f);
    }


    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }
    public void LookAt(Transform transformToLookAt)
    {
        transform.LookAt(transformToLookAt);
        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;

    }
}
