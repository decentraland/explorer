using Builder.Gizmos;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeCameraMovement : MonoBehaviour
{
    public BuilderInputWrapper builderInputWrapper;

    public float smoothLookAtSpeed = 5f;

    [Header("Manual Camera Movement")]

    public float keyboardMovementSpeed = 5f;
    public float lookSpeedH = 2f;

    public float lookSpeedV = 2f;

    public float zoomSpeed = 2f;

    public float dragSpeed = 3f;

    private float yaw = 0f;
    private float pitch = 0f;

    bool isCameraAbleToMove = true;

    Coroutine smoothLookAtCor;
    private void Awake()
    {
        builderInputWrapper.OnMouseDrag += MouseDrag;
        builderInputWrapper.OnMouseDragRaw += MouseDragRaw;
        builderInputWrapper.OnMouseWheel += MouseWheel;

        DCLBuilderGizmoManager.OnGizmoTransformObjectStart += OnGizmoTransformObjectStart;
        DCLBuilderGizmoManager.OnGizmoTransformObjectEnd += OnGizmoTransformObjectEnd;

    }

    private void Update()
    {

        if (Input.GetKey("w"))
        {
            transform.position += transform.forward * keyboardMovementSpeed * Time.deltaTime;
        }
        if (Input.GetKey("s"))
        {
            transform.position += -transform.forward *keyboardMovementSpeed* Time.deltaTime;
        }
        if (Input.GetKey("d"))
        {
            transform.position += transform.right * keyboardMovementSpeed * Time.deltaTime;
        }
        if (Input.GetKey("a"))
        {
            transform.position += -transform.right * keyboardMovementSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.Space))
        {
            transform.position += transform.up * keyboardMovementSpeed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.X))
        {
            transform.position += -transform.up * keyboardMovementSpeed * Time.deltaTime;
        }

    }


    private void OnGizmoTransformObjectEnd(string gizmoType)
    {
        isCameraAbleToMove = true;
    }

    private void OnGizmoTransformObjectStart(string gizmoType)
    {
        isCameraAbleToMove = false;
    }


    private void MouseWheel(float axis)
    {
      if(isCameraAbleToMove)  transform.Translate(0, 0, axis * zoomSpeed, Space.Self);
    }
    private void MouseDragRaw(int buttonId, Vector3 mousePosition, float axisX, float axisY)
    {
        if(buttonId == 1) CameraLook(axisX, axisY);
    }
    private void MouseDrag(int buttonId, Vector3 mousePosition, float axisX, float axisY)
    {
        if (buttonId == 0 ||buttonId == 2) CameraDrag(axisX, axisY);
    }
 

    public void CameraDrag(float axisX, float axisY)
    {
        if (isCameraAbleToMove) transform.Translate(-axisX * Time.deltaTime * dragSpeed, -axisY * Time.deltaTime * dragSpeed, 0);
    }
    public void CameraLook(float axisX, float axisY)
    {
        if (isCameraAbleToMove)
        {
            yaw += lookSpeedH * axisX;
            pitch -= lookSpeedV * axisY;

            transform.eulerAngles = new Vector3(pitch, yaw, 0f);
        }
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

    public void SmoothLookAt(Transform transform)
    {
        if (smoothLookAtCor != null) StopCoroutine(smoothLookAtCor);
        smoothLookAtCor = StartCoroutine(SmoothLookAtCorutine(transform));
    }


    IEnumerator SmoothLookAtCorutine(Transform target)
    {
        Quaternion targetRotation = Quaternion.LookRotation(target.transform.position - transform.position);
        float advance = 0;
        while(advance <= 1)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, advance);
            advance += smoothLookAtSpeed * Time.deltaTime;
            yield return null;
        }
        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;
    }
}
