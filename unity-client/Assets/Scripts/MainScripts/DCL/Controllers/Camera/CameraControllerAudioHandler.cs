using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControllerAudioHandler : MonoBehaviour
{
    [SerializeField]
    CameraController cameraController;

    [SerializeField]
    AudioEvent eventCameraFadeIn, eventCameraFadeOut;

    private void Awake()
    {
        DestroyImmediate(this);
        return;

        cameraController.onSetCameraMode += OnSetCameraMode;
    }

    void OnSetCameraMode(CameraMode.ModeId mode)
    {
        switch (mode)
        {
            case CameraMode.ModeId.FirstPerson:
                eventCameraFadeIn.Play(true);
                break;
            case CameraMode.ModeId.ThirdPerson:
                eventCameraFadeOut.Play(true);
                break;
            default:
                break;
        }
    }
}