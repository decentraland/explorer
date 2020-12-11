using System.Collections;
using System.Collections.Generic;
using DCL.Interface;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class EnableDisableAO : MonoBehaviour
{

    [SerializeField] private ForwardRendererData rendererData;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            rendererData.rendererFeatures[0].SetActive(!rendererData.rendererFeatures[0].isActive);
            rendererData.SetDirty();
        }

        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            WebInterface.SetBaseResolution(720);
        }

        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            WebInterface.SetBaseResolution(1080);
        }

        if (Input.GetKeyDown(KeyCode.Keypad3))
        {
            WebInterface.SetBaseResolution(9999);
        }
    }
}
