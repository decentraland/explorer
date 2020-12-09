using System.Collections;
using System.Collections.Generic;
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
    }
}
