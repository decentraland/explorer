﻿using System.Collections;
using System.Collections.Generic;
using DCL.Components;
using DCL.Models;
using UnityEngine;

public class Gizmo : MonoBehaviour
{
    public string gizmoType;
    public bool transformWithObject;

    public void SetObject(GameObject selectedObject)
    {
        if (selectedObject != null)
        {
            if (transformWithObject)
            {
                transform.SetParent(selectedObject.transform);
                transform.localPosition = Vector3.zero;
                
            }
            else
            {
                transform.position = selectedObject.transform.position;
            }

            gameObject.SetActive(true);
        }
        else
        {
            transform.SetParent(null);
            gameObject.SetActive(false);
        }
    }
}
