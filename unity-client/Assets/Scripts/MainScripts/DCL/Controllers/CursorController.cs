﻿using DCL;
using UnityEngine;
using UnityEngine.UI;

public class CursorController : MonoBehaviour
{
    public Image cursorImage;
    public Sprite normalCursor;
    public Sprite hoverCursor;

    void OnEnable()
    {
        PointerEventsController.OnPointerHoverStarts += SetHoverCursor;
        PointerEventsController.OnPointerHoverEnds += SetNormalCursor;
    }

    void OnDisable()
    {
        PointerEventsController.OnPointerHoverStarts -= SetHoverCursor;
        PointerEventsController.OnPointerHoverEnds -= SetNormalCursor;
    }

    void SetNormalCursor()
    {
        cursorImage.sprite = normalCursor;
        cursorImage.SetNativeSize();

        Debug.Log("SWITCH TO NORMAL CURSOR");
    }

    void SetHoverCursor()
    {
        cursorImage.sprite = hoverCursor;
        cursorImage.SetNativeSize();

        Debug.Log("SWITCH TO HOVER CURSOR");
    }
}
