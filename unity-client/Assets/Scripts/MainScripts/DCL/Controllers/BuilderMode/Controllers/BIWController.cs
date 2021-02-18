using DCL.Controllers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BIWController : MonoBehaviour
{
    protected ParcelScene sceneToEdit;

    protected bool isFeatureActive = false;

    public virtual void Init()
    {
        isFeatureActive = false;
    }

    public virtual void EnterEditMode(ParcelScene sceneToEdit)
    {
        this.sceneToEdit = sceneToEdit;
        isFeatureActive = true;
    }

    public virtual void ExitEditMode()
    {
        isFeatureActive = false;
        sceneToEdit = null;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (!isFeatureActive)
            return;
        FrameUpdate();
    }

    protected virtual void FrameUpdate()
    {

    }
}
