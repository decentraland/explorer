using UnityEngine;

public class CameraStateFPS : CameraStateBase
{
    protected Vector3Variable cameraForward => CommonScriptableObjects.cameraForward;
    protected Vector3NullableVariable characterForward => CommonScriptableObjects.characterForward;

    public override void OnUpdate()
    {
        var xzPlaneForward = Vector3.Scale(cameraTransform.forward, new Vector3(1, 0, 1));

        characterForward.Set(xzPlaneForward);
    }
}
