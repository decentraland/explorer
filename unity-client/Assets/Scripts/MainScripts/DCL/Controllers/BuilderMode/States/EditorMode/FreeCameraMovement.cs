using Builder.Gizmos;
using DCL.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeCameraMovement : CameraStateBase
{
    public BuilderInputWrapper builderInputWrapper;

    public float smoothLookAtSpeed = 5f;
    public float focusDistance = 5f;
    public float focusSpeed = 5f;

    [Header("Manual Camera Movement")]

    public float keyboardMovementSpeed = 5f;
    public float lookSpeedH = 2f;

    public float lookSpeedV = 2f;

    public float zoomSpeed = 2f;

    public float dragSpeed = 3f;

    [Header("InputActions")]
    [SerializeField] internal InputAction_Hold advanceFowardInputAction;
    [SerializeField] internal InputAction_Hold advanceBackInputAction;
    [SerializeField] internal InputAction_Hold advanceLeftInputAction;
    [SerializeField] internal InputAction_Hold advanceRightInputAction;
    [SerializeField] internal InputAction_Hold advanceUpInputAction;
    [SerializeField] internal InputAction_Hold advanceDownInputAction;


    private float yaw = 0f;
    private float pitch = 0f;

    bool isCameraAbleToMove = true, isAdvancingFoward = false,isAdvancingBackward = false, isAdvancingLeft = false, isAdvancingRight = false, isAdvancingUp = false, isAdvancingDown = false;

    Coroutine smoothLookAtCor;
    private void Awake()
    {
        builderInputWrapper.OnMouseDrag += MouseDrag;
        builderInputWrapper.OnMouseDragRaw += MouseDragRaw;
        builderInputWrapper.OnMouseWheel += MouseWheel;

        DCLBuilderGizmoManager.OnGizmoTransformObjectStart += OnGizmoTransformObjectStart;
        DCLBuilderGizmoManager.OnGizmoTransformObjectEnd += OnGizmoTransformObjectEnd;

    }
    private void Start()
    {
        advanceFowardInputAction.OnStarted += (o) => { isAdvancingFoward = true; };
        advanceFowardInputAction.OnFinished += (o) => { isAdvancingFoward = false; };

        advanceBackInputAction.OnStarted += (o) => { isAdvancingBackward = true; };
        advanceBackInputAction.OnFinished += (o) => { isAdvancingBackward = false; };

        advanceLeftInputAction.OnStarted += (o) => { isAdvancingLeft = true; };
        advanceLeftInputAction.OnFinished += (o) => { isAdvancingLeft = false; };

        advanceRightInputAction.OnStarted += (o) => { isAdvancingRight = true; };
        advanceRightInputAction.OnFinished += (o) => { isAdvancingRight = false; };

        advanceUpInputAction.OnStarted += (o) => { isAdvancingUp = true; };
        advanceUpInputAction.OnFinished += (o) => { isAdvancingUp = false; };

        advanceDownInputAction.OnStarted += (o) => { isAdvancingDown = true; };
        advanceDownInputAction.OnFinished += (o) => { isAdvancingDown = false; };
    }

    private void Update()
    {
        Vector3 velocity = Vector3.zero;
        if (isAdvancingFoward)
        {
            velocity += transform.forward;
        }
        if (isAdvancingBackward)
        {
            velocity += -transform.forward;
        }
        if (isAdvancingRight)
        {
            velocity += transform.right;
        }
        if (isAdvancingLeft)
        {
            velocity += -transform.right;
        }
        if (isAdvancingUp)
        {
            velocity += transform.up;
        }
        if (isAdvancingDown)
        {
            velocity += -transform.up;
        }
        transform.position += velocity * keyboardMovementSpeed * Time.deltaTime;
    }


    public void SetCameraCanMove(bool canMove)
    {
        isCameraAbleToMove = canMove;
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

    public override Vector3 OnGetRotation()
    {
        return transform.eulerAngles;
    }
    public void FocusOnEntities(List<DecentralandEntityToEdit> entitiesToFocus)
    {
        Vector3 middlePoint = FindMidPoint(entitiesToFocus);
        StartCoroutine(SmoothFocusOnTarget(middlePoint));
        SmoothLookAt(middlePoint);
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
        SmoothLookAt(transform.position);
    }
    public void SmoothLookAt(Vector3 position)
    {
        if (smoothLookAtCor != null) StopCoroutine(smoothLookAtCor);
        smoothLookAtCor = StartCoroutine(SmoothLookAtCorutine(position));
    }

    Vector3 FindMidPoint(List<DecentralandEntityToEdit> entitiesToLook)
    {
        Vector3 finalPosition = Vector3.zero;
        int totalPoints = 0;
        foreach(DecentralandEntityToEdit entity in entitiesToLook)
        {
            if (entity.rootEntity.meshRootGameObject && entity.rootEntity.meshesInfo.renderers.Length > 0) {
                Vector3 midPointFromEntity = Vector3.zero;
                foreach (Renderer render in entity.rootEntity.renderers)
                {
                    midPointFromEntity += render.bounds.center;
                }
                midPointFromEntity /= entity.rootEntity.renderers.Length;
                finalPosition += midPointFromEntity;
                totalPoints++;
            }
           
        }

        finalPosition /= totalPoints;
        return finalPosition;
    }


    IEnumerator SmoothFocusOnTarget(Vector3 targetPosition)
    {
        float advance = 0;
        while (advance <= 1)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, advance);
            advance += smoothLookAtSpeed * Time.deltaTime;
            if (Vector3.Distance(transform.position, targetPosition) <= focusDistance) advance = 2;
            yield return null;
        }
    }
    IEnumerator SmoothLookAtCorutine(Vector3 targetPosition)
    {
        Quaternion targetRotation = Quaternion.LookRotation(targetPosition - transform.position);
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
