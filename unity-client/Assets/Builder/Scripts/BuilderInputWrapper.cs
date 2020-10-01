using Builder;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuilderInputWrapper : MonoBehaviour
{
    public float msClickThreshold = 200;
    public Action<int,Vector3> OnMouseClick;
    public Action<int,Vector3,float,float> OnMouseDrag;
    public Action<int,Vector3,float,float> OnMouseDragRaw;
    public Action<float> OnMouseWheel;

    float lastTimeMouseDown = 0;
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
        if (Time.unscaledTime  <= lastTimeMouseDown + msClickThreshold / 1000) OnMouseClick?.Invoke(buttonId, mousePosition);


    }

    private void MouseDown(int buttonId, Vector3 mousePosition)
    {
        lastTimeMouseDown = Time.unscaledTime;
    }

    private void MouseWheel(float axisValue)
    {
        OnMouseWheel?.Invoke(axisValue);
    }

    private void MouseDrag(int buttonId, Vector3 mousePosition, float axisX, float axisY)
    {
        OnMouseDrag?.Invoke(buttonId, mousePosition, axisX, axisY);
    }
    private void MouseRawDrag(int buttonId, Vector3 mousePosition, float axisX, float axisY)
    {
        OnMouseDragRaw?.Invoke(buttonId, mousePosition, axisX, axisY);
    }
}
