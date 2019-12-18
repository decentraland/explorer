using Cinemachine;
using UnityEngine;

public class CameraStateFPS : CameraStateBase
{
    public CinemachineVirtualCameraBase fpsVirtualCam;

    protected Vector3Variable cameraForward => CommonScriptableObjects.cameraForward;
    protected Vector3NullableVariable characterForward => CommonScriptableObjects.characterForward;

    float localCameraDistForward;
    float localCameraDistY;
    bool hasLocalCameraUnselectValues = false;
    public override void OnSelect()
    {
        base.OnSelect();
    }

    public override void OnUnselect()
    {
        base.OnUnselect();
    }

    public override void OnUpdate()
    {
        var xzPlaneForward = Vector3.Scale(cameraTransform.forward, new Vector3(1, 0, 1));
        characterForward.Set(xzPlaneForward);
    }

    public override void OnSetRotation(CameraController.SetRotationPayload payload)
    {
        var eulerDir = Vector3.zero;

        if (payload.cameraTarget.HasValue)
        {
            var newPos = new Vector3(payload.x, payload.y, payload.z);
            var cameraTarget = payload.cameraTarget.GetValueOrDefault();
            var dirToLook = (cameraTarget - newPos);
            eulerDir = Quaternion.LookRotation(dirToLook).eulerAngles;
        }

        if (fpsVirtualCam is CinemachineVirtualCamera vcamera)
        {
            var pov = vcamera.GetCinemachineComponent<CinemachinePOV>();
            pov.m_HorizontalAxis.Value = eulerDir.y;
            pov.m_VerticalAxis.Value = eulerDir.x;
        }
    }
}
