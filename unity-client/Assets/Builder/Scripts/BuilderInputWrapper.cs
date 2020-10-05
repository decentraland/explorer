using Builder;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuilderInputWrapper : MonoBehaviour
{
    public float msClickThreshold = 200;
    public float movementClickThreshold = 50;
    public Action<int,Vector3> OnMouseClick;
    public Action<int,Vector3,float,float> OnMouseDrag;
    public Action<int,Vector3,float,float> OnMouseDragRaw;
    public Action<float> OnMouseWheel;

    float lastTimeMouseDown = 0;
    Vector3 lastMousePosition;
    private void Awake()
    {
        DCLBuilderInput.OnMouseDrag += MouseDrag;
        DCLBuilderInput.OnMouseRawDrag += MouseRawDrag;
        DCLBuilderInput.OnMouseWheel += MouseWheel;
        DCLBuilderInput.OnMouseDown += MouseDown;
        DCLBuilderInput.OnMouseUp += MouseUp;
    }

    private void MouseUp(int buttonId, Vector3 mousePosition)
    {
        if (Vector3.Distance(mousePosition, lastMousePosition) >= movementClickThreshold) return;
        if (Time.unscaledTime >= lastTimeMouseDown + msClickThreshold / 1000) return;

        if(!IsPointerOverUIElement())OnMouseClick?.Invoke(buttonId, mousePosition);
    }

    private void MouseDown(int buttonId, Vector3 mousePosition)
    {
        lastTimeMouseDown = Time.unscaledTime;
        lastMousePosition = mousePosition;
    }

    private void MouseWheel(float axisValue)
    {
        if (!IsPointerOverUIElement()) OnMouseWheel?.Invoke(axisValue);
    }

    private void MouseDrag(int buttonId, Vector3 mousePosition, float axisX, float axisY)
    {
        if (!IsPointerOverUIElement()) OnMouseDrag?.Invoke(buttonId, mousePosition, axisX, axisY);
    }
    private void MouseRawDrag(int buttonId, Vector3 mousePosition, float axisX, float axisY)
    {
        if (!IsPointerOverUIElement()) OnMouseDragRaw?.Invoke(buttonId, mousePosition, axisX, axisY);
    }

    public bool IsPointerOverUIElement()
    {
        var eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 1;
    }
}
